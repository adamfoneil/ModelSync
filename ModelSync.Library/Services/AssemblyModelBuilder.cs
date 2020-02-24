using AO.DbSchema.Attributes;
using ModelSync.Library.Extensions;
using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using ModelSync.Library.Models.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ModelSync.Library.Services
{
    /// <summary>
    /// look back to https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Postulate/PostulateDbProvider.cs
    /// for example/inspiration
    /// </summary>
    public class AssemblyModelBuilder : IAssemblyModelBuilder
    {
        public DataModel GetDataModel(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {           
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract);
            var typeTableMap = GetTypeTableMap(types, defaultSchema, defaultIdentityColumn);
            DataModel result = GetDataModelInner(typeTableMap, defaultSchema, defaultIdentityColumn);
            return result;
        }

        public static Table GetTableFromType<T>(string defaultSchema, string defaultIdentityColumn)
        {
            return GetTableFromType(typeof(T), defaultSchema, defaultIdentityColumn);
        }

        public static DataModel GetDataModelFromTypes(IEnumerable<Type> modelTypes, string defaultSchema, string defaultIdentityColumn)
        {
            var typeTableMap = GetTypeTableMap(modelTypes, defaultSchema, defaultIdentityColumn);
            return GetDataModelInner(typeTableMap, defaultSchema, defaultIdentityColumn);
        }

        private static Dictionary<Type, string> DataTypes => GetSupportedTypes();

        private static Dictionary<Type, string> GetSupportedTypes()
        {
            var nullableBaseTypes = new Dictionary<Type, string>()
            {
                { typeof(int), "int" },
                { typeof(long), "bigint" },
                { typeof(short), "smallint" },
                { typeof(byte), "tinyint" },
                { typeof(DateTime), "datetime" },
                { typeof(bool), "bit" },
                { typeof(TimeSpan), "time" },
                { typeof(Guid), "uniqueidentifier" }
            };

            // help from https://stackoverflow.com/a/23402195/2023653
            IEnumerable<Type> getBothTypes(Type type)
            {
                yield return type;
                yield return typeof(Nullable<>).MakeGenericType(type);
            }

            var results = nullableBaseTypes.Select(kp => new
            {
                Types = getBothTypes(kp.Key),
                SqlType = kp.Value
            }).SelectMany(item => item.Types.Select(t => new
            {
                Type = t,
                item.SqlType
            }));

            var result = results.ToDictionary(item => item.Type, item => item.SqlType);

            // string is special in that it's already nullable
            result.Add(typeof(string), "nvarchar");

            return result;
        }

        private static DataModel GetDataModelInner(Dictionary<Type, Table> typeTableMap, string defaultSchema, string defaultIdentityColumn)
        {
            var result = new DataModel();

            bool referencedTypeIsMapped(PropertyInfo propertyInfo, IEnumerable<string> fkPropertyNamesInner)
            {
                var referenced = propertyInfo.GetCustomAttribute<ReferencesAttribute>();
                return (referenced != null) ?
                    typeTableMap.ContainsKey(referenced.PrimaryType) :
                    fkPropertyNamesInner.Contains(propertyInfo.Name);
            };

            result.Tables = typeTableMap.Select(kp => kp.Value).ToArray();

            result.Schemas = result.Tables
                .GroupBy(item => item.GetSchema(defaultSchema)).Select(grp => new Schema() { Name = grp.Key })
                .ToArray();

            // reverse the typeTableMap so we can get types from tables
            var tableTypeMap = typeTableMap.ToDictionary(item => item.Value, item => item.Key);

            // combine all table names with identity column to get properties that we can infer are FKs
            var fkProperties = result.Tables
                .Where(tbl => tbl.TryGetIdentityColumn(defaultIdentityColumn, out _))
                .Select(tbl => new ForeignKeyProperty
                {                    
                    PropertyName = tbl.GetBaseName() + tbl.GetIdentityColumn(defaultIdentityColumn),
                    PrimaryType = tableTypeMap[tbl]
                }).ToArray();

            var fkPropertyNames = fkProperties.Select(p => p.PropertyName);
            var fkPropertyMap = fkProperties.ToDictionary(item => item.PropertyName);

            result.ForeignKeys = typeTableMap
                .SelectMany(kp => ForeignKeyProperties(kp.Key, fkPropertyNames))
                .Where(pi => referencedTypeIsMapped(pi, fkPropertyNames))
                .Select(pi => ForeignKeyFromProperty(pi, typeTableMap, fkPropertyMap, defaultSchema, defaultIdentityColumn))
                .ToArray();

            return result;
        }

        private static IEnumerable<PropertyInfo> ForeignKeyProperties(Type type, IEnumerable<string> defaultFKNames)
        {
            return type.GetProperties().Where(pi => 
                defaultFKNames.Contains(pi.Name) || 
                pi.HasAttribute<ReferencesAttribute>(out _) || 
                pi.HasAttribute<ForeignKeyAttribute>(out _)); // needed for EF support, but doesn't work now
        }

        private static ForeignKey ForeignKeyFromProperty(PropertyInfo propertyInfo, Dictionary<Type, Table> typeTableMap, Dictionary<string, ForeignKeyProperty> fkPropertyMap, string defaultSchema, string defaultIdentityColumn)
        {
            var fk = propertyInfo.GetCustomAttribute<ReferencesAttribute>() ??
                ((fkPropertyMap.ContainsKey(propertyInfo.Name)) ?
                    new ReferencesAttribute(fkPropertyMap[propertyInfo.Name].PrimaryType) :
                    throw new Exception($"Couldn't infer foreign key info from {propertyInfo.DeclaringType.Name}.{propertyInfo.Name}")); 
                
            return new ForeignKey()
            {
                Name = $"FK_{GetTableConstraintName(propertyInfo.DeclaringType, defaultSchema)}_{propertyInfo.Name}",
                ReferencedTable = typeTableMap[fk.PrimaryType],
                Parent = typeTableMap[propertyInfo.DeclaringType],
                CascadeUpdate = false,
                CascadeDelete = fk.CascadeDelete,
                Columns = new ForeignKey.Column[] 
                { 
                    new ForeignKey.Column()
                    {
                        ReferencedName = FindIdentityProperty(fk.PrimaryType, defaultIdentityColumn).Name,
                        ReferencingName = propertyInfo.Name
                    }
                }
            };
        }

        private static Dictionary<Type, Table> GetTypeTableMap(IEnumerable<Type> modelTypes, string defaultSchema, string defaultIdentityColumn)
        {            
            var source = modelTypes.Select(t => new
            {
                Type = t,
                Table = GetTableFromType(t, defaultSchema, defaultIdentityColumn)
            });

            foreach (var item in source)
            {
                foreach (var col in item.Table.Columns) col.Parent = item.Table;
                foreach (var ndx in item.Table.Indexes) ndx.Parent = item.Table;
            }

            return source.ToDictionary(item => item.Type, item => item.Table);
        }

        private static ObjectName GetObjectName(Type type, string defaultSchema)
        {
            string name = (type.HasAttribute(out TableAttribute tableAttr)) ? tableAttr.Name : type.Name;

            string schema =
                (type.HasAttribute(out SchemaAttribute schemaAttr)) ? schemaAttr.Name :
                (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Schema)) ? tableAttr.Schema :
                defaultSchema;

            return new ObjectName() { Schema = schema, Name = name };
        }

        private static string GetTableName(Type type, string defaultSchema)
        {
            var objName = GetObjectName(type, defaultSchema);
            return $"{objName.Schema}.{objName.Name}";
        }

        private static string GetTableConstraintName(Type type, string defaultSchema)
        {
            var objName = GetObjectName(type, defaultSchema);
            return (objName.Schema.Equals(defaultSchema)) ? objName.Name : objName.Schema + objName.Name;
        }

        private static Table GetTableFromType(Type modelType, string defaultSchema, string defaultIdentityColumn)
        {
            string constraintName = GetTableConstraintName(modelType, defaultSchema);

            var idProperty = FindIdentityProperty(modelType, defaultIdentityColumn);
            if (idProperty == null) throw new Exception($"No 'Id` or [Identity] property found on class {modelType.Name}");

            IEnumerable<Index> getIndexes(Type type)
            {
                IndexType identityType = IndexType.PrimaryKey;
                
                var keyColumns = type.GetProperties().Where(pi => pi.HasAttribute<KeyAttribute>(out _));
                if (keyColumns.Any())
                {                    
                    if (idProperty != null && keyColumns.Contains(idProperty))
                    {
                        throw new Exception($"Model property {modelType.Name}.{idProperty.Name} can be either an [Identity] or [Key] property, but not both.");
                    }

                    identityType = IndexType.UniqueConstraint;

                    yield return new Index()
                    {
                        Type = IndexType.PrimaryKey,
                        Name = $"PK_{constraintName}",
                        Columns = keyColumns.Select((pi, index) => new Index.Column()
                        {
                            Name = pi.Name,
                            Order = index,
                            SortDirection = SortDirection.Ascending
                        })
                    };
                }

                var uniqueConstraints = modelType.GetCustomAttributes<UniqueConstraintAttribute>();
                foreach (var unique in uniqueConstraints)
                {
                    string columns = string.Join("_", unique.PropertyNames);

                    yield return new Index()
                    {
                        Name = $"U_{constraintName}_{columns}",
                        Type = IndexType.UniqueConstraint,
                        Columns = unique.PropertyNames
                            .Select((name, index) => new Index.Column() { Name = name, SortDirection = SortDirection.Ascending, Order = index })
                            .ToArray()
                    };
                }

                string identityIndexName = (!keyColumns.Any()) ? $"PK_{constraintName}" : $"U_{constraintName}_{idProperty.Name}";

                yield return new Index()
                {
                    Type = identityType,
                    Name = identityIndexName,
                    Columns = new Index.Column[] 
                    { 
                        new Index.Column() { Name = idProperty.Name, Order = 1, SortDirection = SortDirection.Ascending } 
                    }
                };
            }

            var result = new Table()
            {
                Name = GetTableName(modelType, defaultSchema),
                Columns = modelType
                    .GetProperties().Where(pi => DataTypes.ContainsKey(pi.PropertyType))
                    .Select(pi => GetColumnFromProperty(pi, defaultIdentityColumn))
                    .ToArray(),
                Indexes = getIndexes(modelType).ToArray()
            };

            foreach (var col in result.Columns) col.Parent = result;

            return result;
        }

        private static Column GetColumnFromProperty(PropertyInfo propertyInfo, string defaultIdentityColumn)
        {
            var result = new Column()
            {
                Name = propertyInfo.Name,
                IsNullable = propertyInfo.PropertyType.IsNullable() && !propertyInfo.HasAttribute<RequiredAttribute>(out _)
            };

            if (DataTypes.ContainsKey(propertyInfo.PropertyType))
            {
                result.DataType = DataTypes[propertyInfo.PropertyType];
            }

            SetColumnProperties(propertyInfo, result, defaultIdentityColumn);

            return result;
        }

        protected static void SetColumnProperties(PropertyInfo propertyInfo, Column column, string defaultIdentityColumn)
        {
            if (propertyInfo.PropertyType.Equals(typeof(string)))
            {
                if (propertyInfo.HasAttribute(out MaxLengthAttribute maxLength))
                {
                    column.DataType += $"({maxLength.Length})";
                }
                else
                {
                    column.DataType += "(max)";
                }
            }

            if (propertyInfo.HasAttribute(out ColumnAttribute columnAttr))
            {
                if (!string.IsNullOrEmpty(columnAttr.TypeName))
                {
                    column.DataType = columnAttr.TypeName;
                }
            }

            if (IsIdentity(propertyInfo, defaultIdentityColumn))
            {
                var idTypes = new Dictionary<Type, string>
                {
                    { typeof(int), "identity(1,1)" },
                    { typeof(long), "identity(1,1)" },
                    { typeof(Guid), "default NewId()" }
                };

                if (!idTypes.ContainsKey(propertyInfo.PropertyType)) throw new Exception($"Property {propertyInfo.DeclaringType.Name}.{propertyInfo.Name} uses unsupported identity type {propertyInfo.PropertyType.Name}");

                column.DataType += " " + idTypes[propertyInfo.PropertyType];
            }
        }

        private static PropertyInfo FindIdentityProperty(Type type, string defaultIdentityColumn)
        {
            return type.GetProperties().Where(pi => IsIdentity(pi, defaultIdentityColumn)).FirstOrDefault();
        }

        private static bool IsIdentity(PropertyInfo propertyInfo, string defaultIdentityColumn)
        {
            return 
                (propertyInfo.DeclaringType.HasAttribute(out IdentityAttribute idAttr) && idAttr.PropertyName.Equals(propertyInfo.Name)) ||
                (propertyInfo.Name.Equals(defaultIdentityColumn));
        }
    }
}

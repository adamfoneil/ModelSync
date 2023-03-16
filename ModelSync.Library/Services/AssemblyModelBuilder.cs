using AO.Models;
using AO.Models.Attributes;
using ModelSync.Extensions;
using ModelSync.Interfaces;
using ModelSync.Models;
using ModelSync.Models.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ModelSync.Services
{
    /// <summary>
    /// look back to https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Postulate/PostulateDbProvider.cs
    /// for example/inspiration
    /// </summary>
    public class AssemblyModelBuilder : IAssemblyModelBuilder
    {
        public DataModel GetDataModel(IEnumerable<Type> types, string defaultSchema, string defaultIdentityColumn)
        {
            var typeInfo = GetTypeTableMap(types, defaultSchema, defaultIdentityColumn);
            var result = GetDataModelInner(typeInfo.Tables, defaultSchema, defaultIdentityColumn);
            result.Errors = typeInfo.Errors;
            return result;
        }

        public DataModel GetDataModel(Assembly assembly, string modelClassNamespace, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && t.Namespace.StartsWith(modelClassNamespace));
            return GetDataModel(types, defaultSchema, defaultIdentityColumn);
        }

        public DataModel GetDataModel(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract);
            return GetDataModel(types, defaultSchema, defaultIdentityColumn);
        }


        public static Table GetTableFromType<T>(string defaultSchema, string defaultIdentityColumn)
        {
            return GetTableFromType(typeof(T), defaultSchema, defaultIdentityColumn);
        }

        public static DataModel GetDataModelFromTypes(IEnumerable<Type> modelTypes, string defaultSchema, string defaultIdentityColumn)
        {
            var typeInfo = GetTypeTableMap(modelTypes, defaultSchema, defaultIdentityColumn);
            var result = GetDataModelInner(typeInfo.Tables, defaultSchema, defaultIdentityColumn);
            result.Errors = typeInfo.Errors;
            return result;
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
                { typeof(decimal), "decimal" },
                { typeof(bool), "bit" },
                { typeof(TimeSpan), "time" },
                { typeof(Guid), "uniqueidentifier" },
                { typeof(double), "float" },
                { typeof(Single), "float" }
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
            result.Add(typeof(byte[]), "varbinary");

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

        private static TypeInfo GetTypeTableMap(IEnumerable<Type> modelTypes, string defaultSchema, string defaultIdentityColumn)
        {
            var errors = new Dictionary<Type, string>();

            var source = modelTypes.Select(t =>
            {
                try
                {
                    return new
                    {
                        Type = t,
                        Table = GetTableFromType(t, defaultSchema, defaultIdentityColumn),
                        Success = true
                    };
                }
                catch (Exception exc)
                {
                    errors.Add(t, exc.Message);
                    return new
                    {
                        Type = t,
                        Table = new Table(),
                        Success = false
                    };
                }
            });

            var successItems = source.Where(i => i.Success).ToArray();

            foreach (var item in successItems)
            {
                foreach (var col in item.Table.Columns) col.Parent = item.Table;
                foreach (var ndx in item.Table.Indexes) ndx.Parent = item.Table;
            }

            return new TypeInfo()
            {
                Tables = successItems.ToDictionary(item => item.Type, item => item.Table),
                Errors = errors
            };
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

            var result = new Table()
            {
                Name = GetTableName(modelType, defaultSchema),
                Columns = getColumns(modelType).ToArray(),
                Indexes = getIndexes(modelType).ToArray(),
                CheckConstraints = getChecks(modelType).ToArray()
            };

            foreach (var col in result.Columns) col.Parent = result;
            foreach (var ndx in result.Indexes) ndx.Parent = result;
            foreach (var chk in result.CheckConstraints) chk.Parent = result;

            return result;

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
                //var aa = modelType.HasAttribute<UniqueConstraintAttribute>(out _);
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

                var explicitIdentity = modelType.GetCustomAttribute<IdentityAttribute>();

                string identityIndexName =
                    (explicitIdentity != null && keyColumns.Any()) ? $"U_{constraintName}_{idProperty.Name}" :
                    (!keyColumns.Any()) ? $"PK_{constraintName}" :
                    $"U_{constraintName}_{idProperty.Name}";

                if (explicitIdentity != null && keyColumns.Any()) identityType = IndexType.UniqueConstraint;

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

            IEnumerable<Column> getColumns(Type type)
            {
                List<Column> results = new List<Column>();
                results.Add(GetColumnFromProperty(idProperty, defaultIdentityColumn));

                var properties = type
                        .GetProperties().Where(pi =>
                            (DataTypes.ContainsKey(pi.PropertyType) || pi.PropertyType.IsEnum || pi.PropertyType.IsNullableEnum()) &&
                            pi.CanWrite &&
                            !pi.HasAttribute<NotMappedAttribute>(out _))
                        .Select(pi => GetColumnFromProperty(pi, defaultIdentityColumn))
                        .Except(results);

                results.AddRange(properties);

                return results;
            }

            IEnumerable<CheckConstraint> getChecks(Type type)
            {
                List<CheckConstraint> results = new List<CheckConstraint>();

                var checks = type.GetCustomAttributes<CheckConstraintAttribute>();
                results.AddRange(checks.Select(attr => new CheckConstraint()
                {
                    Name = attr.ConstraintName,
                    Expression = attr.Expression
                }));

                return results;
            }
        }

        private static Column GetColumnFromProperty(PropertyInfo propertyInfo, string defaultIdentityColumn)
        {
            var result = new Column()
            {
                Name = propertyInfo.Name,
                IsNullable = propertyInfo.PropertyType.IsNullable() && !propertyInfo.HasAttribute<RequiredAttribute>(out _) && !propertyInfo.HasAttribute<KeyAttribute>(out _)
            };

            //if (DataTypes.ContainsKey(propertyInfo.PropertyType))
            //{
            //    result.DataType = DataTypes[propertyInfo.PropertyType];
            //}

            if (DataTypes.Any(x => x.Key.AssemblyQualifiedName == propertyInfo.PropertyType.AssemblyQualifiedName))
            {
                result.DataType = DataTypes.Where(x => x.Key.AssemblyQualifiedName == propertyInfo.PropertyType.AssemblyQualifiedName).FirstOrDefault().Value;
            }

            if (propertyInfo.PropertyType.IsEnum)
            {
                result.DataType = "int";
            }

            if (propertyInfo.PropertyType.IsNullableEnum())
            {
                result.DataType = "int";
                result.IsNullable = true;
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

            if (propertyInfo.HasAttribute(out CalculatedAttribute calcAttr))
            {
                column.IsCalculated = true;
                column.Expression = calcAttr.Expression;
            }

            if (propertyInfo.HasAttribute(out DefaultAttribute defaultAttr))
            {
                column.DefaultValue = defaultAttr.Expression;
            }

            if (IsIdentity(propertyInfo, defaultIdentityColumn))
            {
                var idTypes = new Dictionary<Type, string>
                {
                    { typeof(int), "identity(1,1)" },
                    { typeof(long), "identity(1,1)" },
                    { typeof(Guid), "default NewId()" }
                };

                var knownTypes = idTypes.Any(x => x.Key.AssemblyQualifiedName == propertyInfo.PropertyType.AssemblyQualifiedName);
                if (!knownTypes)
                    throw new Exception($"Property {propertyInfo.DeclaringType.Name}.{propertyInfo.Name} uses unsupported identity type {propertyInfo.PropertyType.Name}");

                //if (!idTypes.ContainsKey(propertyInfo.PropertyType)) throw new Exception($"Property {propertyInfo.DeclaringType.Name}.{propertyInfo.Name} uses unsupported identity type {propertyInfo.PropertyType.Name}");

                // column.TypeModifier = idTypes[propertyInfo.PropertyType];
                column.TypeModifier = idTypes.Where(x => x.Key.AssemblyQualifiedName == propertyInfo.PropertyType.AssemblyQualifiedName).FirstOrDefault().Value;
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
                (propertyInfo.Name.Equals(defaultIdentityColumn) && !propertyInfo.HasAttribute<KeyAttribute>(out _));
        }

        private class TypeInfo
        {
            public Dictionary<Type, Table> Tables { get; set; }
            public Dictionary<Type, string> Errors { get; set; }
        }
    }
}

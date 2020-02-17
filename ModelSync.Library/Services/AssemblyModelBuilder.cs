using AO.DbSchema.Attributes;
using ModelSync.Library.Extensions;
using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Services
{
    /// <summary>
    /// look back to https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Postulate/PostulateDbProvider.cs
    /// for example/inspiration
    /// </summary>
    public class AssemblyModelBuilder : IModelBuilder
    {
        private readonly Assembly _assembly;
        private readonly string _defaultSchema;
        private readonly string _defaultIdentityColumn;

        public AssemblyModelBuilder(Assembly assembly, string defaultSchema, string defaultIdentityColumn) 
        {
            _assembly = assembly;
            _defaultSchema = defaultSchema;
            _defaultIdentityColumn = defaultIdentityColumn;
        }

        public static Table GetTableFromType<T>(string defaultSchema, string defaultIdentityColumn)
        {
            return GetTableFromType(typeof(T), defaultSchema, defaultIdentityColumn);
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

        public async Task<DataModel> GetDataModelAsync()
        {
            var typeTableMap = GetTypeTableMap(_assembly, _defaultSchema, _defaultIdentityColumn);

            var result = new DataModel();
            result.Tables = typeTableMap.Select(kp => kp.Value);
            result.Schemas = result.Tables.GroupBy(item => item.GetSchema(_defaultSchema)).Select(grp => new Schema() { Name = grp.Key });
            result.ForeignKeys = typeTableMap.SelectMany(kp => ForeignKeyProperties(kp.Key)).Select(pi => ForeignKeyFromProperty(pi, typeTableMap));
            return await Task.FromResult(result);            
        }

        private static IEnumerable<PropertyInfo> ForeignKeyProperties(Type type)
        {
            return type.GetProperties().Where(pi => pi.HasAttribute<ReferencesAttribute>(out _));
        }

        private static ForeignKey ForeignKeyFromProperty(PropertyInfo pi, Dictionary<Type, Table> typeTableMap)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<Type, Table> GetTypeTableMap(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
        {
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract);

            var source = types.Select(t => new
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

        private static string GetTableName(Type type, string defaultSchema)
        {
            string name = (type.HasAttribute(out TableAttribute tableAttr)) ? tableAttr.Name : type.Name;

            string schema =
                (type.HasAttribute(out SchemaAttribute schemaAttr)) ? schemaAttr.Name :
                (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Schema)) ? tableAttr.Schema :
                defaultSchema;

            return $"{schema}.{name}";
        }

        private static Table GetTableFromType(Type modelType, string defaultSchema, string defaultIdentityColumn)
        {
            string constraintName = GetTableName(modelType, defaultSchema).Replace(".", string.Empty);

            var idProperty = FindIdentityProperty(modelType, defaultIdentityColumn);

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

                string identityIndexName = (!keyColumns.Any()) ? $"PK_{constraintName}" : $"U_{constraintName}_{idProperty.Name}";

                yield return new Index()
                {
                    Type = identityType,
                    Name = identityIndexName,
                    Columns = new Index.Column[] { new Index.Column() {  Name = idProperty.Name, Order = 1, SortDirection = SortDirection.Ascending } }
                };
            }

            return new Table()
            {
                Name = GetTableName(modelType, defaultSchema),
                Columns = modelType.GetProperties().Where(pi => DataTypes.ContainsKey(pi.PropertyType)).Select(pi => GetColumnFromProperty(pi, defaultIdentityColumn)),
                Indexes = getIndexes(modelType)
            };
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

                column.DataType += idTypes[propertyInfo.PropertyType];
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

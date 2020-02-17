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

        public AssemblyModelBuilder(Assembly assembly, string defaultSchema) 
        {
            _assembly = assembly;
            _defaultSchema = defaultSchema;
        }

        private Dictionary<Type, string> DataTypes => new Dictionary<Type, string>()
        {
            { typeof(int), "int" },
            { typeof(long), "bigint" },
            { typeof(string), "nvarchar" },
            { typeof(DateTime), "datetime" },
            { typeof(bool), "bit" }
        };

        public async Task<DataModel> GetDataModelAsync()
        {
            var typeTableMap = GetTypeTableMap(_assembly);

            var result = new DataModel();
            result.Tables = typeTableMap.Select(kp => kp.Value);
            result.Schemas = result.Tables.GroupBy(item => item.GetSchema(_defaultSchema)).Select(grp => new Schema() { Name = grp.Key });
            result.ForeignKeys = BuildForeignKeys(_assembly);
            return await Task.FromResult(result);            
        }

        private Dictionary<Type, Table> GetTypeTableMap(Assembly assembly)
        {
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract);

            string getTableName(Type type)
            {
                string name = (type.HasAttribute(out TableAttribute tableAttr)) ? tableAttr.Name : type.Name;

                string schema =
                    (type.HasAttribute(out SchemaAttribute schemaAttr)) ? schemaAttr.Name :
                    (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Schema)) ? tableAttr.Schema :
                    _defaultSchema;

                return $"{schema}.{name}";
            };

            IEnumerable<Column> getColumns(Type type)
            {
                throw new NotImplementedException();
            }

            IEnumerable<Index> getIndexes(Type type)
            {
                throw new NotImplementedException();
            }

            var source = types.Select(t => new
            {
                Type = t,
                Table = new Table()
                {
                    Name = getTableName(t),
                    Columns = getColumns(t),
                    Indexes = getIndexes(t)
                }
            });

            foreach (var item in source)
            {
                foreach (var col in item.Table.Columns) col.Parent = item.Table;
                foreach (var ndx in item.Table.Indexes) ndx.Parent = item.Table;
            }

            return source.ToDictionary(item => item.Type, item => item.Table);
        }

        private static IEnumerable<ForeignKey> BuildForeignKeys(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        private Column GetColumnFromProperty(PropertyInfo propertyInfo)
        {
            var result = new Column()
            {
                Name = propertyInfo.Name,
                IsNullable = propertyInfo.PropertyType.IsNullable()
            };

            if (DataTypes.ContainsKey(propertyInfo.PropertyType))
            {
                result.DataType = DataTypes[propertyInfo.PropertyType];
            }

            SetColumnProperties(propertyInfo, result);

            return result;
        }

        protected void SetColumnProperties(PropertyInfo propertyInfo, Column column)
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
        }
    }
}

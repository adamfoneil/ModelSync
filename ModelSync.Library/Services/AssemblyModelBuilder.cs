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

        public AssemblyModelBuilder(Assembly assembly) 
        {
            _assembly = assembly;
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
            var result = new DataModel();
            result.Schemas = BuildSchemas(_assembly);
            result.Tables = BuildTables(_assembly);
            result.ForeignKeys = BuildForeignKeys(_assembly);
            return await Task.FromResult(result);            
        }

        private static IEnumerable<Schema> BuildSchemas(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ForeignKey> BuildForeignKeys(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Table> BuildTables(Assembly assembly)
        {
            var types = assembly.GetExportedTypes();

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

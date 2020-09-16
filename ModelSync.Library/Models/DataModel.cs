using ModelSync.Interfaces;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public partial class DataModel : IDataModel
    {
        public DataModel()
        {
            Schemas = Enumerable.Empty<Schema>();
            Tables = Enumerable.Empty<Table>();
            ForeignKeys = Enumerable.Empty<ForeignKey>();
        }

        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public Dictionary<Type, string> Errors { get; set; }

        public static async Task<DataModel> FromSqlServerAsync(IDbConnection connection)
        {
            var sqlServer = new SqlServerModelBuilder();
            return await sqlServer.GetDataModelAsync(connection);
        }

        public static DataModel FromAssembly(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            return new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
        }

        public static DataModel FromAssembly(string fileName, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var assembly = Assembly.LoadFrom(fileName);
            return FromAssembly(assembly, defaultSchema, defaultIdentityColumn);
        }
    }
}

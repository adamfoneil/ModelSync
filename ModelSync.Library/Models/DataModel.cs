using Microsoft.Data.SqlClient;
using ModelSync.Library.Services;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
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

        public static async Task<DataModel> FromSqlServerAsync(IDbConnection connection)
        {
            var sqlServer = new SqlServerModelBuilder(connection as SqlConnection);
            return await sqlServer.GetDataModelAsync();
        }

        public static async Task<DataModel> FromAssemblyAsync(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var builder = new AssemblyModelBuilder(assembly, defaultSchema, defaultIdentityColumn);
            return await builder.GetDataModelAsync();
        }

        public static async Task<DataModel> FromAssemblyAsync(string fileName, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var assembly = Assembly.LoadFrom(fileName);
            return await FromAssemblyAsync(assembly, defaultSchema, defaultIdentityColumn);
        }
    }
}

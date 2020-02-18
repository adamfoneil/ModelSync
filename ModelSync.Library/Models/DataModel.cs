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
        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public IEnumerable<Schema> GetSchemas()
        {
            return Schemas ?? Enumerable.Empty<Schema>();
        }

        public IEnumerable<Table> GetTables()
        {
            return Tables ?? Enumerable.Empty<Table>();
        }

        public IEnumerable<ForeignKey> GetForeignKeys()
        {
            return ForeignKeys ?? Enumerable.Empty<ForeignKey>();
        }

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

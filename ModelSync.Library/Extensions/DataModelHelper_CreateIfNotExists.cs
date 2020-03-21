using ModelSync.Library.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Extensions
{
    public static partial class DataModelHelper
    {
        public static async Task CreateIfNotExistsAsync(this IEnumerable<Type> modelTypes, IDbConnection connection, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var dataModel = AssemblyModelBuilder.GetDataModelFromTypes(modelTypes, defaultSchema, defaultIdentityColumn);
            var script = await dataModel.CreateIfNotExistsAsync(connection);
            await new SqlServerDialect().ExecuteAsync(connection, script);            
        }

        public static async Task CreateIfNotExistsAsync(this IEnumerable<Type> modelTypes, Func<IDbConnection> getConnection, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            using (var cn = getConnection.Invoke())
            {
                await CreateIfNotExistsAsync(modelTypes, cn, defaultSchema, defaultIdentityColumn);
            }
        }
    }
}

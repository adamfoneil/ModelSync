using ModelSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Abstract
{
    public abstract class ConnectionModelBuilder
    {
        protected readonly IDbConnection Connection;

        protected abstract Task<IEnumerable<Schema>> GetSchemasAsync();
        protected abstract Task<IEnumerable<Table>> GetTablesAsync();
        protected abstract Task<IEnumerable<ForeignKey>> GetForeignKeysAsync();

        public ConnectionModelBuilder(IDbConnection connection)
        {
            Connection = connection;
        }

        public async Task<DataModel> GetDataModelAsync()
        {
            var result = new DataModel();
            result.Schemas = await GetSchemasAsync();
            result.Tables = await GetTablesAsync();
            result.ForeignKeys = await GetForeignKeysAsync();
            return result;
        }
    }
}

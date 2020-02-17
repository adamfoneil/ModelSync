using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Services
{
    public class SqlServerModelBuilder : IModelBuilder
    {
        private readonly IDbConnection _connection;

        public SqlServerModelBuilder(IDbConnection connection)
        {
            _connection = connection;
        }

        public Task<DataModel> GetDataModelAsync()
        {
            throw new NotImplementedException();
        }
    }
}

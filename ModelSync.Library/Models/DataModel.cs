using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public class DataModel
    {
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public static Task<DataModel> FromConnectionAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public static DataModel FromAssembly(Assembly assembly, SqlDialect sqlDialect)
        {
            throw new NotImplementedException();
        }
    }
}

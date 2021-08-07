using ModelSync.Abstract;
using ModelSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class TableFunction : DbObject, IDefinable
    {
        public override ObjectType ObjectType => ObjectType.TableFunction;
        
        public string Definition { get; set; }

        public override IEnumerable<string> CreateStatements()
        {
            throw new NotImplementedException();
        }

        public override string DropStatement()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            throw new NotImplementedException();
        }

        public override (bool result, string comment) IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }
    }
}

using ModelSync.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class Trigger : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Trigger;

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

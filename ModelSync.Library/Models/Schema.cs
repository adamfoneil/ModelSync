using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{
    public class Schema : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Schema;
        public override bool IsDialectSpecific => false;

        public const string DefaultSchema = "dbo";

        public override string CreateStatement()
        {
            return $"CREATE SCHEMA <{Name}>";
        }

        public override string DropStatement()
        {
            return $"DROP SCHEMA <{Name}";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return dataModel.Tables.Where(t => t.GetSchema(DefaultSchema).Equals(this.Name));
        }

        public override bool IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }
    }
}

using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Library.Models
{
    public enum IndexType
    {
        PrimaryKey,
        UniqueIndex,
        UniqueConstraint
    }

    public class Index : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Index;

        public IndexType IndexType { get; set; }

        public IEnumerable<IndexColumn> Columns { get; set; }

        public string GetDefinition()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> CreateStatements(DbObject parentObject)
        {
            throw new NotImplementedException();
        }

        public override string DropStatement(DbObject parentObject)
        {
            return $"ALTER TABLE <{parentObject}> DROP INDEX "
        }

        public override IEnumerable<DbObject> GetDependencies(DataModel dataModel)
        {
            throw new NotImplementedException();
        }
    }
}

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
        public override bool HasSchema => false;
        public override ObjectType ObjectType => ObjectType.Index;

        public IndexType IndexType { get; set; }

        public IEnumerable<Column> Columns { get; set; }

        public string GetDefinition()
        {
            throw new NotImplementedException();
        }

        public override string CreateStatement()
        {
            throw new NotImplementedException();
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP INDEX <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            throw new NotImplementedException();
        }

        public class Column
        {
            public string Name { get; set; }
            public int Order { get; set; }
        }
    }
}

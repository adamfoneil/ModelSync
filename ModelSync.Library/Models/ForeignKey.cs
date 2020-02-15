using ModelSync.Library.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{
    public class ForeignKey : DbObject
    {        
        public override ObjectType ObjectType => ObjectType.ForeignKey;

        public DbObject ReferencedTable { get; set; }
        public IEnumerable<Column> Columns { get; set; }

        public override string CreateStatement()
        {
            string referencingColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencingName}>"));
            string referencedColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencedName}>"));
            return $"ALTER TABLE <{Parent}> ADD CONSTRAINT <{Name}> FOREIGN KEY ({referencingColumns}) REFERENCES <{ReferencedTable}> ({referencedColumns})";
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP CONSTRAINT <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return Enumerable.Empty<DbObject>();
        }

        public class Column
        {
            public string ReferencedName { get; set; }
            public string ReferencingName { get; set; }
        }
    }
}

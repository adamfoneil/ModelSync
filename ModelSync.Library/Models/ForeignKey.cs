using ModelSync.Library.Abstract;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public class ForeignKey : DbObject
    {        
        public override ObjectType ObjectType => ObjectType.ForeignKey;        

        public Table ReferencedTable { get; set; }        
        public bool CascadeDelete { get; set; }
        public bool CascadeUpdate { get; set; }
        public IEnumerable<Column> Columns { get; set; }

        public override string CreateStatement()
        {
            string referencingColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencingName}>"));
            string referencedColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencedName}>"));
            string result = $"ALTER TABLE <{Parent}> ADD CONSTRAINT <{Name}> FOREIGN KEY ({referencingColumns}) REFERENCES <{ReferencedTable}> ({referencedColumns})";
            if (CascadeDelete) result += " ON DELETE CASCADE";
            if (CascadeUpdate) result += " ON UPDATE CASCADE";
            return result;
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP CONSTRAINT <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return Enumerable.Empty<DbObject>();
        }

        public override bool IsAltered(DbObject @object, out string comment)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public class Column
        {
            public string ReferencedName { get; set; }
            public string ReferencingName { get; set; }
        }
    }
}

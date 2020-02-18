using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public class Table : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Table;        
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Index> Indexes { get; set; }
        public long RowCount { get; set; }

        public override string CreateStatement()
        {            
            List<string> members = new List<string>();
            members.AddRange(Columns.Select(col => col.GetDefinition()));
            members.AddRange((Indexes ?? Enumerable.Empty<Index>()).Select(ndx => ndx.GetDefinition()));

            string createMembers = string.Join(",\r\n", members.Select(member => "\t" + member));

            return $"CREATE TABLE <{Name}> (\r\n{createMembers}\r\n)";
        }

        public override string DropStatement()
        {
            return $"DROP TABLE <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return dataModel.ForeignKeys.Where(fk => fk.Parent.Equals(this));
        }

        public override bool IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}

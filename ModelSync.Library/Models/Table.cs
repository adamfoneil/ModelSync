using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{
    public class Table : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Table;
        public string Schema { get; set; }
        public string IdentityColumn { get; set; }

        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Index> Indexes { get; set; }

        public override IEnumerable<string> CreateStatements(DbObject parentObject)
        {
            List<string> members = new List<string>();
            members.AddRange(Columns.Select(col => col.GetDefinition()));
            members.AddRange(Indexes.Select(ndx => ndx.GetDefinition()));

            string createMembers = string.Join(",\r\n", members.Select(member => "\t" + member));

            yield return $"CREATE TABLE <{Name}> (\r\n{createMembers}\r\n)";
        }

        public override string DropStatement(DbObject parentObject)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DbObject> GetDependencies(DataModel dataModel)
        {
            throw new NotImplementedException();
        }
    }
}

using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{
    public class Table : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Table;        
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Index> Indexes { get; set; }

        public override string CreateStatement()
        {            
            List<string> members = new List<string>();
            members.AddRange(Columns.Select(col => col.GetDefinition()));
            members.AddRange(Indexes.Select(ndx => ndx.GetDefinition()));

            string createMembers = string.Join(",\r\n", members.Select(member => "\t" + member));

            return $"CREATE TABLE <{Name}> (\r\n{createMembers}\r\n)";
        }

        public override string DropStatement()
        {
            return $"DROP TABLE <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            // return foreign keys referencing this table
            throw new NotImplementedException();
        }

        public override bool IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }
    }
}

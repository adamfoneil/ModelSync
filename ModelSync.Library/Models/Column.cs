using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{    
    public class Column : DbObject
    {
        public string DataType { get; set; }        
        public IdentityType? IdentityType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsCalculated { get; set; }
        public string Expression { get; set; }

        public override ObjectType ObjectType => ObjectType.Column;

        public string GetDefinition()
        {
            string result = $"<{Name}>";
            if (IsCalculated)
            {
                return $"{result} {Expression}";
            }
            else
            {                
                string nullable = (IsNullable) ? " NULL" : " NOT NULL";
                string identity = (IdentityType.HasValue) ? $" %identity:{IdentityType}%" : string.Empty;
                return $"{result} {DataType}{identity}{nullable}";
            }            
        }        

        public override string CreateStatement()
        {
            return $"ALTER TABLE <{Parent}> ADD <{Name}> {GetDefinition()}";
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP COLUMN <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            var table = Parent as Table;
            if (table != null)
            {
                return table.Indexes.Where(ndx => ndx.Columns.Any(col => col.Name.Equals(Name)));
            }

            return Enumerable.Empty<DbObject>();
        }

        public override bool IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }
    }
}

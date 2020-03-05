using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{    
    public class Column : DbObject
    {
        public string DataType { get; set; }                
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
                string nullable = (IsNullable) ? "NULL" : "NOT NULL";                
                return $"{result} {DataType} {nullable}";
            }            
        }        

        public override string CreateStatement()
        {
            return $"ALTER TABLE <{Parent}> ADD {GetDefinition()}";
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
            var column = @object as Column;
            if (column != null)
            {
                if (!DataType.Equals(column.DataType)) return true;
                if (IsNullable != column.IsNullable) return true;
                if (IsCalculated != column.IsCalculated) return true;
                if (!Expression?.Equals(column?.Expression) ?? false) return true;
            }

            return false;
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{Parent}.{Name}";
        }
    }
}

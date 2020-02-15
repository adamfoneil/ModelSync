using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;

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
            return $"ALTER TABLE <{Parent}> ADD <{Name}> {GetDefinition()}";
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP COLUMN <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            // return this table's indexes that contain this column
            throw new NotImplementedException();
        }
    }
}

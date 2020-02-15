using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;

namespace ModelSync.Library.Models
{
    public class Column : DbObject
    {        
        public string DataType { get; set; }        
        public bool IsNullable { get; set; }
        public string Expression { get; set; }

        public override ObjectType ObjectType => ObjectType.Column;

        public string GetDefinition(bool isIdentity)
        {
            throw new NotImplementedException();
        }        

        public override string CreateStatement(DbObject parentObject)
        {
            return $"ALTER TABLE <{parentObject}> ADD <{Name}> {GetDefinition(false)}";
        }

        public override string DropStatement(DbObject parentObject)
        {
            return $"ALTER TABLE <{parentObject}> DROP COLUMN <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            // return this table's indexes that contain this column
            throw new NotImplementedException();
        }
    }
}

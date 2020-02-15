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

        public string GetDefinition()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> CreateStatements(DbObject parentObject)
        {
            yield return $"ALTER TABLE <{parentObject}> ADD <{Name}> {GetDefinition()}";
        }

        public override string DropStatement(DbObject parentObject)
        {
            return $"ALTER TABLE <{parentObject}> DROP COLUMN <{Name}>";
        }

        public override IEnumerable<DbObject> GetDependencies(DataModel dataModel)
        {
            throw new NotImplementedException();
        }
    }
}

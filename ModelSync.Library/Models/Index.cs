using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum IndexType
    {
        PrimaryKey,
        UniqueIndex,
        UniqueConstraint
    }

    public class Index : DbObject
    {        
        public override ObjectType ObjectType => ObjectType.Index;        
        
        public IndexType Type { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public int InternalId { get; set; }

        public override string CreateStatement()
        {
            string definition = GetDefinition();

            switch (Type)
            {
                case IndexType.UniqueIndex:
                    return $"CREATE {definition}";

                case IndexType.UniqueConstraint:                    
                case IndexType.PrimaryKey:
                    return $"ALTER TABLE <{Parent}> ADD {definition}";

                default:
                    throw new Exception($"Unrecognized index type {Type} on {Name}");
            }
        }

        public string GetDefinition()
        {
            string columnList = string.Join(", ", Columns.OrderBy(col => col.Order).Select(col => $"<{col.Name}> {((col.SortDirection == SortDirection.Ascending) ? "ASC" : "DESC")}"));            

            switch (Type)
            {
                case IndexType.UniqueIndex:
                    return $"INDEX <{Name}> ON <{Parent}> ({columnList})";

                case IndexType.UniqueConstraint:
                    return $"CONSTRAINT <{Name}> UNIQUE ({columnList})";

                case IndexType.PrimaryKey:
                    return $"CONSTRAINT <{Name}> PRIMARY KEY ({columnList})";

                default:
                    throw new Exception($"Unrecognized index type {Type} on {Name}");

            }
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP INDEX <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            // return
            throw new NotImplementedException();
        }

        public override bool IsAltered(DbObject @object)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public class Column
        {
            public string Name { get; set; }
            public int Order { get; set; }
            public SortDirection SortDirection { get; set; }
        }
    }
}

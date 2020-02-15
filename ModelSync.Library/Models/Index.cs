using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool IsClustered { get; set; }
        public IndexType Type { get; set; }

        public IEnumerable<Column> Columns { get; set; }

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
            string clustered = (IsClustered) ? "CLUSTERED" : "NONCLUSTERED";

            switch (Type)
            {
                case IndexType.UniqueIndex:
                    return $"{clustered} INDEX <{Name}> ON <{Parent}> ({columnList})";

                case IndexType.UniqueConstraint:
                    return $"CONSTRAINT <{Name}> UNIQUE {clustered} ({columnList})";

                case IndexType.PrimaryKey:
                    return $"CONSTRAINT <{Name}> PRIMARY KEY {clustered} ({columnList})";

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

        public class Column
        {
            public string Name { get; set; }
            public int Order { get; set; }
            public SortDirection SortDirection { get; set; }
        }
    }
}

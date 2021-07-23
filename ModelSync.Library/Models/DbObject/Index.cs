using ModelSync.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum IndexType
    {
        PrimaryKey = 1,
        UniqueIndex = 2,
        UniqueConstraint = 3,
        NonUnique = 4
    }

    public class Index : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Index;

        public IndexType Type { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public int InternalId { get; set; }

        public override IEnumerable<string> CreateStatements()
        {
            string definition = GetDefinition();

            switch (Type)
            {
                case IndexType.UniqueIndex:
                    yield return $"CREATE {definition}";
                    break;

                case IndexType.UniqueConstraint:
                case IndexType.PrimaryKey:
                    yield return $"ALTER TABLE <{Parent}> ADD {definition}";
                    break;

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
            return
                (Type == IndexType.UniqueIndex || Type == IndexType.NonUnique) ? $"DROP INDEX <{Name}> ON <{Parent}>" :
                (Type == IndexType.UniqueConstraint || Type == IndexType.PrimaryKey) ? $"ALTER TABLE <{Parent}> DROP CONSTRAINT <{Name}>" :
                throw new Exception($"Unrecognized index type {Type}");
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return (Type == IndexType.PrimaryKey || Type == IndexType.UniqueConstraint) ?
                dataModel.ForeignKeys.Where(fk => fk.ReferencedTable.Equals(this.Parent)) :
                Enumerable.Empty<DbObject>();
        }

        public override bool IsAltered(DbObject @object, out string comment)
        {
            var index = @object as Index;
            if (index != null)
            {
                var sourceCols = this.Columns.Select(col => col.Name).OrderBy(col => col).ToArray();
                var destCols = index.Columns.Select(col => col.Name).OrderBy(col => col).ToArray();
                if (!sourceCols.SequenceEqual(destCols))
                {
                    comment = string.Empty;

                    var modified = new[]
                    {
                        new { text = "Added", columns = sourceCols.Except(destCols) },
                        new { text = "Removed", columns = destCols.Except(sourceCols) }
                    };

                    comment = $"{index.Name}: " + string.Join(", ", modified
                        .Where(cols => cols.columns.Any())
                        .Select(cols => $"{cols.text}: {string.Join(", ", cols.columns)}"));

                    return true;
                }
            }

            comment = null;
            return false;
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            throw new NotImplementedException();
        }

        public class Column
        {
            public string Name { get; set; }
            public int Order { get; set; }
            public SortDirection SortDirection { get; set; }

            public override bool Equals(object obj)
            {
                var col = obj as Index.Column;
                return (col != null) ? col.Name.Equals(this.Name) : false;
            }

            public override int GetHashCode() => Name.GetHashCode();
        }
    }
}

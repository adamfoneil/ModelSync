using ModelSync.Abstract;
using ModelSync.Extensions;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class ForeignKey : DbObject
    {
        public override ObjectType ObjectType => ObjectType.ForeignKey;

        public Table ReferencedTable { get; set; }
        public bool CascadeDelete { get; set; }
        public bool CascadeUpdate { get; set; }
        public IEnumerable<Column> Columns { get; set; }

        public override IEnumerable<string> CreateStatements()
        {
            string referencingColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencingName}>"));
            string referencedColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencedName}>"));
            string result = $"ALTER TABLE <{Parent}> ADD CONSTRAINT <{Name}> FOREIGN KEY ({referencingColumns}) REFERENCES <{ReferencedTable}> ({referencedColumns})";
            if (CascadeDelete) result += " ON DELETE CASCADE";
            if (CascadeUpdate) result += " ON UPDATE CASCADE";
            yield return result;
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP CONSTRAINT <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return Enumerable.Empty<DbObject>();
        }        

        public override (bool result, string comment) IsAltered(DbObject @object)
        {
            var fk = @object as ForeignKey;
            if (fk == null)
            {                
                return (false, null);
            }

            if (CascadeDelete != fk.CascadeDelete)
            {
                var comment = $"{Name} cascade delete: {fk.CascadeDelete} -> {CascadeDelete}";
                return (true, comment);
            }

            if (CascadeUpdate != fk.CascadeUpdate)
            {
                var comment  = $"{Name} cascade update: {fk.CascadeUpdate} -> {CascadeUpdate}";
                return (true, comment);
            }

            if (!Columns.OrderBy(col => col.ReferencingName).SequenceEqual(fk.Columns.OrderBy(col => col.ReferencingName)))
            {
                var added = Columns.Except(fk.Columns).Select(col => col.ReferencingName);
                var removed = fk.Columns.Except(Columns).Select(col => col.ReferencingName);

                var modified = new[]
                {
                    new { text = "Added", columns = added },
                    new { text = "Removed", columns = removed }
                };

                var comment = string.Join(", ", modified
                    .Where(cols => cols.columns.Any())
                    .Select(cols => $"{cols.text}: {string.Join(", ", cols.columns)}"));

                return (true, comment);
            }

            return (false, null);
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            var sqlServer = dialect as SqlServerDialect;
            if (sqlServer != null)
            {
                return await connection.RowExistsAsync("[sys].[foreign_keys] WHERE [name]=@name", new { Name });
            }

            throw new NotImplementedException();
        }

        public class Column
        {
            public string ReferencedName { get; set; }
            public string ReferencingName { get; set; }

            public override bool Equals(object obj)
            {
                var col = obj as Column;
                return (col != null) ?
                    ReferencedName.ToLower().Equals(col.ReferencedName.ToLower()) && ReferencingName.ToLower().Equals(col.ReferencingName.ToLower()) :
                    false;
            }

            public override int GetHashCode()
            {
                return $"{ReferencingName.ToLower()}.{ReferencedName.ToLower()}".GetHashCode();
            }
        }
    }
}

using ModelSync.Library.Abstract;
using ModelSync.Library.Extensions;
using ModelSync.Library.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public class Table : DbObject
    {
        public Table()
        {
            Columns = Enumerable.Empty<Column>();
            Indexes = Enumerable.Empty<Index>();
        }

        public override ObjectType ObjectType => ObjectType.Table;
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Index> Indexes { get; set; }
        public long RowCount { get; set; }

        public override string CreateStatement()
        {
            List<string> members = new List<string>();
            members.AddRange(Columns.Select(col => col.GetDefinition()));
            members.AddRange((Indexes ?? Enumerable.Empty<Index>()).Select(ndx => ndx.GetDefinition()));

            string createMembers = string.Join(",\r\n", members.Select(member => "\t" + member));

            return $"CREATE TABLE <{Name}> (\r\n{createMembers}\r\n)";
        }

        public override string DropStatement()
        {
            return $"DROP TABLE <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return dataModel.ForeignKeys.Where(fk => fk.ReferencedTable.Equals(this));
        }

        public override bool IsAltered(DbObject @object, out string comment)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            var sqlServer = dialect as SqlServerDialect;
            if (sqlServer != null)
            {
                return await connection.RowExistsAsync(
                    "[sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@name",
                    new { schema = GetSchema("dbo"), name = GetBaseName() });
            }

            throw new NotImplementedException();
        }

        [JsonIgnore]
        public Dictionary<string, Column> ColumnDictionary
        {
            get { return Columns.ToDictionary(row => row.Name); }
        }

        public bool TryGetIdentityColumn(string defaultIdentityColumn, out string identityColumn)
        {
            try
            {
                identityColumn = GetIdentityColumn(defaultIdentityColumn);
                return true;
            }
            catch
            {
                identityColumn = null;
                return false;
            }
        }

        public string GetIdentityColumn(string defaultIdentityColumn)
        {
            Func<Index, bool>[] searchIndexes = new Func<Index, bool>[]
            {
                (ndx) => ndx.Columns.Select(col => col.Name).SequenceEqual(new string[] { defaultIdentityColumn }),
                (ndx) => ndx.Columns.Count() == 1
            };

            foreach (var func in searchIndexes)
            {
                var identityIndex = Indexes.FirstOrDefault(ndx => func.Invoke(ndx));
                if (identityIndex != null) return identityIndex.Columns.First().Name;
            }

            throw new Exception($"Table {Name} has no identity column.");
        }
    }
}

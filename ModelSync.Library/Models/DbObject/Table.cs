using ModelSync.Abstract;
using ModelSync.Extensions;
using ModelSync.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class Table : DbObject
    {
        public Table()
        {
            Columns = Enumerable.Empty<Column>();
            Indexes = Enumerable.Empty<Index>();
            CheckConstraints = Enumerable.Empty<CheckConstraint>();
        }

        public override ObjectType ObjectType => ObjectType.Table;
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Index> Indexes { get; set; }
        public IEnumerable<CheckConstraint> CheckConstraints { get; set; }
        public long RowCount { get; set; }

        public override IEnumerable<string> CreateStatements()
        {
            List<string> members = new List<string>();
            members.AddRange(Columns.Select(col => col.GetDefinition()));
            members.AddRange(CheckConstraints.Select(chk => chk.GetDefinition()));
            members.AddRange((Indexes ?? Enumerable.Empty<Index>()).Select(ndx => ndx.GetDefinition()));

            string createMembers = string.Join(",\r\n", members.Select(member => "\t" + member));

            yield return $"CREATE TABLE <{Name}> (\r\n{createMembers}\r\n)";
        }

        public override string DropStatement()
        {
            return $"DROP TABLE <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return dataModel.ForeignKeys.Where(fk => fk.ReferencedTable.Equals(this));
        }

        public override (bool result, string comment) IsAltered(DbObject @object)
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

        public bool IsIdentityColumn(string name, string defaultIdentityColumn)
        {
            try
            {
                string identityCol = GetIdentityColumn(defaultIdentityColumn);
                return name.ToLower().Equals(identityCol.ToLower());
            }
            catch
            {
                return false;
            }
        }
    }
}

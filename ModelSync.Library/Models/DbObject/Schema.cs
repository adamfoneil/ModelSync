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
    public class Schema : DbObject
    {
        public override ObjectType ObjectType => ObjectType.Schema;

        public const string DefaultSchema = "dbo";

        public override IEnumerable<string> CreateStatements()
        {
            yield return $"CREATE SCHEMA <{Name}>";
        }

        public override string DropStatement()
        {
            return $"DROP SCHEMA <{Name}";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            return dataModel.Tables.Where(t => t.GetSchema(DefaultSchema).Equals(this.Name));
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
                return await connection.RowExistsAsync("[sys].[schemas] WHERE [Name]=@name", new { Name });
            }

            throw new NotImplementedException();
        }
    }
}

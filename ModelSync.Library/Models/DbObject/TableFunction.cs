using ModelSync.Abstract;
using ModelSync.Extensions;
using ModelSync.Interfaces;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class TableFunction : DbObject, IDefinable
    {
        public override ObjectType ObjectType => ObjectType.TableFunction;

        public string Definition { get; set; }

        public override IEnumerable<string> CreateStatements()
        {
            yield return Definition;
        }

        public override string DropStatement() => $"DROP FUNCTION <{Name}>";

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            if (dialect is SqlServerDialect)
            {
                return await connection.RowExistsAsync(
                    @"[sys].[objects] [o] INNER JOIN [sys].[sql_modules] [m] ON [o].[object_id]=[m].[object_id] 
                    WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@name AND [o].[type]='TF'",
                    new { schema = GetSchema("dbo"), name = GetBaseName() });
            }

            throw new NotImplementedException();
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel) => Enumerable.Empty<DbObject>();

        public override (bool result, string comment) IsAltered(DbObject @object)
        {
            if (@object is TableFunction function)
            {
                return (!function.Definition.Equals(Definition), null);
            }

            return (false, null);
        }
    }
}

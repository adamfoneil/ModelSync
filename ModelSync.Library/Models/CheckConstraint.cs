using Dapper;
using ModelSync.Abstract;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class CheckConstraint : DbObject
    {
        public override ObjectType ObjectType => ObjectType.CheckConstraint;
        
        public string Expression { get; set; }

        public string GetDefinition() => $"CONSTRAINT <{Name}> CHECK {Expression}";

        public override IEnumerable<string> CreateStatements() => new string[]
        {            
            $"ALTER TABLE <{Parent}> ADD {GetDefinition()}"
        };

        public override string DropStatement() => $"ALTER TABLE <{Parent}> DROP CONSTRAINT <{Name}>";
        
        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            var result = await connection.QuerySingleOrDefaultAsync<int>(
                @"SELECT 1 FROM [sys].[check_constraints] [ck] WHERE [ck].[name]=@name", 
                new { name = Name });

            return result == 1;
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel) => Enumerable.Empty<DbObject>();
        
        public override bool IsAltered(DbObject @object, out string comment)
        {           
            if (@object is CheckConstraint check)
            {
                comment = $"check rule changed from {Expression} to {check.Expression}";
                return check.Name.ToLower().Equals(Name.ToLower()) && !Expression.ToLower().Equals(check.Expression.ToLower());
            }

            comment = null;
            return false;
        }
    }
}

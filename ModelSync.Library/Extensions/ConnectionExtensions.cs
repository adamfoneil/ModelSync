using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Extensions
{
    internal static class ConnectionExtensions
    {
        public static async Task<bool> RowExistsAsync(this IDbConnection connection, string fromWhere, object parameters = null, IDbTransaction transaction = null)
        {
            return ((await connection.QueryFirstOrDefaultAsync<int?>($"SELECT 1 FROM {fromWhere}", parameters, transaction) ?? 0) == 1);
        }
    }
}

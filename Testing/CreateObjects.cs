using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Abstract;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using SqlServer.LocalDb;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Testing.Models;

namespace Testing
{
    [TestClass]
    public class CreateObjects
    {
        [TestMethod]
        public void CreateIfNotExists()
        {
            using (var cn = GetConnection())
            {
                DropTableIfExists(cn, "dbo.ActionItem2").Wait();
                DropTableIfExists(cn, "dbo.Employee").Wait();
            }

            DataModel.CreateTablesAsync(new[]
            {
                typeof(Employee),
                typeof(ActionItem2)
            }, GetConnection).Wait();

            using (var cn = GetConnection())
            {
                Assert.IsTrue(ObjectExistsAsync(cn, new Table() { Name = "dbo.ActionItem2" }).Result);
                Assert.IsTrue(ObjectExistsAsync(cn, new Table() { Name = "dbo.Employee" }).Result);
                Assert.IsTrue(ObjectExistsAsync(cn, new ForeignKey() { Name = "FK_ActionItem2_EmployeeId" }).Result);
            }
        }

        private async Task<bool> ObjectExistsAsync(SqlConnection cn, DbObject dbObject)
        {
            return await dbObject.ExistsAsync(cn, new SqlServerDialect());
        }

        private async Task DropTableIfExists(SqlConnection cn, string tableName)
        {
            if (await ObjectExistsAsync(cn, new Table() { Name = tableName }))
            {
                cn.Execute($"DROP TABLE {tableName}");
            }
        }

        private SqlConnection GetConnection() => LocalDb.GetConnection("ModelSync");
    }
}

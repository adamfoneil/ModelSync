using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Services;
using SqlServer.LocalDb;

namespace Testing
{
    /// <summary>
    /// I'm using some local databases not in source control, sorry
    /// </summary>
    [TestClass]
    public class SqlModelBuilder
    {
        [TestMethod]
        public void BuildHs5Model()
        {
            using (var cn = LocalDb.GetConnection("Hs5"))
            {
                var model = new SqlServerModelBuilder().GetDataModelAsync(cn).Result;
            }
        }

        [TestMethod]
        public void BuildAerieHubModel()
        {
            using (var cn = new SqlConnection("Data Source=localhost;Database=AerieLib3;Integrated Security=true"))
            {
                var model = new SqlServerModelBuilder().GetDataModelAsync(cn).Result;
            }
        }
    }
}

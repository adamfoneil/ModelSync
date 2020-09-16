using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Services;
using SqlServer.LocalDb;

namespace Testing
{
    [TestClass]
    public class SqlModelBuilder
    {
        [TestMethod]
        public void BuildSampleModel()
        {
            using (var cn = LocalDb.GetConnection("Hs5"))
            {
                var model = new SqlServerModelBuilder().GetDataModelAsync(cn).Result;
            }
        }
    }
}

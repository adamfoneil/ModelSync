using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using ModelSync.Services;

namespace Testing
{
    [TestClass]
    public class EFAssemblySources
    {
        [TestMethod]
        public void Netstandard()
        {
            var a = GetAssembly("Hs5.Database.dll");
            var builder = new AssemblyModelBuilder();
            var model = builder.GetDataModel(a);
            var output = DataModel.FromJson("Hs5Model.json");
            Assert.IsTrue(model.Equals(output));
        }

        [TestMethod]
        public void Framework()
        {
        }

        [TestMethod]
        public void Core()
        {
        }
    }
}

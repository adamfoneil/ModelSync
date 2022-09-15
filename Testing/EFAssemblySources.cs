using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using ModelSync.Services;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Testing
{
    [TestClass]
    public class EFAssemblySources
    {
        [TestMethod]
        public void Netstandard()
        {
            var a = Assembly.LoadFrom("EFAssemblies\\netstandard\\SampleModel.dll");
            var builder = new AssemblyModelBuilder();
            var model = builder.GetDataModel(a);
            //var json = model.ToJson();
            //File.WriteAllText("EFAssemblies\\netstandard\\SampleModel.json", json);
            
            var output = DataModel.FromJson(File.ReadAllText("EFAssemblies\\netstandard\\SampleModel.json"));
            //Assert.IsTrue(model.Equals(output));
            
        }

        [TestMethod]
        public void Framework()
        {
            var assembly = Assembly.LoadFrom("EFAssemblies\\framework\\EFSample.dll");
            var model = new AssemblyModelBuilder().GetDataModel(assembly, "dbo", "ID");

            var json = model.ToJson();
            File.WriteAllText("EFAssemblies\\framework\\EFSample.dll", json);

            var output = DataModel.FromJson(json);

        }

        [TestMethod]
        public void Core()
        {
            var assembly = Assembly.LoadFrom("EFAssemblies\\Core\\UsmanModels.dll");
            var model = new AssemblyModelBuilder().GetDataModel(assembly,"dbo","ID");

            var json = model.ToJson();
            File.WriteAllText("EFAssemblies\\core\\UsmanModel.json", json);

            var output = DataModel.FromJsonFile("EFAssemblies\\core\\UsmanModel.json");
            //Assert.IsTrue(model.Equals(output));
        }
    }
}

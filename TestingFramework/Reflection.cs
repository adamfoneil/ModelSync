using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.App.Helpers;
using ModelSync.Library.Services;

namespace TestingFramework
{
    [TestClass]
    public class Reflection
    {
        [TestMethod]
        public void LoadGinsengModels()
        {
            // note: AssemblyHelper is not in the source of this repo -- this test is internal only
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyHelper.LoadReflectionOnlyDependencies;
            var assembly = Assembly.ReflectionOnlyLoadFrom(@"C:\Users\Adam\Source\Repos\Ginseng8\Ginseng8.Models\bin\Debug\netstandard2.0\Ginseng.Models.dll");
            var dataModel = new AssemblyModelBuilder().GetDataModel(assembly);
        }
    }

}

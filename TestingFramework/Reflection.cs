using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.App.Helpers;
using ModelSync.Services;
using System;
using System.Reflection;

namespace TestingFramework
{
    [TestClass]
    public class Reflection
    {
        [TestMethod]
        public void LoadGinsengModels()
        {
            // note: AssemblyHelper is not in the source of this repo -- this test is internal only
            //Assembly.Load("System.ComponentModel.Annotations, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyHelper.LoadReflectionOnlyDependencies;
            var assembly = Assembly.ReflectionOnlyLoadFrom(@"C:\Users\Adam\Source\Repos\Ginseng8\Ginseng8.Models\bin\Debug\netstandard2.0\Ginseng.Models.dll");
            var dataModel = new AssemblyModelBuilder().GetDataModel(assembly);
        }
    }

}

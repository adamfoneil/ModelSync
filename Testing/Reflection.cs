using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Testing
{
    [TestClass]
    public class Reflection
    {
        [TestMethod]
        public void LoadSampleModel()
        {
            var asm = Assembly.LoadFile(@"C:\Users\Adam\Source\Repos\ModelSync.WinForms\SampleModel\bin\Debug\netstandard2.0\SampleModel.dll");
            var model = new AssemblyModelBuilder().GetDataModel(asm);
        }
    }
}

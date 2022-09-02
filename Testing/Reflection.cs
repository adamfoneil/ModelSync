using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Services;
using System.Reflection;

namespace Testing
{
    [TestClass]
    public class Reflection
    {
        [TestMethod]
        public void LoadSampleModel()
        {
            var asm = Assembly.LoadFile(@"C:\Users\Adam\Source\Repos\ModelSync.WinForms\SampleModel\bin\Debug\netstandard2.0\SampleModel.dll");
            var model = new AOModelBuilder().GetDataModel(asm);
        }
    }
}

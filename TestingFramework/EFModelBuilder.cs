using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestingFramework
{
    [TestClass]
    public class EFDatabaseVersion
    {
        [TestMethod ]
        public void LoadEFAssemblies()
        {
            bool result = false;

            
            //using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            //{
            //    Byte[] assemblyData = new Byte[stream.Length];
            //    stream.Read(assemblyData, 0, assemblyData.Length);
            //    return Assembly.Load(assemblyData);
            //}
            Assert.IsTrue(result == true);
        }
    }
}

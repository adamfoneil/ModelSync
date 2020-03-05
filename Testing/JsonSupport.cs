using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using System;
using Testing.Models;

namespace Testing
{
    [TestClass]
    public class JsonSupport
    {
        [TestMethod]
        public void WriteJson()
        {
            var model = AssemblyModelBuilder.GetDataModelFromTypes(new Type[]
            {
                typeof(Employee), typeof(ActionItem2)
            }, "dbo", "Id");

            string fileName = @"C:\users\adam\desktop\sampleModel.json";
            model.SaveJson(fileName);
            var load = DataModel.FromJsonFile(fileName);
        }
    }
}

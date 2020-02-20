using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using System;
using System.Linq;
using Testing.Models;

namespace Testing
{
    [TestClass]
    public class ObjectNaming
    {
        [TestMethod]
        public void VerifyTableNaming()
        {
            var dataModel = AssemblyModelBuilder.GetDataModelFromTypes(new Type[] 
            { 
                typeof(Employee),                 
                typeof(LogTable),
                typeof(UserProfile)
            }, "dbo", "Id");

            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "dbo.Employee" }));
            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "log.LogTable" }));
            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "dbo.AspNetUsers" }));
        }
    }
}

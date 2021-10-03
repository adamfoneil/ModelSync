using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using ModelSync.Services;
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
                typeof(UserProfile), // "simpler" table
                typeof(UserProfile2)
            }, "dbo", "Id");

            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "dbo.Employee" }));
            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "log.LogTable" }));
            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "dbo.AspNetUsers-simpler" }));
            Assert.IsTrue(dataModel.Tables.Contains(new Table() { Name = "dbo.AspNetUsers" }));
        }
    }
}

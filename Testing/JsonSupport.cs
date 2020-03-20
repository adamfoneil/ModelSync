using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using System;
using System.Linq;
using Testing.Helpers;
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

            Assert.IsTrue(model.ForeignKeys.Count() == 1);

            string fileName = @"C:\users\adam\desktop\sampleModel.json";
            model.SaveJson(fileName);
            var load = DataModel.FromJsonFile(fileName);

            Assert.IsTrue(load.ForeignKeys.Count() == 1);
        }

        [TestMethod]
        public void SaveAndLoadModel()
        {
            var model =
                ModelBuilder.BuildModel(
                    new ModelBuilder.TableSignature("Clinic", "Id:int", "Name", "Address", "City", "State"),
                    new ModelBuilder.TableSignature("Calendar", "Id:int", "ClinicId:int", "Name", "IsActive"),
                    new ModelBuilder.TableSignature("Item", "Id", "ClinicId:int", "Name", "IsActive"),
                    new ModelBuilder.TableSignature("Species", "Id", "ClinicId:int", "Name"),
                    new ModelBuilder.TableSignature("PointCapacity", "Id", "CalendarId:int", "Sex", "SpeciesId", "Maximum")
                );

            model.SaveJson(@"c:\users\adam\desktop\miniHsModel.json");

            var load = DataModel.FromJsonFile(@"c:\users\adam\desktop\miniHsModel.json");
        }

    }
}

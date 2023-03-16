using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using ModelSync.Services;
using System;
using System.IO;
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

            string fileName = GetFilename("sampleModel.json");
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

            string fileName = GetFilename("miniHsModel.json");

            model.SaveJson(fileName);

            var load = DataModel.FromJsonFile(fileName);
        }


        private static string GetFilename(string name) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModelBuilder", name);
    }
}

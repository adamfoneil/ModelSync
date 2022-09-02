using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Services;
using System;
using System.Linq;
using Testing.Models;
using ModelSync.Models;

namespace Testing
{
    [TestClass]
    public class AssemblyModels
    {
        [TestMethod]
        public void UserProfileModel()
        {
            var model = new AOModelBuilder().GetDataModel(new Type[]
            {
                typeof(UserProfile)
            }, "dbo", "Id");

            Assert.IsTrue(model.Tables.Count() == 1);
            Assert.IsTrue(model.TableDictionary["dbo.AspNetUsers"].Indexes.Count() == 2);
            Assert.IsTrue(model.TableDictionary["dbo.AspNetUsers"].Indexes.Any(ndx =>
            {
                return ndx.Name.Equals("U_AspNetUsers_UserId") &&
                ndx.Type == IndexType.UniqueConstraint &&
                ndx.Columns.SequenceEqual(new ModelSync.Models.Index.Column[]
                {
                    new ModelSync.Models.Index.Column() { Name = "UserId" }
                });
            }));
            Assert.IsTrue(model.TableDictionary["dbo.AspNetUsers"].Indexes.Any(ndx =>
            {
                return ndx.Name.Equals("PK_AspNetUsers") &&
                ndx.Type == IndexType.PrimaryKey &&
                ndx.Columns.SequenceEqual(new ModelSync.Models.Index.Column[]
                {
                    new ModelSync.Models.Index.Column() { Name = "Id" }
                });
            }));
        }
    }
}

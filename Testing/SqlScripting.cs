using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using System;
using System.Linq;
using Testing.Models;
using Index = ModelSync.Library.Models.Index;

namespace Testing
{
    [TestClass]
    public class SqlScripting
    {
        [TestMethod]
        public void CreateTable()
        {
            var table = new Table()
            {
                Name = "dbo.Employee",
                Columns = new Column[]
                {
                    new Column() { Name = "Id", DataType = "int identity(1,1)" },
                    new Column() { Name = "FirstName", DataType = "nvarchar(50)", IsNullable = false },
                    new Column() { Name = "LastName", DataType = "nvarchar(50)", IsNullable = false },
                    new Column() { Name = "HireDate", DataType = "date", IsNullable = true },
                    new Column() { Name = "TermDate", DataType = "date", IsNullable = true }
                },
                Indexes = new Index[]
                {
                    new Index() { Name = "PK_dboEmployee", Type = IndexType.PrimaryKey, Columns = new Index.Column[]
                        {
                            new Index.Column() { Name = "Id"}
                        }
                    }
                }
            };

            var sql = table.CreateStatement();
            var output = new SqlServerDialect().FormatStatement(sql);
        }

        [TestMethod]
        public void CreateTableFromClass()
        {
            var table = AssemblyModelBuilder.GetTableFromType<Employee>("dbo", "Id");
            var sql = table.CreateStatement();
            var output = new SqlServerDialect().FormatStatement(sql);
        }

        [TestMethod]
        public void CreateDataModelFromTypes()
        {
            var model = AssemblyModelBuilder.GetDataModelFromTypes(new Type[]
            {
                typeof(Employee),
                typeof(ActionItem)
            }, "dbo", "Id");
        }

        [TestMethod]
        public void ScriptParse()
        {
            var input =
                @"-- nullable True -> False
ALTER TABLE [dbo].[Widget] ALTER COLUMN [Name] nvarchar(50) NOT NULL

GO

-- Added: Name, Removed: Id

GO

ALTER TABLE [dbo].[Widget] DROP CONSTRAINT [PK_Widget]

GO

ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [PK_Widget] PRIMARY KEY ([Name] ASC)

";

            var output = new SqlServerDialect().ParseScript(input);

            Assert.IsTrue(output.SequenceEqual(new string[]
            {
                "\r\nALTER TABLE [dbo].[Widget] ALTER COLUMN [Name] nvarchar(50) NOT NULL",
                "ALTER TABLE [dbo].[Widget] DROP CONSTRAINT [PK_Widget]",
                "ALTER TABLE [dbo].[Widget] ADD CONSTRAINT [PK_Widget] PRIMARY KEY ([Name] ASC)"
            }));

        }
    }
}

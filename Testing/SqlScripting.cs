using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using Testing.Models;

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
            var output = new SqlServer().FormatStatement(sql);
        }

        [TestMethod]
        public void CreateTableFromClass()
        {
            var table = AssemblyModelBuilder.GetTableFromType<Employee>("dbo", "Id");
        }
    }
}

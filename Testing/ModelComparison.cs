using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using System;
using System.Linq;

namespace Testing
{
    [TestClass]
    public class ModelComparison
    {
        [TestMethod]
        public void CreateTable()
        {
            var table1 = BuildTable("table1", "FirstName", "LastName", "HireDate");
            var table2 = BuildTable("table2", "Jiminy", "Hambone", "Ecclesiast");
            var table3 = BuildTable("table3", "Yardicle", "Shorshana", "Mranzikis");

            var src = new DataModel()
            {
                Tables = new Table[] { table1, table2, table3 }
            };

            var dest = new DataModel()
            {
                Tables = new Table[] { table1, table3 }
            };

            var script = DataModel.Compare(src, dest);
            Assert.IsTrue(script.Contains(new ScriptAction()
            {
                Type = ActionType.Create,
                Object = table2,
                Commands = table2.CreateStatements()
            }));
        }

        [TestMethod]
        public void AddColumn()
        {
            var tableSrc = BuildTable("table1", "FirstName", "LastName", "HireDate", "WhateverDate");
            var table2 = BuildTable("table2", "Jiminy", "Hambone", "Ecclesiast");
            var tableDest = BuildTable("table1", "FirstName", "LastName", "HireDate");

            var src = new DataModel()
            {
                Tables = new Table[] { tableSrc, table2 }
            };

            var dest = new DataModel()
            {
                Tables = new Table[] { tableDest }
            };

            var script = DataModel.Compare(src, dest);
            var col = tableSrc.Columns.Last();
            Assert.IsTrue(script.Contains(new ScriptAction()
            {
                Type = ActionType.Create,
                Object = col,
                Commands = col.CreateStatements()
            }));

        }

        [TestMethod]
        public void DropColumn()
        {
            var tableSrc = BuildTable("table1", "FirstName", "LastName", "HireDate");
            var table2 = BuildTable("table2", "Jiminy", "Hambone", "Ecclesiast");
            var tableDest = BuildTable("table1", "FirstName", "LastName", "HireDate", "WhateverDate");

            var src = new DataModel()
            {
                Tables = new Table[] { tableSrc }
            };

            var dest = new DataModel()
            {
                Tables = new Table[] { tableDest, table2 }
            };

            var script = DataModel.Compare(src, dest);
            var col = tableDest.Columns.Last();
            Assert.IsTrue(script.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = col,
                Commands = col.DropStatements(dest)
            }));

            Assert.IsTrue(script.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = table2,
                Commands = table2.DropStatements(dest)
            }));
        }

        private Table BuildTable(string tableName, params string[] columnNames)
        {
            return new Table()
            {
                Name = tableName,
                Columns = columnNames.Select(col => new Column() { Name = col, DataType = "nvarchar(20)" })
            };
        }
    }
}

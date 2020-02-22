using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Library.Models;
using ModelSync.Library.Services;
using SqlServer.LocalDb;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

        [TestMethod]
        public void AddForeignKey()
        {
            var parentTable = BuildTable("table1", "this", "that", "other", "Id");
            var childTable = BuildTable("table2", "table1Id", "whatever", "tom", "dick", "harry");

            var fk = new ForeignKey()
            {
                Name = "FK_table2_table1",
                Parent = childTable,
                ReferencedTable = parentTable,
                Columns = new ForeignKey.Column[]
                {
                    new ForeignKey.Column() { ReferencingName = "table1Id", ReferencedName = "Id"}
                }
            };                

            var srcModel = new DataModel()
            {
                Tables = new Table[] { parentTable, childTable },
                ForeignKeys = new ForeignKey[] { fk }
            };

            var destModel = new DataModel()
            {
                Tables = new Table[] { parentTable, childTable }
            };

            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Create,
                Object = fk,
                Commands = fk.CreateStatements()
            }));
        }

        [TestMethod]
        public void DropForeignKey()
        {
            var parentTable = BuildTable("table1", "this", "that", "other", "Id");
            var childTable = BuildTable("table2", "table1Id", "whatever", "tom", "dick", "harry");

            var fk = new ForeignKey()
            {
                Name = "FK_table2_table1",
                Parent = childTable,
                ReferencedTable = parentTable,
                Columns = new ForeignKey.Column[]
                {
                    new ForeignKey.Column() { ReferencingName = "table1Id", ReferencedName = "Id"}
                }
            };

            var srcModel = new DataModel()
            {
                Tables = new Table[] { parentTable, childTable }
                
            };

            var destModel = new DataModel()
            {
                Tables = new Table[] { parentTable, childTable },
                ForeignKeys = new ForeignKey[] { fk }
            };

            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = fk,
                Commands = fk.DropStatements(destModel)
            }));
        }

        [TestMethod]
        public void DropTableWithoutRedundantFKDrop()
        {
            var parentTable = BuildTable("table1", "this", "that", "other", "Id");
            var childTable = BuildTable("table2", "table1Id", "whatever", "tom", "dick", "harry");

            var fk = new ForeignKey()
            {
                Name = "FK_table2_table1",
                Parent = childTable,
                ReferencedTable = parentTable,
                Columns = new ForeignKey.Column[]
                {
                    new ForeignKey.Column() { ReferencingName = "table1Id", ReferencedName = "Id"}
                }
            };

            var srcModel = new DataModel()
            {
                Tables = new Table[] { parentTable }
            };

            var destModel = new DataModel()
            {
                Tables = new Table[] { parentTable, childTable },
                ForeignKeys = new ForeignKey[] { fk }
            };

            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = childTable,
                Commands = childTable.DropStatements(destModel)
            }));
            Assert.IsTrue(diff.Count() == 1);
        }

        [TestMethod]
        public void DropTableWithoutRedundantIndexDrop()
        {
            var table1 = BuildTable("table1", "this", "that", "other", "Id");

            var index = new ModelSync.Library.Models.Index()
            {
                Name = "U_table1_this_that",
                Type = IndexType.UniqueConstraint,
                Columns = new ModelSync.Library.Models.Index.Column[]
                {
                    new ModelSync.Library.Models.Index.Column() { Name = "this"},
                    new ModelSync.Library.Models.Index.Column() { Name = "that" }
                }
            };

            table1.Indexes = new ModelSync.Library.Models.Index[] { index };

            var srcModel = new DataModel();
            var destModel = new DataModel() { Tables = new Table[] { table1 } };

            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = table1,
                Commands = table1.DropStatements(destModel)
            }));
            Assert.IsTrue(diff.Count() == 1);
        }

        [TestMethod]
        public void DropIndex()
        {
            var srcTable = BuildTable("table1", "this", "that", "other", "Id");            
            var destTable = BuildTable("table1", "this", "that", "other", "Id");

            var index = new ModelSync.Library.Models.Index()
            {
                Parent = destTable,
                Name = "U_table1_this_that",
                Type = IndexType.UniqueConstraint,
                Columns = new ModelSync.Library.Models.Index.Column[]
                {
                    new ModelSync.Library.Models.Index.Column() { Name = "this"},
                    new ModelSync.Library.Models.Index.Column() { Name = "that" }
                }
            };

            destTable.Indexes = new ModelSync.Library.Models.Index[] { index };

            var srcModel = new DataModel() { Tables = new Table[] { srcTable } };
            var destModel = new DataModel() { Tables = new Table[] { destTable } };
            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = index,
                Commands = index.DropStatements(destModel)
            }));
        }

        [TestMethod]
        public void AddIndex()
        {
            var srcTable = BuildTable("table1", "this", "that", "other", "Id");
            var destTable = BuildTable("table1", "this", "that", "other", "Id");

            var index = new ModelSync.Library.Models.Index()
            {
                Parent = destTable,
                Name = "U_table1_this_that",
                Type = IndexType.UniqueConstraint,
                Columns = new ModelSync.Library.Models.Index.Column[]
                {
                    new ModelSync.Library.Models.Index.Column() { Name = "this"},
                    new ModelSync.Library.Models.Index.Column() { Name = "that" }
                }
            };

            srcTable.Indexes = new ModelSync.Library.Models.Index[] { index };

            var srcModel = new DataModel() { Tables = new Table[] { srcTable } };
            var destModel = new DataModel() { Tables = new Table[] { destTable } };
            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Create,
                Object = index,
                Commands = index.CreateStatements()
            }));
        }

        [TestMethod]
        public void AlterColumn()
        {
            var srcTable = BuildTable("table1", "this:int", "that", "other", "Id");                        
            var destTable = BuildTable("table1", "this", "that", "other", "Id");

            var srcModel = new DataModel() { Tables = new Table[] { srcTable } };
            var destModel = new DataModel() { Tables = new Table[] { destTable } };
            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Alter,
                Object = srcTable.ColumnDictionary["this"],
                Commands = srcTable.ColumnDictionary["this"].CreateStatements()
            }));
        }

        [TestMethod]
        public void DropParentShouldDropFKs()
        {
            ForeignKey getForeignKey(Table table, string fkColumn, Table parentTable, string parentColumn)
            {
                var fk = new ForeignKey()
                {
                    Parent = table,
                    Name = $"FK_{table.Name}_{fkColumn}",
                    ReferencedTable = parentTable,
                    Columns = new ForeignKey.Column[]
                    {
                        new ForeignKey.Column() { ReferencingName = fkColumn, ReferencedName = parentColumn }
                    }
                };
                return fk;
            };

            var parentTbl = BuildTable("parent", "this", "that", "other", "Id");
            var child1 = BuildTable("child1", "parentId", "hello", "whatever", "chunga");            
            var child2 = BuildTable("child2", "parentId", "yowza", "plimza", "faruga");

            var srcModel = new DataModel()
            {
                Tables = new Table[] { child1, child2 },
                ForeignKeys = new ForeignKey[]
                {
                    getForeignKey(child1, "parentId", parentTbl, "Id"),
                    getForeignKey(child2, "parentId", parentTbl, "Id")
                }
            };

            var destModel = new DataModel()
            {
                Tables = new Table[] { parentTbl, child1, child2 },
                ForeignKeys = new ForeignKey[]
                {
                    getForeignKey(child1, "parentId", parentTbl, "Id"),
                    getForeignKey(child2, "parentId", parentTbl, "Id")
                }
            };

            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = parentTbl,
                Commands = new string[]
                {
                    "ALTER TABLE <child1> DROP CONSTRAINT <FK_child1_parentId>",
                    "ALTER TABLE <child2> DROP CONSTRAINT <FK_child2_parentId>",
                    "DROP TABLE <parent>"
                }
            }));

            var script = new SqlServerDialect().FormatScript(diff);
            Debug.Write(script);
        }

        [TestMethod]
        public void DropColumnShouldDropContainingIndex()
        {
            ModelSync.Library.Models.Index getIndex(Table parentTable)
            {
                return new ModelSync.Library.Models.Index()
                {
                    Parent = parentTable,
                    Name = "U_table1_ThisThat",
                    Columns = new ModelSync.Library.Models.Index.Column[]
                    {
                        new ModelSync.Library.Models.Index.Column() { Name = "this"},
                        new ModelSync.Library.Models.Index.Column() { Name = "that"}
                    }
                };
            };

            var srcTable = BuildTable("table1", "that", "other", "hello", "goodbye");
            srcTable.Indexes = new ModelSync.Library.Models.Index[] { getIndex(srcTable) };

            var destTable = BuildTable("table1", "this", "that", "other", "hello", "goodbye");
            destTable.Indexes = new ModelSync.Library.Models.Index[] { getIndex(destTable) };

            var srcModel = new DataModel() { Tables = new Table[] { srcTable } };
            var destModel = new DataModel() { Tables = new Table[] { destTable } };
            var diff = DataModel.Compare(srcModel, destModel);
            Assert.IsTrue(diff.Contains(new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = destTable.ColumnDictionary["this"],
                Commands = new string[]
                {
                    "ALTER TABLE <table1> DROP INDEX <U_table1_ThisThat>",
                    "ALTER TABLE <table1> DROP COLUMN <this>"
                }
            }));

            Assert.IsTrue(diff.Count() == 1);
        }

        [TestMethod]
        public void SampleModelCompare()
        {
            using (var cn = LocalDb.GetConnection("Hs5"))
            {
                var asm = Assembly.LoadFile(@"C:\Users\Adam\Source\Repos\ModelSync.WinForms\SampleModel\bin\Debug\netstandard2.0\SampleModel.dll");
                var srcModel = new AssemblyModelBuilder().GetDataModel(asm);
                var destModel = new SqlServerModelBuilder().GetDataModelAsync(cn).Result;
                var diff = DataModel.Compare(srcModel, destModel);
            }
        }

        private Table BuildTable(string tableName, params string[] columnNames)
        {
            Column columnFromName(Table table, string name)
            {
                var nameAndType = name.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                return (nameAndType.Length == 2) ?
                    new Column() { Name = nameAndType[0], DataType = nameAndType[1], Parent = table } :
                    new Column() { Name = name, DataType = "nvarchar(20)", Parent = table };                    
            };

            var result = new Table() { Name = tableName };
            result.Columns = columnNames.Select(col => columnFromName(result, col));

            return result;
        }
    }
}

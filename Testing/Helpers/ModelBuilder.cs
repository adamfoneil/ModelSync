using ModelSync.Library.Models;
using System;
using System.Linq;

namespace Testing.Helpers
{
    /// <summary>
    /// helper methods to shorthand model creation for testing purposes
    /// </summary>
    internal static class ModelBuilder
    {
        internal static Table BuildTable(string tableName, params string[] columnNames)
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

        internal class TableSignature
        {
            public TableSignature(string tableName, params string[] columns)
            {
                TableName = tableName;
                Columns = columns;
            }

            public string TableName { get; set; }
            public string[] Columns { get; set; }            
        }

        internal static DataModel BuildModel(params TableSignature[] tables)
        {
            var model = new DataModel();

            model.Tables = tables.Select(t => BuildTable(t.TableName, t.Columns));

            var pkCandidates = model.Tables.SelectMany(t => t.Columns.Select(col => new { Table = t, FKColumnName = t.Name + col.Name, PKColumnName = col.Name }));
            var allColumns = model.Tables.SelectMany(t => t.Columns.Select(col => new { TableName = t.Name, ColumnName = col.Name }));

            model.ForeignKeys = from pkCols in pkCandidates
                                join fkCols in allColumns on pkCols.FKColumnName equals fkCols.ColumnName
                                select new ForeignKey()
                                {
                                    Name = $"FK_{fkCols.TableName}_{fkCols.ColumnName}",
                                    Parent = model.TableDictionary[fkCols.TableName],
                                    ReferencedTable = pkCols.Table,
                                    Columns = new ForeignKey.Column[] 
                                    { 
                                        new ForeignKey.Column() { ReferencedName = pkCols.PKColumnName, ReferencingName = fkCols.ColumnName } 
                                    }
                                };

            return model;
        }
    }
}

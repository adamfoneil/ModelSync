using ModelSync.Abstract;
using ModelSync.Extensions;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public class Column : DbObject
    {
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsCalculated { get; set; }
        public string Expression { get; set; }
        public string DefaultValue { get; set; }
        public string TypeModifier { get; set; }

        /// <summary>
        /// true when you're merging a non-nullable column into a non-empty table
        /// </summary>
        public bool DefaultValueRequired { get; set; }

        public override ObjectType ObjectType => ObjectType.Column;

        public string GetDefinition(bool isCreating = true, bool? isNullable = null)
        {
            if (!isNullable.HasValue) isNullable = IsNullable;

            string result = $"<{Name}>";
            if (IsCalculated)
            {
                return $"{result} AS ({Expression})";
            }
            else
            {
                string nullable = (isNullable.Value) ? " NULL" : " NOT NULL";
                string defaultExp = (!string.IsNullOrEmpty(DefaultValue)) ? $" DEFAULT {SqlLiteral(DefaultValue)}" : string.Empty;
                string modifier = (isCreating) ? $" {TypeModifier} " : string.Empty;
                return $"{result} {DataType}{modifier}{nullable}{defaultExp}";
            }
        }

        public override IEnumerable<string> CreateStatements()
        {
            if (DefaultValueRequired && !string.IsNullOrEmpty(DefaultValue))
            {
                yield return $"ALTER TABLE <{Parent}> ADD {GetDefinition(isCreating: false)}";
            }
            else
            {
                if (DefaultValueRequired && string.IsNullOrEmpty(DefaultValue))
                {
                    yield return "-- adding non-nullable column to table with rows requires a default";
                }
                yield return $"ALTER TABLE <{Parent}> ADD {GetDefinition(isCreating: false)}";
            }
        }

        private string SqlLiteral(string input)
        {
            string result = input;

            string quote(string value)
            {
                return "'" + value + "'";
            };

            if (DataType.Contains("char"))
            {
                result = result.Replace("'", "''");
                result = quote(result);
            }

            if (DataType.Contains("date"))
            {
                result = quote(result);
            }

            return result;
        }

        public IEnumerable<string> AlterStatements(string comment, DataModel destModel)
        {
            if (partOfIndex(destModel))
            {
                var deps = GetDropDependencies(destModel).ToArray();
                foreach (var obj in deps) yield return obj.DropStatement();
            }

            if (!IsCalculated)
            {
                yield return $"-- {comment}\r\nALTER TABLE <{Parent}> ALTER COLUMN {GetDefinition(isCreating: false)}";
            }
            else
            {
                yield return $"-- {comment}\r\nALTER TABLE <{Parent}> DROP COLUMN <{Name}>";
                yield return $"ALTER TABLE <{Parent}> ADD {GetDefinition(isNullable: false)}";
            }            

            bool partOfIndex(DataModel dataModel)
            {
                if (dataModel.TableDictionary.ContainsKey(this.Parent.Name))
                {
                    var table = dataModel.TableDictionary[this.Parent.Name];
                    return table.Indexes.Any(ndx => ndx.Columns.Any(col => col.Name.Equals(Name)));
                }

                return false;
            }
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP COLUMN <{Name}>";
        }

        private IEnumerable<Index> GetIndexes(Table table) => table.Indexes.Where(ndx => ndx.Columns.Any(col => col.Name.Equals(Name)));

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            HashSet<Index> results = new HashSet<Index>();

            var table = Parent as Table;
            if (table != null)
            {
                foreach (var ndx in GetIndexes(table)) results.Add(ndx);

                if (dataModel.TableDictionary.ContainsKey(table.Name))
                {
                    var destTable = dataModel.TableDictionary[table.Name];
                    foreach (var ndx in GetIndexes(destTable)) results.Add(ndx);
                }
            }

            return results;
        }

        public override (bool result, string comment) IsAltered(DbObject @object)
        {            
            if (@object is Column column)
            {
                if (IsCalculated && column.IsCalculated)
                {
                    string thisExpr = prepExpression(Expression);
                    string thatExpr = prepExpression(column.Expression);

                    // for calculated columns, we care only about the expression diff
                    if (!thisExpr?.Equals(thatExpr) ?? false)
                    {
                        var comment = $"expression {column.Expression} -> {Expression}";
                        return (true, comment);
                    }
                }
                else
                {
                    string thisDataType = prepDataType(DataType);
                    string thatDataType = prepDataType(column.DataType);

                    if (!thisDataType.Equals(thatDataType))
                    {
                        var comment = $"data type {column.DataType} -> {DataType}";
                        return (true, comment);
                    }

                    if (IsNullable != column.IsNullable)
                    {
                        var comment = $"nullable {column.IsNullable} -> {IsNullable}";
                        return (true, comment);
                    }
                }

                // changes in calc status I think would be pretty uncommon
                if (IsCalculated != column.IsCalculated)
                {
                    var comment = $"calculated {column.IsCalculated} -> {IsCalculated}";
                    return (true, comment);
                }
            }

            return (false, null);

            string prepDataType(string input) => input.Replace(" ", string.Empty).ToLower();

            string prepExpression(string input)
            {
                string result = input;
                if (result?.StartsWith("(") ?? false) result = result.Substring(1);
                if (result?.EndsWith(")") ?? false) result = result.Substring(0, result.Length - 1);
                return result;
            }
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            var sqlServer = dialect as SqlServerDialect;
            if (sqlServer != null)
            {
                return await connection.RowExistsAsync(
                    @"[sys].[columns] [col]
	                INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
                    WHERE SCHEMA_NAME([t].[schema_id])=@schema AND [t].[name]=@tableName AND [col].[name]=@columnName",
                    new { schema = Parent.GetSchema("dbo"), tableName = Parent.GetBaseName(), columnName = Name });
            }

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{Parent}.{Name}";
        }
    }
}

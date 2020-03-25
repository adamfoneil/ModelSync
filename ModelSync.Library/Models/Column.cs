using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public class Column : DbObject
    {
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsCalculated { get; set; }
        public string Expression { get; set; }
        public string DefaultValue { get; set; }

        /// <summary>
        /// true when you're merging a non-nullable column into a non-empty table
        /// </summary>
        public bool DefaultValueRequired { get; set; }

        public override ObjectType ObjectType => ObjectType.Column;

        public string GetDefinition()
        {
            string result = $"<{Name}>";
            if (IsCalculated)
            {
                return $"{result} {Expression}";
            }
            else
            {
                string nullable = (IsNullable) ? "NULL" : "NOT NULL";
                return $"{result} {DataType} {nullable}";
            }
        }

        public override string CreateStatement()
        {
            return $"ALTER TABLE <{Parent}> ADD {GetDefinition()}";
        }

        public IEnumerable<string> AlterStatements(string comment)
        {
            yield return $"-- {comment}\r\nALTER TABLE <{Parent}> ALTER COLUMN {GetDefinition()}";
        }

        public override string DropStatement()
        {
            return $"ALTER TABLE <{Parent}> DROP COLUMN <{Name}>";
        }

        public override IEnumerable<DbObject> GetDropDependencies(DataModel dataModel)
        {
            var table = Parent as Table;
            if (table != null)
            {
                return table.Indexes.Where(ndx => ndx.Columns.Any(col => col.Name.Equals(Name)));
            }

            return Enumerable.Empty<DbObject>();
        }

        public override bool IsAltered(DbObject @object, out string comment)
        {
            string prepDataType(string input)
            {
                return input.Replace(" ", string.Empty).ToLower();
            };

            string prepExpression(string input)
            {
                string result = input;
                if (result?.StartsWith("(") ?? false) result = result.Substring(1);
                if (result?.EndsWith(")") ?? false) result = result.Substring(0, result.Length - 1);
                return result;
            }

            var column = @object as Column;
            if (column != null)
            {
                if (IsCalculated && column.IsCalculated)
                {
                    string thisExpr = prepExpression(Expression);
                    string thatExpr = prepExpression(column.Expression);

                    // for calculated columns, we care only about the expression diff
                    if (!thisExpr?.Equals(thatExpr) ?? false)
                    {
                        comment = $"expression {column.Expression} -> {Expression}";
                        return true;
                    }
                }
                else
                {
                    string thisDataType = prepDataType(DataType);
                    string thatDataType = prepDataType(column.DataType);

                    if (!thisDataType.Equals(thatDataType))
                    {
                        comment = $"data type {column.DataType} -> {DataType}";
                        return true;
                    }

                    if (IsNullable != column.IsNullable)
                    {
                        comment = $"nullable {column.IsNullable} -> {IsNullable}";
                        return true;
                    }
                }

                // changes in calc status I think would be pretty uncommon
                if (IsCalculated != column.IsCalculated)
                {
                    comment = $"calculated {column.IsCalculated} -> {IsCalculated}";
                    return true;
                }
            }

            comment = null;
            return false;
        }

        public override async Task<bool> ExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{Parent}.{Name}";
        }
    }
}

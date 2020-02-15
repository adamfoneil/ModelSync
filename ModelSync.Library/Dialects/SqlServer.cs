using ModelSync.Library.Abstract;
using ModelSync.Library.Extensions;
using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace ModelSync.Library.Dialects
{
    public class SqlServer : SqlDialect
    {
        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string BatchSeparator => "\r\nGO\r\n";

        public override Dictionary<Type, string> DataTypes => new Dictionary<Type, string>()
        {
            { typeof(int), "int" },
            { typeof(long), "bigint" },
            { typeof(string), "nvarchar" },
            { typeof(DateTime), "datetime" },
            { typeof(bool), "bit" }
        };

        public override Dictionary<IdentityType, string> IdentityTypes => throw new NotImplementedException();

        protected override void SetColumnProperties(PropertyInfo propertyInfo, Column column)
        {
            base.SetColumnProperties(propertyInfo, column);

            if (propertyInfo.PropertyType.Equals(typeof(string)))
            {
                if (propertyInfo.HasAttribute(out MaxLengthAttribute maxLength))
                {
                    column.DataType += $"({maxLength.Length})";
                }
                else
                {
                    column.DataType += "(max)";
                }
            }

            if (propertyInfo.HasAttribute(out ColumnAttribute columnAttr))
            {
                if (!string.IsNullOrEmpty(columnAttr.TypeName))
                {
                    column.DataType = columnAttr.TypeName;
                }
            }
        }
    }
}

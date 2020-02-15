using ModelSync.Library.Extensions;
using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModelSync.Library.Abstract
{
    public enum IdentityType
    {
        Int,
        Long,
        Guid
    }

    public abstract class SqlDialect
    {
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string BatchSeparator { get; }
        public abstract Dictionary<Type, string> DataTypes { get; }
        public abstract Dictionary<IdentityType, string> IdentityTypes { get; }

        /// <summary>
        /// override this to look for certain attributes or execute other logic to build out a Column 
        /// object in ways that aren't determined by Type alone        
        /// </summary>
        protected virtual void SetColumnProperties(PropertyInfo propertyInfo, Column column)
        {
            // do nothing by default
        }

        public Column GetColumnFromProperty(PropertyInfo propertyInfo)
        {
            var result = new Column()
            {
                Name = propertyInfo.Name,
                IsNullable = propertyInfo.PropertyType.IsNullable()
            };

            if (DataTypes.ContainsKey(propertyInfo.PropertyType))
            {
                result.DataType = DataTypes[propertyInfo.PropertyType];
            }

            SetColumnProperties(propertyInfo, result);

            return result;
        }

        public string FormatStatement(string statement)
        {
            string result = statement;
            var objectNames = Regex.Matches(statement, @"<([^>]+)>").OfType<Match>();
            foreach (var value in objectNames)
            {                
                result = result.Replace(value.Value, ApplyDelimiters(value.Value));
            }
            return result;
        }

        private string ApplyDelimiters(string objectName)
        {
            string result = objectName.Replace("<", string.Empty).Replace(">", string.Empty);
            var parts = result.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            return string.Join(".", parts.Select(part => $"{StartDelimiter}{part}{EndDelimiter}"));            
        }
    }
}

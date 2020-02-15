using System;
using System.Collections.Generic;
using System.Linq;
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
        public abstract Dictionary<string, string> DataTypes { get; }
        public abstract Dictionary<IdentityType, string> IdentityTypes { get; }

        /// <summary>
        /// default sizes of variable-length columns when no length is specified.
        /// Use the same key from DataTypes
        /// </summary>
        public abstract Dictionary<string, string> DefaultSizes { get; }

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

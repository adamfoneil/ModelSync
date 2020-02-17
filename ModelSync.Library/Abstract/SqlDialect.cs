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
        
        public abstract Dictionary<IdentityType, Func<string, string>> IdentitySyntax { get; }
        
        public string FormatStatement(string statement)
        {
            string result = statement;

            var identityTokens = Regex.Matches(result, @"%(id:[^%]+)%").OfType<Match>();
            foreach (var token in identityTokens)
            {

            }

            var objectNames = Regex.Matches(result, @"<([^>]+)>").OfType<Match>();
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

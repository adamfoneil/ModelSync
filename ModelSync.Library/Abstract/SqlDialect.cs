using ModelSync.Library.Models;
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
        
        public string FormatStatement(string statement)
        {
            string result = statement;

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

        public string FormatScript(IEnumerable<ScriptAction> scriptActions)
        {
            return string.Join("\r\n" + BatchSeparator + "\r\n", scriptActions.SelectMany(scr => scr.Commands.Select(cmd => FormatStatement(cmd)))) + "\r\n\r\n";
        }
    }
}

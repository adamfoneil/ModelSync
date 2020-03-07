using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public abstract string CommentStart { get; }
        
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

        public void Execute(IDbConnection connection, string script)
        {
            if (connection.State == ConnectionState.Closed) connection.Open();

            string[] commands = script
                .Split(new string[] { $"\r\n{BatchSeparator}\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !s.StartsWith(CommentStart))
                .ToArray();

            using (var txn = connection.BeginTransaction())
            {
                try
                {
                    foreach (string statement in commands)
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = statement;
                        cmd.Connection = connection;
                        cmd.Transaction = txn;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }

                txn.Commit();
            }
        }

        public async Task ExecuteAsync(IDbConnection connection, string script)
        {
            await Task.Run(() =>
            {
                Execute(connection, script);
            });
        }
    }
}

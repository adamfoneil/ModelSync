using ModelSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
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

        public void Execute(IDbConnection connection, IEnumerable<ScriptAction> scriptActions)
        {
            string script = FormatScript(scriptActions);
            Execute(connection, script);
        }

        public async Task ExecuteAsync(IDbConnection connection, IEnumerable<ScriptAction> scriptActions)
        {
            string script = FormatScript(scriptActions);
            await ExecuteAsync(connection, script);
        }

        public IEnumerable<string> ParseScript(string rawScript)
        {
            var commentsRemoved = string.Join("\r\n", rawScript
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => removeComment(line)));

            return commentsRemoved
                .Split(new string[] { BatchSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            string removeComment(string input)
            {
                int index = input.IndexOf(CommentStart);
                return (index > -1) ? input.Substring(0, index) : input;
            }
        }

        public void Execute(IDbConnection connection, string script)
        {
            if (connection.State == ConnectionState.Closed) connection.Open();

            var commands = ParseScript(script);

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
                    txn.Commit();
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }                
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

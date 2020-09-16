using ModelSync.Abstract;
using ModelSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Models
{
    public enum ActionType
    {
        Create,
        Alter,
        Drop
    }

    public class ScriptAction : IActionable
    {
        public ActionType Type { get; set; }
        public DbObject Object { get; set; }
        public IEnumerable<string> Commands { get; set; }
        public IEnumerable<string> CommandsWithoutComments() => Commands.SelectMany(cmd => cmd.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)).Where(line => !line.StartsWith("--"));

        public string ObjectName => Object.ToString();
        public ObjectType ObjectType => Object.ObjectType;

        public ExcludeAction GetExcludeAction() => new ExcludeAction()
        {
            Type = this.Type,
            ObjectType = this.Object.ObjectType,
            ObjectName = this.Object.ToString()
        };

        public override bool Equals(object obj)
        {
            var test = obj as ScriptAction;
            if (test != null)
            {
                return
                    test.Type == Type &&
                    test.Object.Equals(Object) &&
                    test.Commands.SequenceEqual(Commands);
            }

            var actionable = obj as IActionable;
            if (actionable != null)
            {
                return Type == actionable.Type && ObjectName.ToLower().Equals(actionable.ObjectName);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Type.ToString() + Object.ToString() + Commands.ToString()).GetHashCode();
        }
    }
}

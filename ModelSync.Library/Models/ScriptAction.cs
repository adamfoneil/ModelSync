using ModelSync.Library.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Models
{
    public enum ActionType
    {
        Create,
        Alter,
        Drop
    }

    public class ScriptAction
    {
        public ActionType Type { get; set; }
        public DbObject Object { get; set; }
        public IEnumerable<string> Commands { get; set; }

        public override bool Equals(object obj)
        {
            var test = obj as ScriptAction;
            return (test != null) ?
                test.Type == Type &&
                test.Object.Equals(Object) &&
                test.Commands.SequenceEqual(Commands) : false;
        }

        public override int GetHashCode()
        {
            return (Type.ToString() + Object.ToString() + Commands.ToString()).GetHashCode();
        }
    }
}

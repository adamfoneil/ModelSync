using ModelSync.Library.Abstract;

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
        public string Command { get; set; }
    }
}

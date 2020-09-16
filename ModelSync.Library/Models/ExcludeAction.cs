using ModelSync.Abstract;
using ModelSync.Interfaces;

namespace ModelSync.Models
{
    public class ExcludeAction : IActionable
    {
        public ActionType Type { get; set; }
        public ObjectType ObjectType { get; set; }
        public string ObjectName { get; set; }

        public override bool Equals(object obj)
        {
            var test = obj as ExcludeAction;
            return (test != null) ?
                Type == test.Type && ObjectType == test.ObjectType && ObjectName.Equals(test.ObjectName) :
                false;
        }

        public override int GetHashCode()
        {
            return (Type.ToString() + ObjectName).GetHashCode();
        }
    }
}

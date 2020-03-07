using ModelSync.Library.Abstract;
using ModelSync.Library.Models;

namespace ModelSync.Library.Interfaces
{
    public interface IActionable
    {
        ActionType Type { get; }
        ObjectType ObjectType { get; }
        string ObjectName { get; }
    }
}

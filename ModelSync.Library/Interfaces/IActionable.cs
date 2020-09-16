using ModelSync.Abstract;
using ModelSync.Models;

namespace ModelSync.Interfaces
{
    public interface IActionable
    {
        ActionType Type { get; }
        ObjectType ObjectType { get; }
        string ObjectName { get; }
    }
}

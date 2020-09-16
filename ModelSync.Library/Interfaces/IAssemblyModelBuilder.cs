using ModelSync.Models;
using System.Reflection;

namespace ModelSync.Interfaces
{
    public interface IAssemblyModelBuilder
    {
        DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn);
    }
}

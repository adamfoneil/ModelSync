using ModelSync.Library.Models;
using System.Reflection;

namespace ModelSync.Library.Interfaces
{
    public interface IAssemblyModelBuilder
    {
        DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn);
    }
}

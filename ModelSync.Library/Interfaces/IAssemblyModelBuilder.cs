using ModelSync.Library.Models;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Interfaces
{
    public interface IAssemblyModelBuilder
    {
        DataModel GetDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn);        
    }
}

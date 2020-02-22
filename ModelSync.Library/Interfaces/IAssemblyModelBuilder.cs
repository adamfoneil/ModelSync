using ModelSync.Library.Models;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Interfaces
{
    public interface IAssemblyModelBuilder
    {
        Task<DataModel> GetDataModelAsync(Assembly assembly, string defaultSchema, string defaultIdentityColumn);
    }
}

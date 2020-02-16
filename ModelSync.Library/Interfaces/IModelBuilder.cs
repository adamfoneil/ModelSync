using ModelSync.Library.Models;
using System.Threading.Tasks;

namespace ModelSync.Library.Interfaces
{
    public interface IModelBuilder
    {
        Task<DataModel> GetDataModelAsync();
    }
}

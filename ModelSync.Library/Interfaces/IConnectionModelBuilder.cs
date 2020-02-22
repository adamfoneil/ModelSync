using ModelSync.Library.Models;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Interfaces
{
    public interface IConnectionModelBuilder
    {
        Task<DataModel> GetDataModelAsync(IDbConnection connection);
    }
}

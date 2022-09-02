using ModelSync.Models;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Interfaces
{
    public interface IConnectionSource
    {
        Task<DataModel> GetDataModelAsync(IDbConnection connection);
    }
}

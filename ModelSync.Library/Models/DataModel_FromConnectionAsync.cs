using System;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public static Task<DataModel> FromConnectionAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}

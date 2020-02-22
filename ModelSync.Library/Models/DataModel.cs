using Microsoft.Data.SqlClient;
using ModelSync.Library.Services;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public DataModel()
        {
            Schemas = Enumerable.Empty<Schema>();
            Tables = Enumerable.Empty<Table>();
            ForeignKeys = Enumerable.Empty<ForeignKey>();
        }

        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public Dictionary<string, Table> TableDictionary
        {
            get { return Tables.ToDictionary(item => item.Name); }
        }
    }
}

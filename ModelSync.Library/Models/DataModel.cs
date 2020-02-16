using System.Collections.Generic;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }
    }
}

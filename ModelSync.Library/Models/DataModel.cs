using System;
using System.Collections.Generic;
using System.Text;

namespace ModelSync.Library.Models
{
    public class DataModel
    {
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }
    }
}

using ModelSync.Library.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }
    }
}

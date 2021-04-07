using ModelSync.Models;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Library.Extensions
{
    public static class DataModelHelpers
    {
        /// <summary>
        /// get the tables that reference me (i.e. child tables)
        /// </summary>
        public static IEnumerable<Table> GetReferencingTables(this Table parentTable, DataModel dataModel) =>
            dataModel.ForeignKeys
                .Where(fk => fk.ReferencedTable.Equals(parentTable))
                .Select(fk => fk.Parent as Table);

        /// <summary>
        /// get the tables I reference (i.e. parent tables)
        /// </summary>
        public static IEnumerable<Table> GetReferencedTables(this Table childTable, DataModel dataModel) =>
            dataModel.ForeignKeys
                .Where(fk => fk.Parent.Equals(childTable))
                .Select(fk => fk.ReferencedTable);
    }
}

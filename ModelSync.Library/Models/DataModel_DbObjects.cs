using System;
using System.Collections.Generic;

namespace ModelSync.Models
{
    /// <summary>
    /// the Sync* methods combine add, alter, and drop statements for the respective object type
    /// </summary>
    public partial class DataModel
    {
        private static IEnumerable<ScriptAction> SyncProcs(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> SyncViews(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> SyncFunctions(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> SyncTypes(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> SyncSequences(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }
    }
}

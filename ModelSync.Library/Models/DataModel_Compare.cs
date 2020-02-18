using System;
using System.Collections.Generic;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public static IEnumerable<ScriptAction> Compare(DataModel sourceModel, DataModel destModel)
        {
            List<ScriptAction> results = new List<ScriptAction>();

            results.AddRange(CreateSchemas(sourceModel, destModel));

            var createTables = CreateTables(sourceModel, destModel);
            results.AddRange(createTables);

            results.AddRange(AddColumns(sourceModel, destModel, createTables));
            results.AddRange(AddIndexes(sourceModel, destModel, createTables));
            results.AddRange(AlterColumns(sourceModel, destModel));
            results.AddRange(CreateForeignKeys(sourceModel, destModel));

            results.AddRange(DropIndexes(sourceModel, destModel));
            results.AddRange(DropForeignKeys(sourceModel, destModel));            
            results.AddRange(DropColumns(sourceModel, destModel));
            results.AddRange(DropTables(sourceModel, destModel));            

            return results;
        }

        private static IEnumerable<ScriptAction> DropTables(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> DropColumns(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> DropForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> DropIndexes(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> CreateForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> AlterColumns(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> AddIndexes(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> AddColumns(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> CreateTables(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<ScriptAction> CreateSchemas(DataModel sourceModel, DataModel destModel)
        {
            throw new NotImplementedException();
        }

    }
}

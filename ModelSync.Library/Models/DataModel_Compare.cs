using System;
using System.Collections.Generic;
using System.Linq;

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

        private static IEnumerable<ScriptAction> CreateSchemas(DataModel sourceModel, DataModel destModel)
        {            
            return sourceModel.GetSchemas().Except(destModel.GetSchemas()).Select(sch => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = sch,
                Commands = sch.CreateStatements()
            });
        }

        private static IEnumerable<ScriptAction> CreateTables(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.GetTables().Except(destModel.GetTables()).Select(tbl => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = tbl,
                Commands = tbl.CreateStatements()
            });
        }

        private static IEnumerable<ScriptAction> DropTables(DataModel sourceModel, DataModel destModel)
        {
            return destModel.GetTables().Except(sourceModel.GetTables()).Select(tbl => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = tbl,
                Commands = tbl.DropStatements(destModel)
            });
        }

        private static IEnumerable<ScriptAction> DropColumns(DataModel sourceModel, DataModel destModel)
        {
            return destModel.GetTables().SelectMany(tbl => tbl.Columns).Except(sourceModel.GetTables().SelectMany(tbl => tbl.Columns)).Select(col => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = col,
                Commands = col.DropStatements(destModel)
            });
        }

        private static IEnumerable<ScriptAction> DropForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            return Enumerable.Empty<ScriptAction>();
        }

        private static IEnumerable<ScriptAction> DropIndexes(DataModel sourceModel, DataModel destModel)
        {
            return Enumerable.Empty<ScriptAction>();
        }

        private static IEnumerable<ScriptAction> CreateForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            return Enumerable.Empty<ScriptAction>();
        }

        private static IEnumerable<ScriptAction> AlterColumns(DataModel sourceModel, DataModel destModel)
        {
            return Enumerable.Empty<ScriptAction>();
        }

        private static IEnumerable<ScriptAction> AddIndexes(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            return Enumerable.Empty<ScriptAction>();
        }

        private static IEnumerable<ScriptAction> AddColumns(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            return Enumerable.Empty<ScriptAction>();
        }
    }
}

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
            
            // nested object creates need to omit tables already created
            results.AddRange(AddColumns(sourceModel, destModel, createTables));
            results.AddRange(AddIndexes(sourceModel, destModel, createTables));
            
            results.AddRange(AlterColumns(sourceModel, destModel));            
            results.AddRange(CreateForeignKeys(sourceModel, destModel));

            var dropTables = DropTables(sourceModel, destModel);
            results.AddRange(dropTables);

            // nested object drops need to exclude tables that were already dropped
            results.AddRange(DropIndexes(sourceModel, destModel, dropTables));
            results.AddRange(DropForeignKeys(sourceModel, destModel, dropTables));
            results.AddRange(DropColumns(sourceModel, destModel, dropTables));
            
            return results;
        }

        private static IEnumerable<ScriptAction> CreateSchemas(DataModel sourceModel, DataModel destModel)
        {            
            return sourceModel.Schemas.Except(destModel.Schemas).Select(sch => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = sch,
                Commands = sch.CreateStatements()
            });
        }

        private static IEnumerable<ScriptAction> CreateTables(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.Tables.Except(destModel.Tables).Select(tbl => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = tbl,
                Commands = tbl.CreateStatements()
            });
        }

        private static IEnumerable<ScriptAction> AddColumns(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            var exceptTables = exceptCreatedTables.Select(scr => scr.Object).OfType<Table>();

            return sourceModel.Tables
                .Except(exceptTables)
                .SelectMany(tbl => tbl.Columns)
                .Except(destModel.Tables.SelectMany(tbl => tbl.Columns))
                .Select(col => new ScriptAction()
                {
                    Type = ActionType.Create,
                    Object = col,
                    Commands = col.CreateStatements()
                });
        }

        private static IEnumerable<ScriptAction> DropTables(DataModel sourceModel, DataModel destModel)
        {
            return destModel.Tables.Except(sourceModel.Tables).Select(tbl => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = tbl,
                Commands = tbl.DropStatements(destModel)
            });
        }

        private static IEnumerable<ScriptAction> DropColumns(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptDroppedTables)
        {
            var exceptTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();

            return destModel.Tables
                .Except(exceptTables)
                .SelectMany(tbl => tbl.Columns)                
                .Except(sourceModel.Tables.SelectMany(tbl => tbl.Columns))
                .Select(col => new ScriptAction()
                {
                    Type = ActionType.Drop,
                    Object = col,
                    Commands = col.DropStatements(destModel)
                });
        }

        private static IEnumerable<ScriptAction> CreateForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.ForeignKeys.Except(destModel.ForeignKeys).Select(fk => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = fk,
                Commands = fk.CreateStatements()
            });
        }

        private static IEnumerable<ScriptAction> DropForeignKeys(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptDroppedTables)
        {
            var droppedTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();
            var alreadyDroppedFKs = destModel.ForeignKeys.Where(fk => droppedTables.Contains(fk.Parent));

            return destModel.ForeignKeys.Except(alreadyDroppedFKs).Except(sourceModel.ForeignKeys).Select(fk => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = fk,
                Commands = fk.DropStatements(destModel)
            });
        }

        private static IEnumerable<ScriptAction> DropIndexes(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptDroppedTables)
        {
            var droppedTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();
            var alreadyDroppedIndexes = droppedTables.SelectMany(tbl => tbl.Indexes);

            var srcIndexes = sourceModel.Tables.SelectMany(tbl => tbl.Indexes).ToArray();

            return destModel.Tables
                .SelectMany(tbl => tbl.Indexes)
                .Except(alreadyDroppedIndexes)
                .Except(srcIndexes)
                .Select(ndx => new ScriptAction()
                {
                    Type = ActionType.Drop,
                    Object = ndx,
                    Commands = ndx.DropStatements(destModel)
                });
        }

        private static IEnumerable<ScriptAction> AddIndexes(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptCreatedTables)
        {
            var exceptTables = exceptCreatedTables.Select(scr => scr.Object).OfType<Table>();

            return sourceModel.Tables
                .Except(exceptTables)
                .SelectMany(tbl => tbl.Indexes)
                .Except(destModel.Tables.SelectMany(tbl => tbl.Indexes))
                .Select(ndx => new ScriptAction()
                {
                    Type = ActionType.Create,
                    Object = ndx,
                    Commands = ndx.CreateStatements()
                });
        }

        private static IEnumerable<ScriptAction> AlterColumns(DataModel sourceModel, DataModel destModel)
        {
            return Enumerable.Empty<ScriptAction>();
        }
    }
}

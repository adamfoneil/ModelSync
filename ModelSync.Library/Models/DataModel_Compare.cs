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

            var srcColumns = sourceModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.Columns);
            var destColumns = destModel.Tables.SelectMany(tbl => tbl.Columns);

            return srcColumns.Except(destColumns).Select(col =>
            {
                var tbl = destModel.TableDictionary[col.Parent.Name];
                col.DefaultValueRequired = tbl.RowCount > 0 && !col.IsNullable && !tbl.IsIdentityColumn(col.Name, "Id");
                return new ScriptAction()
                {
                    Type = ActionType.Create,
                    Object = col,
                    Commands = col.CreateStatements()
                };
            });
        }

        private static IEnumerable<ScriptAction> DropTables(DataModel sourceModel, DataModel destModel)
        {
            return destModel.Tables.Except(sourceModel.Tables).Select(tbl =>
                new ScriptAction()
                {
                    Type = ActionType.Drop,
                    Object = tbl,
                    Commands = tbl.DropStatements(destModel)
                });
        }

        private static IEnumerable<ScriptAction> DropColumns(DataModel sourceModel, DataModel destModel, IEnumerable<ScriptAction> exceptDroppedTables)
        {
            var exceptTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();

            var sourceColumns = sourceModel.Tables.SelectMany(tbl => tbl.Columns).ToArray();
            var destColumns = destModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.Columns).ToArray();

            return
                destColumns.Except(sourceColumns)
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
            var allColumns = from src in sourceModel.Tables.SelectMany(tbl => tbl.Columns)
                             join dest in destModel.Tables.SelectMany(tbl => tbl.Columns) on src equals dest
                             select new
                             {
                                 Source = src,
                                 Dest = dest
                             };

            return allColumns
                .Where(columnPair => columnPair.Source.IsAltered(columnPair.Dest, out _))
                .Select(columnPair =>
                {
                    columnPair.Source.IsAltered(columnPair.Dest, out string comment);
                    return new ScriptAction()
                    {
                        Type = ActionType.Alter,
                        Object = columnPair.Source,
                        Commands = columnPair.Source.AlterStatements(comment)
                    };
                });
        }
    }
}

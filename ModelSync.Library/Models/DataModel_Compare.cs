using ModelSync.Library.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Models
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
            results.AddRange(AddChecks(sourceModel, destModel, createTables));
            results.AddRange(AddIndexes(sourceModel, destModel, createTables));

            results.AddRange(AlterIndexes(sourceModel, destModel));
            results.AddRange(AlterChecks(sourceModel, destModel));
            results.AddRange(AlterColumns(sourceModel, destModel));

            results.AddRange(CreateForeignKeys(sourceModel, destModel));
            results.AddRange(AlterForeignKeys(sourceModel, destModel));

            var dropTables = DropTables(sourceModel, destModel);
            results.AddRange(dropTables);

            // nested object drops need to exclude tables that were already dropped
            var dropIndexes = DropIndexes(sourceModel, destModel, dropTables);
            results.AddRange(dropIndexes);
            results.AddRange(DropForeignKeys(sourceModel, destModel, dropTables));
            // column drop should also not drop parent indexes if they've already been dropped
            results.AddRange(DropColumns(sourceModel, destModel, dropTables, dropIndexes));
            results.AddRange(DropChecks(sourceModel, destModel, dropTables));

            return results;
        }

        private static IEnumerable<ScriptAction> AlterForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            var alteredKFs = from src in sourceModel.ForeignKeys
                             join dest in destModel.ForeignKeys on src equals dest
                             where (src.IsAltered(dest, out _))
                             select new
                             {
                                 @object = src,
                                 comment = src.GetAlterComment(dest)
                             };

            return alteredKFs.Select(fk => new ScriptAction()
            {
                Type = ActionType.Alter,
                Object = fk.@object,
                Commands = fk.@object.RebuildStatements(destModel, fk.comment)
            });
        }

        private static ScriptAction[] CreateSchemas(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.Schemas.Except(destModel.Schemas).Select(sch => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = sch,
                Commands = sch.CreateStatements()
            }).ToArray();
        }

        private static ScriptAction[] CreateTables(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.Tables.Except(destModel.Tables).Select(tbl => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = tbl,
                Commands = tbl.CreateStatements()
            }).ToArray();
        }

        private static ScriptAction[] AddColumns(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptCreatedTables)
        {
            var exceptTables = exceptCreatedTables.Select(scr => scr.Object).OfType<Table>();

            var srcColumns = sourceModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.Columns);
            var destColumns = destModel.Tables.SelectMany(tbl => tbl.Columns);

            return srcColumns.Except(destColumns).Select(col =>
            {
                var destTable = destModel.TableDictionary[col.Parent.Name];
                var srcTable = sourceModel.TableDictionary[col.Parent.Name];
                col.DefaultValueRequired = destTable.RowCount > 0 && !col.IsNullable && !srcTable.IsIdentityColumn(col.Name, "Id");
                return new ScriptAction()
                {
                    Type = ActionType.Create,
                    Object = col,
                    Commands = col.CreateStatements()
                };
            }).ToArray();
        }

        private static ScriptAction[] DropTables(DataModel sourceModel, DataModel destModel)
        {
            var results = destModel.Tables.Except(sourceModel.Tables).Select(tbl =>
                new ScriptAction()
                {
                    Type = ActionType.Drop,
                    Object = tbl,
                    Commands = tbl.DropStatements(destModel)
                }).ToArray();

            var ordered = results.ToDependencyOrder(dropTable =>
            {
                // all tables I (the table being dropped) reference 
                var parentTables = (dropTable.Object as Table)
                    .GetReferencedTables(destModel)
                    .ToArray();

                // filtered to this particular diff
                var droppedTables = parentTables
                    .Where(tbl => results.Any(sa => sa.Object.Equals(tbl)))
                    .ToArray();

                // give me the result as list of script actions because this is the T
                // expected by ToDependencyOrder<T>
                return droppedTables.Select(tbl => new ScriptAction()
                {
                    Type = ActionType.Drop,
                    Object = tbl,
                    Commands = tbl.DropStatements(destModel)
                });
            });

            return ordered.ToArray();
        }

        private static ScriptAction[] DropColumns(
            DataModel sourceModel, DataModel destModel,
            ScriptAction[] exceptDroppedTables, ScriptAction[] exceptDroppedIndexes)
        {
            var exceptTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();

            var sourceColumns = sourceModel.Tables.SelectMany(tbl => tbl.Columns).ToArray();
            var destColumns = destModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.Columns).ToArray();

            var results =
                destColumns.Except(sourceColumns)
                .Select(col =>
                {
                    // don't re-drop indexes you've already dropped
                    var commands = col.DropStatements(destModel).Except(exceptDroppedIndexes.SelectMany(scr => scr.Commands)).ToArray();

                    var result = new ScriptAction()
                    {
                        Type = ActionType.Drop,
                        Object = col,
                        Commands = commands
                    };

                    return result;
                }).ToArray();

            return results;
        }

        private static ScriptAction[] CreateForeignKeys(DataModel sourceModel, DataModel destModel)
        {
            return sourceModel.ForeignKeys.Except(destModel.ForeignKeys).Select(fk => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = fk,
                Commands = fk.CreateStatements()
            }).ToArray();
        }

        private static ScriptAction[] DropForeignKeys(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptDroppedTables)
        {
            var droppedTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();
            var alreadyDroppedFKs = destModel.ForeignKeys.Where(fk => droppedTables.Contains(fk.Parent));

            var deletableFKs = destModel.ForeignKeys.Except(alreadyDroppedFKs).Except(sourceModel.ForeignKeys).ToArray();

            return deletableFKs.Select(fk => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = fk,
                Commands = fk.DropStatements(destModel)
            }).ToArray();
        }

        private static ScriptAction[] DropIndexes(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptDroppedTables)
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
                }).ToArray();
        }

        private static ScriptAction[] AddIndexes(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptCreatedTables)
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
                }).ToArray();
        }

        private static ScriptAction[] AlterColumns(DataModel sourceModel, DataModel destModel)
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
                        Commands = columnPair.Source.AlterStatements(comment, destModel)
                    };
                }).ToArray();
        }

        private static ScriptAction[] AlterIndexes(DataModel sourceModel, DataModel destModel)
        {
            var allIndexes = from src in sourceModel.Tables.SelectMany(tbl => tbl.Indexes)
                             join dest in destModel.Tables.SelectMany(tbl => tbl.Indexes) on src equals dest
                             select new
                             {
                                 Source = src,
                                 Dest = dest
                             };

            return allIndexes
                .Where(indexPair => indexPair.Source.IsAltered(indexPair.Dest, out _))
                .Select(indexPair =>
                {
                    indexPair.Source.IsAltered(indexPair.Dest, out string comment);
                    return new ScriptAction()
                    {
                        Type = ActionType.Alter,
                        Object = indexPair.Source,
                        Commands = indexPair.Source.RebuildStatements(destModel, comment)
                    };
                }).ToArray();
        }

        private static IEnumerable<ScriptAction> AddChecks(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptCreatedTables)
        {
            var exceptTables = exceptCreatedTables.Select(scr => scr.Object).OfType<Table>();

            var srcChecks = sourceModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.CheckConstraints);
            var destChecks = destModel.Tables.SelectMany(tbl => tbl.CheckConstraints);

            return srcChecks.Except(destChecks).Select(chk =>
            {
                return new ScriptAction()
                {
                    Type = ActionType.Create,
                    Object = chk,
                    Commands = chk.CreateStatements()
                };
            }).ToArray();
        }

        private static IEnumerable<ScriptAction> AlterChecks(DataModel sourceModel, DataModel destModel)
        {
            var allChecks = from src in sourceModel.Tables.SelectMany(tbl => tbl.CheckConstraints)
                            join dest in destModel.Tables.SelectMany(tbl => tbl.CheckConstraints) on src equals dest
                            select new
                            {
                                Source = src,
                                Dest = dest
                            };

            return allChecks
                .Where(chkPair => chkPair.Source.IsAltered(chkPair.Dest, out _))
                .Select(chkPair =>
                {
                    chkPair.Source.IsAltered(chkPair.Dest, out string comment);
                    return new ScriptAction()
                    {
                        Type = ActionType.Alter,
                        Object = chkPair.Source,
                        Commands = chkPair.Source.RebuildStatements(destModel, comment)
                    };
                }).ToArray();
        }

        private static IEnumerable<ScriptAction> DropChecks(DataModel sourceModel, DataModel destModel, ScriptAction[] exceptDroppedTables)
        {
            var exceptTables = exceptDroppedTables.Select(scr => scr.Object).OfType<Table>();

            var sourceChecks = sourceModel.Tables.SelectMany(tbl => tbl.CheckConstraints).ToArray();
            var destChecks = destModel.Tables.Except(exceptTables).SelectMany(tbl => tbl.CheckConstraints).ToArray();

            var results =
               destChecks.Except(sourceChecks)
               .Select(chk =>
               {
                   var result = new ScriptAction()
                   {
                       Type = ActionType.Drop,
                       Object = chk,
                       Commands = chk.DropStatements(destModel)
                   };

                   return result;
               }).ToArray();

            return results;
        }
    }
}

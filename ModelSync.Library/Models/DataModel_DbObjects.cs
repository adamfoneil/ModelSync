using ModelSync.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelSync.Models
{
    /// <summary>
    /// the Sync* methods combine add, alter, and drop statements for the respective object type
    /// </summary>
    public partial class DataModel
    {
        private static IEnumerable<ScriptAction> SyncProcs(DataModel sourceModel, DataModel destModel) =>
            SyncObjects(sourceModel, destModel, (model) => model.Procedures);

        private static IEnumerable<ScriptAction> SyncViews(DataModel sourceModel, DataModel destModel) =>
            SyncObjects(sourceModel, destModel, (model) => model.Views);

        private static IEnumerable<ScriptAction> SyncFunctions(DataModel sourceModel, DataModel destModel) =>
            SyncObjects(sourceModel, destModel, (model) => model.Functions);

        private static IEnumerable<ScriptAction> SyncTypes(DataModel sourceModel, DataModel destModel) =>
            SyncObjects(sourceModel, destModel, (model) => model.TableTypes);

        private static IEnumerable<ScriptAction> SyncSequences(DataModel sourceModel, DataModel destModel) =>
            SyncObjects(sourceModel, destModel, (model) => model.Sequences);

        private static IEnumerable<ScriptAction> SyncObjects<TObject>(DataModel sourceModel, DataModel destModel, Func<DataModel, IEnumerable<TObject>> collection) where TObject : DbObject
        {
            var results = new List<ScriptAction>();

            var add = collection.Invoke(sourceModel).Except(collection.Invoke(destModel));

            results.AddRange(add.Select(obj => new ScriptAction()
            {
                Type = ActionType.Create,
                Object = obj,
                Commands = obj.CreateStatements()
            }));

            var alter = collection.Invoke(sourceModel).Join(collection.Invoke(destModel), source => source, dest => dest, (source, dest) => new
            {
                sourceObj = source,
                destObj = dest
            }).Where(pair => pair.sourceObj.IsAltered(pair.destObj).result);

            results.AddRange(alter.Select(pair =>
            {
                var comment = pair.sourceObj.GetAlterComment(pair.destObj);
                return new ScriptAction()
                {
                    Type = ActionType.Alter,
                    Object = pair.sourceObj,
                    Commands = pair.sourceObj.RebuildStatements(destModel, comment)
                };
            }));

            var drop = collection.Invoke(destModel).Except(collection.Invoke(sourceModel));

            results.AddRange(drop.Select(obj => new ScriptAction()
            {
                Type = ActionType.Drop,
                Object = obj,
                Commands = obj.DropStatements(destModel)
            }));

            return results;
        }
    }
}

using ModelSync.Library.Abstract;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public async Task<IEnumerable<ScriptAction>> CreateIfNotExistsAsync(IDbConnection connection)
        {
            List<ScriptAction> results = new List<ScriptAction>();

            async Task AddObjectsAsync(IEnumerable<DbObject> objects)
            {
                foreach (var obj in objects)
                {
                    if (!await obj.ExistsAsync(connection))
                    {
                        results.Add(new ScriptAction() 
                        { 
                            Object = obj, 
                            Commands = obj.CreateStatements(), 
                            Type = ActionType.Create 
                        });
                    }
                }
            }

            await AddObjectsAsync(Schemas);
            await AddObjectsAsync(Tables);
            await AddObjectsAsync(ForeignKeys);

            return results;
        }
    }
}

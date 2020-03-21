using ModelSync.Library.Abstract;
using ModelSync.Library.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Library.Models
{
    public partial class DataModel
    {
        public static async Task CreateTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection, SqlDialect dialect = null, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var dataModel = AssemblyModelBuilder.GetDataModelFromTypes(modelTypes, defaultSchema, defaultIdentityColumn);

            if (dialect == null) dialect = new SqlServerDialect();
            var script = await dataModel.CreateIfNotExistsAsync(connection, dialect);
            await dialect.ExecuteAsync(connection, script);
        }

        public static async Task CreateTablesAsync(IEnumerable<Type> modelTypes, Func<IDbConnection> getConnection, SqlDialect dialect = null, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            using (var cn = getConnection.Invoke())
            {
                await CreateTablesAsync(modelTypes, cn, dialect, defaultSchema, defaultIdentityColumn);
            }
        }

        public async Task<IEnumerable<ScriptAction>> CreateIfNotExistsAsync(IDbConnection connection, SqlDialect dialect)
        {
            List<ScriptAction> results = new List<ScriptAction>();

            async Task AddObjectsAsync(IEnumerable<DbObject> objects)
            {
                foreach (var obj in objects)
                {
                    if (!await obj.ExistsAsync(connection, dialect))
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

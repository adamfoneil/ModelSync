using ModelSync.Abstract;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public partial class DataModel
    {
        public void ImportTypes(IEnumerable<Type> types, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var model = new AssemblyModelBuilder().GetDataModel(types, defaultSchema, defaultIdentityColumn);
            ImportModel(model);
        }

        public void ImportTypes(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var model = new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
            ImportModel(model);
        }

        private void ImportModel(DataModel model)
        {
            Schemas = model.Schemas;
            Tables = model.Tables;
            ForeignKeys = model.ForeignKeys;
            Errors = model.Errors;
        }

        public async Task DeployAsync(IDbConnection connection, SqlDialect dialect)
        {
            var createTables = await ScriptCreateTablesAsync(connection, dialect);
            await dialect.ExecuteAsync(connection, createTables);
        }

        public static async Task CreateTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection, SqlDialect dialect = null, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var dataModel = AssemblyModelBuilder.GetDataModelFromTypes(modelTypes, defaultSchema, defaultIdentityColumn);

            if (dialect == null) dialect = new SqlServerDialect();
            var script = await dataModel.ScriptCreateTablesAsync(connection, dialect);
            await dialect.ExecuteAsync(connection, script);
        }

        public static async Task CreateTablesAsync(IEnumerable<Type> modelTypes, Func<IDbConnection> getConnection, SqlDialect dialect = null, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            using (var cn = getConnection.Invoke())
            {
                await CreateTablesAsync(modelTypes, cn, dialect, defaultSchema, defaultIdentityColumn);
            }
        }

        public async Task<IEnumerable<ScriptAction>> ScriptCreateTablesAsync(IDbConnection connection, SqlDialect dialect)
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

using AO.Models.Interfaces;
using ModelSync.Interfaces;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModelSync.Models
{
    public partial class DataModel : IDataModel, ISqlObjectCreator
    {
        public DataModel()
        {
            Schemas = Enumerable.Empty<Schema>();
            Tables = Enumerable.Empty<Table>();
            ForeignKeys = Enumerable.Empty<ForeignKey>();
        }

        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public Dictionary<Type, string> Errors { get; set; }

        public static async Task<DataModel> FromSqlServerAsync(IDbConnection connection)
        {
            var sqlServer = new SqlServerModelBuilder();
            return await sqlServer.GetDataModelAsync(connection);
        }

        public static DataModel FromAssembly(Assembly assembly, string defaultSchema = "dbo", string defaultIdentityColumn = "Id") =>
            new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);

        public static DataModel FromAssembly(string fileName, string defaultSchema = "dbo", string defaultIdentityColumn = "Id")
        {
            var assembly = Assembly.LoadFrom(fileName);
            return FromAssembly(assembly, defaultSchema, defaultIdentityColumn);
        }

        public static DataModel FromAssembly(Assembly assembly, string modelClassNamespace, string defaultSchema = "dbo", string defaultIdentityColumn = "Id") =>
            new AssemblyModelBuilder().GetDataModel(assembly, modelClassNamespace, defaultSchema, defaultIdentityColumn);

        public async Task<IEnumerable<string>> GetStatementsAsync(IDbConnection connection, IEnumerable<Type> types)
        {
            ImportTypes(types);
            var dialect = new SqlServerDialect();
            var createObjects = await ScriptCreateTablesAsync(connection, dialect);
            return createObjects
                .SelectMany(scriptAction => scriptAction.Commands)
                .Select(statement => dialect.FormatStatement(statement));
        }
    }
}

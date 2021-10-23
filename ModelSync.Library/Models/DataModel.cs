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
            Views = Enumerable.Empty<View>();
            Procedures = Enumerable.Empty<Procedure>();
            Functions = Enumerable.Empty<TableFunction>();
            TableTypes = Enumerable.Empty<TableType>();
            Sequences = Enumerable.Empty<Sequence>();
            Triggers = Enumerable.Empty<Trigger>();
        }

        public IEnumerable<Schema> Schemas { get; set; }
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }
        public IEnumerable<View> Views { get; set; }
        public IEnumerable<Procedure> Procedures { get; set; }
        public IEnumerable<TableFunction> Functions { get; set; }
        public IEnumerable<TableType> TableTypes { get; set; }
        public IEnumerable<Sequence> Sequences { get; set; }
        public IEnumerable<Trigger> Triggers { get; set; }

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

        public static DataModel FromTypesInAssembly(Assembly assembly, string modelClassNamespace, string defaultSchema = "dbo", string defaultIdentityColumn = "Id") =>
            new AssemblyModelBuilder().GetDataModel(assembly, modelClassNamespace, defaultSchema, defaultIdentityColumn);

        public async Task<IEnumerable<string>> GetStatementsAsync(IDbConnection connection, IEnumerable<Type> types)
        {
            ImportTypes(types);
            return await GetStatementsAsync(connection);
        }

        public async Task<IEnumerable<string>> GetStatementsAsync(IDbConnection connection)
        {
            var dialect = new SqlServerDialect();
            var createObjects = await ScriptCreateTablesAsync(connection, dialect);
            return createObjects
                .SelectMany(scriptAction => scriptAction.Commands)
                .Select(statement => dialect.FormatStatement(statement));
        }

        public static void CopyDatabaseSpecificObjects(DataModel fromModel, DataModel toModel)
        {
            // copy default constraints from one model to another (for #26)

            var defaults = 
                fromModel.Tables
                .SelectMany(t => t.Columns, (t, col) => new
                {
                    Table = t,
                    Column = col,
                    Key = $"{t}.{col.Name}",
                    Constraint = col.DefaultConstraint,
                    DefaultValue = col.DefaultValue
                })
                .Where(tableCol => !string.IsNullOrEmpty(tableCol.Column.DefaultConstraint))
                .ToArray();

            var matching =
                toModel.Tables
                .SelectMany(t => t.Columns, (t, col) => new
                {
                    Table = t,
                    Column = col,
                    Key = $"{t}.{col.Name}"
                }).Join(defaults, outer => outer.Key, inner => inner.Key, (outer, inner) => new
                {
                    inner.Constraint,
                    inner.DefaultValue,
                    outer.Table,
                    outer.Column
                })
                .ToArray();

            foreach (var col in matching)
            {
                var toCol = toModel.TableDictionary[col.Table.Name].ColumnDictionary[col.Column.Name];
                toCol.DefaultConstraint = col.Constraint;
                toCol.DefaultValue = col.DefaultValue;
            }
            //col.Column.DefaultConstraint = col.Constraint;
        }
    }
}

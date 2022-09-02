﻿using AO.Models.Interfaces;
using ModelSync.Interfaces;
using ModelSync.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    }
}

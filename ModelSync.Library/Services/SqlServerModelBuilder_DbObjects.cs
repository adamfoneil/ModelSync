using Dapper;
using ModelSync.Interfaces;
using ModelSync.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Services
{
    public partial class SqlServerModelBuilder : IConnectionModelBuilder
    {
        private static async Task<IEnumerable<View>> GetViewsAsync(IDbConnection connection) =>
            await connection.QueryAsync<View>(
                @"SELECT
                    SCHEMA_NAME([v].[schema_id]) + '.' + [v].[name] AS [Name],                    
                    [v].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[views] [v]
                    INNER JOIN [sys].[sql_modules] [m] ON [v].[object_id]=[m].[object_id]");

        private static async Task<IEnumerable<Procedure>> GetProceduresAsync(IDbConnection connection) =>
            await connection.QueryAsync<Procedure>(
                @"SELECT
                    SCHEMA_NAME([p].[schema_id]) + '.' + [p].[name] AS [Name],
                    [p].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[procedures] [p]
                    INNER JOIN [sys].[sql_modules] [m] ON [p].[object_id]=[m].[object_id]");

        // help from https://stackoverflow.com/a/468780/2023653
        private static async Task<IEnumerable<TableFunction>> GetTableFunctionsAsync(IDbConnection connection) =>
            await connection.QueryAsync<TableFunction>(
                @"SELECT
                    SCHEMA_NAME([o].[schema_id]) + '.' + [o].[name] AS [Name],                    
                    [o].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[objects] [o]
                    INNER JOIN [sys].[sql_modules] [m] ON [o].[object_id]=[m].[object_id]
                WHERE
                    [o].[type]='TF'");
        
        private static async Task<IEnumerable<TableType>> GetTableTypesAsync(IDbConnection connection)
        {
            var types = await connection.QueryAsync<TableType>(
                @"SELECT 
                    SCHEMA_NAME([t].[schema_id]) + '.' + [t].[name] AS [Name],
                    [t].[type_table_object_id] AS [ObjectId]
                FROM 
                    [sys].[table_types] [t]
                WHERE
                    [t].[is_table_type]=1");

            var objectIds = types.Select(row => row.ObjectId).ToArray();

            var columns = (await connection.QueryAsync<Column>(GetColumnsQuery(tableTypes: true), new { objectIds })).ToLookup(row => row.ObjectId);

            foreach (var t in types) t.Columns = columns[t.ObjectId];

            return types;
        }

        private static async Task<IEnumerable<Sequence>> GetSequencesAsync(IDbConnection connection) =>
            await connection.QueryAsync<Sequence>(
                @"SELECT 
                    SCHEMA_NAME([seq].[schema_id]) + '.' + [seq].[name] AS [Name],
                    [object_id] AS [ObjectId],
                    [start_value] AS [SeedValue],
                    [increment] AS [Increment]
                FROM 
                    [sys].[sequences] [seq]");
    }
}

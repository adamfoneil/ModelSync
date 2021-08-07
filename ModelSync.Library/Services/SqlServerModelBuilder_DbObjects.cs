using Dapper;
using ModelSync.Interfaces;
using ModelSync.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModelSync.Services
{
    public partial class SqlServerModelBuilder : IConnectionModelBuilder
    {
        private static async Task<IEnumerable<View>> GetViewsAsync(IDbConnection connection) =>
            await GetObjectsAsync<View>(connection,
                @"SELECT
                    SCHEMA_NAME([v].[schema_id]) + '.' + [v].[name] AS [Name],                    
                    [v].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[views] [v]
                    INNER JOIN [sys].[sql_modules] [m] ON [v].[object_id]=[m].[object_id]");

        private static async Task<IEnumerable<Procedure>> GetProceduresAsync(IDbConnection connection) =>
            await GetObjectsAsync<Procedure>(connection,
                @"SELECT
                    SCHEMA_NAME([p].[schema_id]) '.' + [p].[name] AS [Name],
                    [p].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[procedures] [p]
                    INNER JOIN [sys].[sql_modules] [m] ON [p].[object_id]=[m].[object_id]");

        // help from https://stackoverflow.com/a/468780/2023653
        private static async Task<IEnumerable<TableFunction>> GetFunctionsAsync(IDbConnection connection) =>
            await GetObjectsAsync<TableFunction>(connection,
                @"SELECT
                    SCHEMA_NAME([o].[schema_id]) + '.' + [o].[name] AS [Name],                    
                    [o].[object_id] AS [ObjectId],
                    [m].[definition] AS [Definition]
                FROM 
                    [sys].[objects] [o]
                    INNER JOIN [sys].[sql_modules] [m] ON [o].[object_id]=[m].[object_id]
                WHERE
                    [o].[type]='TF'");
        

        private static Task<IEnumerable<TableType>> GetTableTypesAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        private static Task<IEnumerable<Sequence>> GetSequencesAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        private static async Task<IEnumerable<T>> GetObjectsAsync<T>(IDbConnection connection, string query) where T : IDefinable => 
            await connection.QueryAsync<T>(query);        
    }
}

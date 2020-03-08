using Dapper;
using Microsoft.Data.SqlClient;
using ModelSync.Library.Abstract;
using ModelSync.Library.Interfaces;
using ModelSync.Library.Models;
using ModelSync.Library.Models.Internal;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ModelSync.Library.Services
{
	public partial class SqlServerModelBuilder : IConnectionModelBuilder
	{
		protected static async Task<IEnumerable<Schema>> GetSchemasAsync(IDbConnection connection)
		{
			var schemas = await connection.QueryAsync<string>(
				@"SELECT [name] FROM sys.schemas
				WHERE [name] NOT IN ('guest', 'sys', 'information_schema')");

			return schemas.Select(s => new Schema() { Name = s });
		}

		protected static async Task<IEnumerable<Table>> GetTablesAsync(IDbConnection connection)
		{
			var tables = await connection.QueryAsync<Table>(
				@"WITH [clusteredIndexes] AS (
					SELECT [name], [object_id] FROM [sys].[indexes] WHERE [type_desc]='CLUSTERED'
				), [identityColumns] AS (
					SELECT [object_id], [name] FROM [sys].[columns] WHERE [is_identity]=1
				) SELECT
					SCHEMA_NAME([t].[schema_id]) + '.' + [t].[name] AS [Name],					
					[t].[object_id] AS [ObjectId],
					[c].[name] AS [ClusteredIndex],
					[i].[name] AS [IdentityColumn],
					(SELECT SUM(row_count) FROM [sys].[dm_db_partition_stats] WHERE [object_id]=[t].[object_id] AND [index_id] IN (0, 1)) AS [RowCount]
				FROM
					[sys].[tables] [t]
					LEFT JOIN [clusteredIndexes] [c] ON [t].[object_id]=[c].[object_id]
					LEFT JOIN [identityColumns] [i] ON [t].[object_id]=[i].[object_id]
				WHERE
					([t].[name] NOT LIKE 'AspNet%' OR [t].[name] LIKE 'AspNetUsers') AND
					[t].[name] NOT IN ('__MigrationHistory', '__EFMigrationsHistory')");

			var columns = await connection.QueryAsync<Column>(
				@"WITH [identityColumns] AS (
					SELECT [object_id], [name] FROM [sys].[columns] WHERE [is_identity]=1
				), [source] AS (
					SELECT
						[col].[object_id] AS [ObjectId],
						[col].[name] AS [Name],
						TYPE_NAME([col].[system_type_id]) AS [DataType],
						[col].[is_nullable] AS [IsNullable],
						[def].[definition]  AS [Default],
						[col].[collation_name] AS [Collation],
						CASE
							WHEN TYPE_NAME([col].[system_type_id]) LIKE 'nvar%' AND [col].[max_length]>0 THEN ([col].[max_length]/2)
							WHEN TYPE_NAME([col].[system_type_id]) LIKE 'nvar%' AND [col].[max_length]=-1 THEN -1
							ELSE NULL
						END AS [MaxLength],
						[col].[precision] AS [Precision],
						[col].[scale] AS [Scale],
						[col].[column_id] AS [InternalId],
						[calc].[definition] AS [Expression],
						CASE
							WHEN [ic].[name] IS NOT NULL THEN 1
							ELSE 0
						END AS [IsIdentity],
						[col].[system_type_id]
					FROM
						[sys].[columns] [col]
						INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
						LEFT JOIN [sys].[default_constraints] [def] ON [col].[default_object_id]=[def].[object_id]
						LEFT JOIN [sys].[computed_columns] [calc] ON [col].[object_id]=[calc].[object_id] AND [col].[column_id]=[calc].[column_id]
						LEFT JOIN [identityColumns] [ic] ON [ic].[object_id]=[col].[object_id] AND [ic].[name]=[col].[name]
					WHERE
						[t].[type_desc]='USER_TABLE'
				) SELECT
					[ObjectId],
					[Name],
					CASE
						WHEN [system_type_id]=106 THEN [DataType] + '(' + CONVERT(varchar, [Precision]) + ',' + CONVERT(varchar, [Scale]) + ')'
						WHEN [IsIdentity]=1 THEN [DataType] + ' identity(1,1)'
						WHEN [MaxLength]=-1 THEN [DataType] + '(max)'
						WHEN [MaxLength] IS NULL THEN [DataType]
						ELSE [DataType] + '(' + CONVERT(varchar, [MaxLength]) + ')'
					END AS [DataType],
					[IsNullable],
					[Default],
					[Collation],
					[Precision],
					[InternalId],
					[Expression],
					CASE
						WHEN [Expression] IS NOT NULL THEN 1
						ELSE 0
					END AS [IsCalculated]
				FROM
					[source]");

			var indexes = await connection.QueryAsync<Index>(
				@"SELECT
					[x].[object_id] AS [ObjectId],
					[x].[name] AS [Name],
					CONVERT(bit, CASE
						WHEN [x].[type_desc]='CLUSTERED' THEN 1
						ELSE 0
					END) AS [IsClustered],
					CASE
						WHEN [x].[is_primary_key]=1 THEN 1
						WHEN [x].[is_unique]=1 AND [x].[is_unique_constraint]=0 THEN 2
						WHEN [x].[is_unique_constraint]=1 THEN 3
						WHEN [x].[is_unique]=0 THEN 4
					END AS [Type],
					[x].[index_id] AS [InternalId]
				FROM
					[sys].[indexes] [x]
					INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE' AND
					[x].[type]<>0");

			var indexCols = await connection.QueryAsync<IndexColumnResult>(
				@"SELECT
					[xcol].[object_id],
					[xcol].[index_id],
					[col].[name],
					[xcol].[key_ordinal],
					[xcol].[is_descending_key]
				FROM
					[sys].[index_columns] [xcol]
					INNER JOIN [sys].[indexes] [x] ON [xcol].[object_id]=[x].[object_id] AND [xcol].[index_id]=[x].[index_id]
					INNER JOIN [sys].[columns] [col] ON [xcol].[object_id]=[col].[object_id] AND [xcol].[column_id]=[col].[column_id]
					INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE'");

			var columnLookup = columns.ToLookup(row => row.ObjectId);
			var indexLookup = indexes.ToLookup(row => row.ObjectId);
			var indexColLookup = indexCols.ToLookup(row => new IndexKey() { object_id = row.object_id, index_id = row.index_id });

			foreach (var x in indexes)
			{
				var indexKey = new IndexKey() { object_id = x.ObjectId, index_id = x.InternalId };
				x.Columns = indexColLookup[indexKey].Select(row => new Index.Column()
				{
					Name = row.name,
					Order = row.key_ordinal,
					SortDirection = (row.is_descending_key) ? SortDirection.Descending : SortDirection.Ascending
				});
			}

			foreach (var t in tables)
			{
				t.Columns = columnLookup[t.ObjectId].ToArray();
				foreach (var col in t.Columns) col.Parent = t;

				t.Indexes = indexLookup[t.ObjectId].ToArray();
				foreach (var x in t.Indexes) x.Parent = t;
			}

			return tables;
		}

		protected static async Task<IEnumerable<ForeignKey>> GetForeignKeysAsync(IDbConnection connection, IEnumerable<Table> tables)
		{
			var tableDictionary = tables.ToDictionary(item => item.Name);

			var foreignKeys = await connection.QueryAsync<ForeignKeysResult>(
				@"SELECT
					[fk].[object_id] AS [ObjectId],
					[fk].[name] AS [ConstraintName],
					SCHEMA_NAME([ref_t].[schema_id]) AS [ReferencedSchema],
					[ref_t].[name] AS [ReferencedTable],
					SCHEMA_NAME([child_t].[schema_id]) AS [ReferencingSchema],
					[child_t].[name] AS [ReferencingTable],
					CONVERT(bit, [fk].[delete_referential_action]) AS [CascadeDelete],
					CONVERT(bit, [fk].[update_referential_action]) AS [CascadeUpdate]
				FROM
					[sys].[foreign_keys] [fk]
					INNER JOIN [sys].[tables] [ref_t] ON [fk].[referenced_object_id]=[ref_t].[object_id]
					INNER JOIN [sys].[tables] [child_t] ON [fk].[parent_object_id]=[child_t].[object_id]
				WHERE
					[fk].[name] NOT LIKE '%AspNetUser%' AND
					[fk].[name] NOT LIKE '%AspNetRole%'");

			var columns = await connection.QueryAsync<ForeignKeyColumnsResult>(
				@"SELECT
					[fkcol].[constraint_object_id] AS [ObjectId],
					[child_col].[name] AS [ReferencingName],
					[ref_col].[name] AS [ReferencedName]
				FROM
					[sys].[foreign_key_columns] [fkcol]
					INNER JOIN [sys].[tables] [child_t] ON [fkcol].[parent_object_id]=[child_t].[object_id]
					INNER JOIN [sys].[columns] [child_col] ON
						[child_t].[object_id]=[child_col].[object_id] AND
						[fkcol].[parent_column_id]=[child_col].[column_id]
					INNER JOIN [sys].[tables] [ref_t] ON [fkcol].[referenced_object_id]=[ref_t].[object_id]
					INNER JOIN [sys].[columns] [ref_col] ON
						[ref_t].[object_id]=[ref_col].[object_id] AND
						[fkcol].[referenced_column_id]=[ref_col].[column_id]");

			var colLookup = columns.ToLookup(row => row.ObjectId);

			return foreignKeys.Select(fk => new ForeignKey()
			{
				Name = fk.ConstraintName,
				ReferencedTable = tableDictionary[$"{fk.ReferencedSchema}.{fk.ReferencedTable}"],
				Parent = tableDictionary[$"{fk.ReferencingSchema}.{fk.ReferencingTable}"],
				CascadeDelete = fk.CascadeDelete,
				CascadeUpdate = fk.CascadeUpdate,
				Columns = colLookup[fk.ObjectId].Select(fkcol => new ForeignKey.Column() 
				{ 
					ReferencedName = fkcol.ReferencedName, 
					ReferencingName = fkcol.ReferencingName 
				})
			});
		}

		public async Task<DataModel> GetDataModelAsync(IDbConnection connection)
		{
			var result = new DataModel();
			result.Schemas = await GetSchemasAsync(connection);
			result.Tables = await GetTablesAsync(connection);
			result.ForeignKeys = await GetForeignKeysAsync(connection, result.Tables);
			return result;
		}
	}
}

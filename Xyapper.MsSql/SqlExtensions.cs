using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Xyapper.MsSql
{
    /// <summary>
    /// Xyapper MsSql main extensions
    /// </summary>
    public static class SqlExtensions
    {
        /// <summary>
        /// Get a list of columns for the DB table
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static IEnumerable<SqlColumn> XGetColumns(this SqlConnection sqlConnection, string tableName, string schema = "dbo", SqlTransaction transaction = null)
        {
            var sqlCommandText = @"  
                SELECT
	                [c].[name] AS [ColumnName],
	                [t].[name] AS [ColumnType],
	                [c].[max_length] AS [ColumnSize]				
                FROM [sys].[objects] [o]
                JOIN [sys].[schemas] [s] ON [s].[schema_id] = [o].[schema_id]
                JOIN [sys].[columns] [c] ON [c].[object_id] = [o].[object_id]
                JOIN [sys].[types] [t] ON [t].[user_type_id] = [c].[user_type_id]
                WHERE
	                [o].[name] = @TableName
	                AND [s].[name] = @Schema
	                AND [o].[type] = 'U'";

            return sqlConnection.XQuery<SqlColumn>(sqlCommandText, new { TableName = tableName, Schema = schema }, transaction);
        }

        /// <summary>
        /// Check if table exists in DB
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static bool XCheckIfTableExists(this SqlConnection sqlConnection, string tableName, string schema = "dbo", SqlTransaction transaction = null)
        {
            return sqlConnection.XGetColumns(tableName, schema, transaction).Any();
        }

        /// <summary>
        /// Create table with specified columns
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="schema"></param>
        /// <param name="transaction"></param>
        public static void XCreateTable(this SqlConnection sqlConnection, string tableName, IEnumerable<SqlColumn> columns, string schema = "dbo", SqlTransaction transaction = null)
        {
            var builder = new StringBuilder();
            builder.Append($"CREATE TABLE [{schema}].[{tableName}]\r\n(\r\n");

            builder.Append(
                string.Join(",\r\n",
                    columns.Select(column => 
                        $"\t[{column.ColumnName}] {Enum.GetName(typeof(SqlDbType), column.ColumnType)}" 
                        + (IsSizedType(column.ColumnType) ? $" ({(column.ColumnSize == -1 ? "MAX" : column.ColumnSize.ToString())})" : ""))
                )
            );

            builder.Append("\r\n)\r\n");

            var sqlCommandText = builder.ToString();
            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        /// <summary>
        /// Drop table from DB
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="isTempTable"></param>
        /// <param name="transaction"></param>
        public static void XDropTable(this SqlConnection sqlConnection, string tableName, string schema = "dbo", bool isTempTable = false, SqlTransaction transaction = null)
        {
            var sqlCommandText = isTempTable ? $"DROP TABLE [tempdb]..[{tableName}]" : $"DROP TABLE [{schema}].[{tableName}]";

            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        /// <summary>
        /// Truncate table
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="transaction"></param>
        public static void XTruncateTable(this SqlConnection sqlConnection, string tableName, string schema = "dbo", SqlTransaction transaction = null)
        {
            var sqlCommandText = $"TRUNCATE TABLE [{schema}].[{tableName}]";
            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        /// <summary>
        /// Add column to table in DB
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <param name="schema"></param>
        /// <param name="transaction"></param>
        public static void XAddColumn(this SqlConnection sqlConnection, string tableName, SqlColumn column, string schema = "dbo", SqlTransaction transaction = null)
        {
            var sqlCommandText = $"ALTER TABLE [{schema}].[{tableName}] ADD [{column.ColumnName}] {Enum.GetName(typeof(SqlDbType), column.ColumnType)}"
                  + (IsSizedType(column.ColumnType) ? $"({(column.ColumnSize == -1 ? "MAX" : column.ColumnSize.ToString())})" : "");

            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        /// <summary>
        /// Bulk copy list of objects to DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection">SQL Connection</param>
        /// <param name="tableName">Target table name</param>
        /// <param name="data">List of items to copy</param>
        /// <param name="schema">DB Schema (default = dbo)</param>
        /// <param name="createTableIfNotExists">Create table if it not exists in DB</param>
        /// <param name="addColumnsIfNotExist">Add columns to table if they not exist</param>
        /// <param name="defaultCharColumnSize">Set the size of NVARCHAR(N) column to be created from System.String property</param>
        /// <param name="throwExceptionIfCannotMapType">Stop if any property cannot be stored in DB. If not, it will be ignored</param>
        /// <param name="transaction">Transaction to use</param>
        public static void XBulkCopy<T>(
            this SqlConnection sqlConnection, 
            string tableName, 
            IEnumerable<T> data, 
            string schema = "dbo", 
            bool createTableIfNotExists = true, 
            bool addColumnsIfNotExist = true, 
            int defaultCharColumnSize = 1024, 
            bool throwExceptionIfCannotMapType = false, 
            SqlTransaction transaction = null)
        {
            var dataTable = Xyapper.Internal.Utilities.ToDataTable(data);
            sqlConnection.XBulkCopy(tableName, dataTable, schema, createTableIfNotExists, addColumnsIfNotExist, defaultCharColumnSize, throwExceptionIfCannotMapType, transaction);
        }

        /// <summary>
        /// Bulk copy list of objects to DB
        /// </summary>
        /// <param name="sqlConnection">SQL Connection</param>
        /// <param name="tableName">Target table name</param>
        /// <param name="dataTable">DataTable to copy</param>
        /// <param name="schema">DB Schema (default = dbo)</param>
        /// <param name="createTableIfNotExists">Create table if it not exists in DB</param>
        /// <param name="addColumnsIfNotExist">Add columns to table if they not exist</param>
        /// <param name="defaultCharColumnSize">Set the size of NVARCHAR(N) column to be created from System.String property</param>
        /// <param name="throwExceptionIfCannotMapType">Stop if any property cannot be stored in DB. If not, it will be ignored</param>
        /// <param name="transaction">Transaction to use</param>
        public static void XBulkCopy(
            this SqlConnection sqlConnection, 
            string tableName, 
            DataTable dataTable, 
            string schema = "dbo", 
            bool createTableIfNotExists = true, 
            bool addColumnsIfNotExist = true, 
            int defaultCharColumnSize = 3999, 
            bool throwExceptionIfCannotMapType = false,
            SqlTransaction transaction = null)
        {
            if(sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection.Open();
            }

            var myTransaction = sqlConnection.BeginTransaction();
            try
            {
                var tableColumns = dataTable.Columns.Cast<DataColumn>().Where(column => Internal.TypeConverter.GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).HasValue). 
                Select(column => new SqlColumn
                {
                    ColumnName = column.ColumnName,
                    ColumnType = Internal.TypeConverter.GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).Value,
                    ColumnSize = IsSizedType(Internal.TypeConverter.GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).Value) ? defaultCharColumnSize : -1
                });


                var existingColumns = sqlConnection.XGetColumns(tableName, schema, myTransaction).ToList();
                var tableCreatedRightNow = false;

                var sqlColumns = tableColumns as SqlColumn[] ?? tableColumns.ToArray();
                if (createTableIfNotExists)
                {
                    if(!existingColumns.Any())
                    {
                        sqlConnection.XCreateTable(tableName, sqlColumns, schema, myTransaction);
                        tableCreatedRightNow = true;
                    }
                }

                if(addColumnsIfNotExist && !tableCreatedRightNow)
                {
                    var requiredColumns = sqlColumns.Where(column => !existingColumns.Contains(column));
                    foreach(var column in requiredColumns)
                    {
                        sqlConnection.XAddColumn(tableName, column, schema, myTransaction);
                    }
                }

                using (var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, myTransaction))
                {
                    bulkCopy.DestinationTableName = $"[{schema}].[{tableName}]";

                    foreach (var column in sqlColumns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    LogBulkCopy(bulkCopy, dataTable.Rows.Count);
                    
                    bulkCopy.WriteToServer(dataTable);
                    bulkCopy.Close();
                }
                myTransaction.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }

        }

        /// <summary>
        /// Log bulk copy event
        /// </summary>
        /// <param name="sqlBulkCopy"></param>
        /// <param name="itemCount"></param>
        private static void LogBulkCopy(SqlBulkCopy sqlBulkCopy, int itemCount)
        {
            if (XyapperManager.EnableLogging)
            {
                XyapperManager.Logger.Log(
                    XyapperManager.CommandLogLevel,
                    new Microsoft.Extensions.Logging.EventId(),
                    sqlBulkCopy,
                    null,
                    (bulk, ex) =>
                    {
                        var message =
                            $"SqlBulkCopy: destination table {sqlBulkCopy.DestinationTableName}, row count {itemCount}.\r\nSqlBulkCopy Mapping:\r\n" +
                            string.Join(",\r\n",
                                sqlBulkCopy.ColumnMappings.Cast<SqlBulkCopyColumnMapping>().Select(mapping =>
                                    $"[{mapping.SourceColumn}] -> [{mapping.DestinationColumn}]"));

                        return message;
                    });
            }
        }

        /// <summary>
        /// Returns true if SQL Type requires size definition (for example VARCHAR(100))
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSizedType(SqlDbType type)
        {
            var sizedTypes = new[] { SqlDbType.Char, SqlDbType.NChar, SqlDbType.NVarChar, SqlDbType.VarBinary, SqlDbType.VarChar };
            return sizedTypes.Contains(type);
        }

    }
}

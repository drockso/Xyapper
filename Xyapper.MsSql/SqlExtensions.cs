using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Xyapper.MsSql
{
    public static class SqlExtensions
    {
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

        public static bool XCheckIfTableExists(this SqlConnection sqlConnection, string tableName, string schema = "dbo", SqlTransaction transaction = null)
        {
            return sqlConnection.XGetColumns(tableName, schema, transaction).Any();
        }

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
            var xx = (IDbTransaction)transaction;
            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        public static void XDropTable(this SqlConnection sqlConnection, string tableName, string schema = "dbo", bool isTempTable = false, SqlTransaction transaction = null)
        {
            var sqlCommandText = "";
            if (isTempTable)
            {
                sqlCommandText = $"DROP TABLE [tempdb]..[{tableName}]";
            }
            else
            {
                sqlCommandText = $"DROP TABLE [{schema}].[{tableName}]";
            }

            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

        public static void XAddColumn(this SqlConnection sqlConnection, string tableName, SqlColumn column, string schema = "dbo", SqlTransaction transaction = null)
        {
            var sqlCommandText = $"ALTER TABLE [{schema}].[{tableName}] ADD [{column.ColumnName}] {Enum.GetName(typeof(SqlDbType), column.ColumnType)}"
                  + (IsSizedType(column.ColumnType) ? $"({(column.ColumnSize == -1 ? "MAX" : column.ColumnSize.ToString())})" : "");

            sqlConnection.XExecuteNonQuery(sqlCommandText, null, transaction);
        }

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

        public static void XBulkCopy(
            this SqlConnection sqlConnection, 
            string tableName, 
            DataTable dataTable, 
            string schema = "dbo", 
            bool createTableIfNotExists = true, 
            bool addColumnsIfNotExist = true, 
            int defaultCharColumnSize = 1024, 
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
                var tableColumns = dataTable.Columns.Cast<DataColumn>().Where(column => GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).HasValue). 
                Select(column => new SqlColumn
                {
                    ColumnName = column.ColumnName,
                    ColumnType = GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).Value,
                    ColumnSize = IsSizedType(GetSqlDbType(column.DataType, throwExceptionIfCannotMapType).Value) ? defaultCharColumnSize : -1
                });


                var existingColumns = sqlConnection.XGetColumns(tableName, schema, myTransaction).ToList();
                var tableCreatedRightNow = false;

                if (createTableIfNotExists)
                {
                    if(!existingColumns.Any())
                    {
                        sqlConnection.XCreateTable(tableName, tableColumns, schema, myTransaction);
                        tableCreatedRightNow = true;
                    }
                }

                if(addColumnsIfNotExist && !tableCreatedRightNow)
                {
                    var requiredColumns = tableColumns.Where(column => !existingColumns.Contains(column));
                    foreach(var column in requiredColumns)
                    {
                        sqlConnection.XAddColumn(tableName, column, schema, myTransaction);
                    }
                }

                using (var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, myTransaction))
                {
                    bulkCopy.DestinationTableName = $"[{schema}].[{tableName}]";

                    foreach (var column in tableColumns)
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
                transaction.Rollback();
                throw;
            }

        }

        private static void LogBulkCopy(SqlBulkCopy sqlBulkCopy, int itemCount)
        {
            if (XyapperManager.EnableLogging)
            {
                XyapperManager.Logger.Log<SqlBulkCopy>(
                XyapperManager.CommandLogLevel,
                new Microsoft.Extensions.Logging.EventId(),
                sqlBulkCopy,
                null,
                (bulk, ex) =>
                {
                    var message = $"SqlBulkCopy: destination table {sqlBulkCopy.DestinationTableName}, row count {itemCount}.\r\nSqlBulkCopy Mapping:\r\n" +
                    string.Join(",\r\n", sqlBulkCopy.ColumnMappings.Cast<SqlBulkCopyColumnMapping>().Select(mapping => $"[{mapping.SourceColumn}] -> [{mapping.DestinationColumn}]"));

                    return message;
                });
            }
        }


        private static bool IsSizedType(SqlDbType type)
        {
            var sizedTypes = new[] { SqlDbType.Char, SqlDbType.NChar, SqlDbType.NVarChar, SqlDbType.VarBinary, SqlDbType.VarChar };
            return sizedTypes.Contains(type);
        }

        private static SqlDbType? GetSqlDbType(Type type, bool exceptionIfCannotMap)
        {
            try
            {
                var typeMap = new Dictionary<Type, SqlDbType>();

                typeMap[typeof(string)] = SqlDbType.NVarChar;
                typeMap[typeof(char[])] = SqlDbType.NVarChar;
                typeMap[typeof(int)] = SqlDbType.Int;
                typeMap[typeof(Int32)] = SqlDbType.Int;
                typeMap[typeof(Int16)] = SqlDbType.SmallInt;
                typeMap[typeof(Int64)] = SqlDbType.BigInt;
                typeMap[typeof(Byte[])] = SqlDbType.VarBinary;
                typeMap[typeof(Boolean)] = SqlDbType.Bit;
                typeMap[typeof(DateTime)] = SqlDbType.DateTime2;
                typeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
                typeMap[typeof(Decimal)] = SqlDbType.Decimal;
                typeMap[typeof(Double)] = SqlDbType.Float;
                typeMap[typeof(Decimal)] = SqlDbType.Money;
                typeMap[typeof(Byte)] = SqlDbType.TinyInt;
                typeMap[typeof(TimeSpan)] = SqlDbType.Time;
                typeMap[typeof(Guid)] = SqlDbType.UniqueIdentifier;

                if (exceptionIfCannotMap)
                {
                    return typeMap[(type)];
                }
                else
                {
                    if (typeMap.ContainsKey(type)) return typeMap[(type)];
                    else return null;
                }
            }
            catch
            {
                throw new Exception($"Failed to map type {type.Name} to MsSql column type!");
            }

        }

    }
}

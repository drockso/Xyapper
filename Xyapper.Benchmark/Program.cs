using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Logging;
using Xyapper;
using Xyapper.MsSql;

namespace Xyapper.Benchmark
{
    static class Program
    {
        static void Main(string[] args)
        {
            XyapperManager.EnableLogging = true;
            XyapperManager.Logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger("Xyapper");
            XyapperManager.CommandLogLevel = LogLevel.Trace;
            XyapperManager.ExceptionLogLevel = LogLevel.Error;


            var myResult = Connection.XQuery<MyClass>(
                "SELECT @FieldInt AS [FieldInt], @FieldString AS [FieldString], @FieldDateTime AS [FieldDateTime]",
                new {FieldInt = 1, FieldString = "test", FieldDateTime = DateTime.Now}).ToArray();
            

            Connection.XBulkCopy(tableName: "DestinationTableName", data: myResult, schema: "dbo", createTableIfNotExists: true, addColumnsIfNotExist: true);

            var columns = Connection.XGetColumns("MyTableName", "dbo");
            var tableExists = Connection.XCheckIfTableExists("MyTableName", "dbo");
            

            Connection.XCreateTable("MyNewTable", new List<SqlColumn>()
            {
                new SqlColumn() {ColumnName = "Column1", ColumnType = SqlDbType.Int},
                new SqlColumn() {ColumnName = "Column2", ColumnType = SqlDbType.VarChar, ColumnSize = 100}
            }, 
            "dbo");

            Connection.XDropTable("MyNewTable", "dbo");
            Connection.XAddColumn("MyTableName", new SqlColumn() {ColumnName = "NewColumn", ColumnType = SqlDbType.Int});

            var dataTable = Connection.XGetDataTable(
                "SELECT @FieldInt AS [FieldInt], @FieldString AS [FieldString], @FieldDateTime AS [FieldDateTime]",
                new {FieldInt = 1, FieldString = "test", FieldDateTime = DateTime.Now});
        }


        public static SqlConnection Connection
        {
            get
            {
                return new SqlConnection("Data Source=localhost;Database=Test;User Id=sa;Password=123123");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Logging;
using Xyapper;
using Xyapper.MsSql;

namespace Xyapper.Benchmark
{
    // Nested deserialization is under development. Will be introduced in 1.1.0

    //[NestedMapping(0)]
    //class MyClass
    //{
    //    public int MyClassId { get; set; }
    //    public  string Text { get; set; }

    //    public  MyClass2 Class2 { get; set; }

    //    public List<MyClass3> Class3 { get; set; }
    //}

    //[NestedMapping(1, "MyClassId")]
    //class MyClass2
    //{
    //    public int MyClassId { get; set; }
    //    public string Text { get; set; }

    //}

    //[NestedMapping(2, "MyClassId")]
    //class MyClass3
    //{
    //    public int MyClassId { get; set; }
    //    public int MyClass3Id { get; set; }
    //    public string Text { get; set; }
    //    public List<MyClass4> Class4 { get; set; }
    //}

    //[NestedMapping(3, "MyClass3Id")]
    //class MyClass4
    //{
    //    public int MyClass3Id { get; set; }
    //    public string Text { get; set; }
    //}


    static class Program
    {
        private static SQLiteConnection Connection => new SQLiteConnection($"Data Source={SQLiteDbPath};");
        private static string SQLiteDbPath;

        static void Main(string[] args)
        {
            XyapperManager.EnableLogging = true;
            XyapperManager.Logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger("Xyapper");
            XyapperManager.CommandLogLevel = LogLevel.Information;
            XyapperManager.ExceptionLogLevel = LogLevel.Error;

            XyapperManager.TrimStrings = true; //Trim all strings retrieved from DB
            XyapperManager.UseAdvancedTypeConversions = true; //Use automatic explicit type conversions


            Console.WriteLine("Xyapper vs Dapper benchmark");

            Console.WriteLine("Creating test SQLite DB...");
            CreateDb();


            //Connection.XQueryNested<MyClass>(@"
            //    SELECT 1 AS MyClassId, 'test1' AS Text;
            //    SELECT 1 AS MyClassId, 'test2' AS Text;
            //    SELECT 1 AS MyClassId, 1 AS MyClass3Id, 'test31' AS Text UNION SELECT 1 AS MyClassId, 2 AS MyClass3Id, 'test32' AS Text;
            //    SELECT 1 AS MyClass3Id, 'test4' AS Text;

            //");

            Console.WriteLine("Populating test DB...");
            PopulateDb();

            Console.WriteLine("Running benchmark...");
            RunBenchmark();

            Console.WriteLine("All done!");
            Console.ReadLine();
        }

        private static void RunBenchmark()
        {
            var selectCommandText = @"SELECT * FROM TestTable LIMIT {0}";

            var stopwatch = new Stopwatch();

            foreach (var rowCount in new[] {10, 100, 1000, 10000})
            {
                for (int i = 0; i < 5; i++)
                {
                    stopwatch.Restart();
                    var xyapperResult = Connection.XQuery<MyClassName>(string.Format(selectCommandText, rowCount)).ToList();
                    stopwatch.Stop();
                    Console.WriteLine("Xyapper result for {0} rows is {1} ms", rowCount, stopwatch.ElapsedMilliseconds);

                    stopwatch.Restart();
                    var dapperResult = Connection.Query<MyClassName>(string.Format(selectCommandText, rowCount)).ToList();
                    stopwatch.Stop();
                    Console.WriteLine("Dapper result for {0} rows is {1} ms", rowCount, stopwatch.ElapsedMilliseconds);
                }
                Console.WriteLine("**************************************");
            }
        }

        private static void CreateDb()
        {
            SQLiteDbPath = "MyDatabase.sqlite".ToBinaryPath();

            if (File.Exists(SQLiteDbPath))
            {
                File.Delete(SQLiteDbPath);
            }
            SQLiteConnection.CreateFile(SQLiteDbPath);
        }

        private static void PopulateDb()
        {
            var createTableCommandText = @"
                CREATE TABLE TestTable
                (
                    Column1 INTEGER,
                    Column2 TEXT,
                    Column3 REAL,
                    Column4 NUMERIC
                )";

            Connection.XExecuteNonQuery(createTableCommandText);

            var insertCommandText = @"
                INSERT INTO TestTable (Column1, Column2, Column3, Column4) 
                VALUES(@A, @B, @C, @D)";

            var random = new Random();

            //Magic to make SQLite do inserts much faster
            var singleConnection = (SQLiteConnection)Connection.Clone();
            singleConnection.XExecuteNonQuery("BEGIN");

            for (int i = 0; i < 10000; i++)
            {
                singleConnection.XExecuteNonQuery(insertCommandText, new {A = random.Next(0, 1000), B = $"Test {i}", C = i, D = random.Next(0, 1000) });
                Console.Write("Populated {0:0.00}%\r", (float)i / 10000 * 100.0);
            }

            singleConnection.XExecuteNonQuery("END");
            singleConnection.Close();
            Console.WriteLine();
        }

        public static string ToBinaryPath(this string fileName)
        {
            return Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, fileName);
        }
    }
}

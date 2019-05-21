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
    static class Program
    {
        private static SQLiteConnection Connection => new SQLiteConnection($"Data Source={SQLiteDbPath};");
        private static string SQLiteDbPath;

        static void Main(string[] args)
        {
            Console.WriteLine("Xyapper vs Dapper benchmark");

            Console.WriteLine("Creating test SQLite DB...");
            CreateDb();

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

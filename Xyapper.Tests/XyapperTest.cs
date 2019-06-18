using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xyapper.Tests
{
    [TestClass]
    public class XyapperTest
    {
        public XyapperTest()
        {
            if (!File.Exists("MyDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("MyDatabase.sqlite");
            }
        }

        ~XyapperTest()
        {
            if (File.Exists("MyDatabase.sqlite"))
            {
                try
                {
                    File.Delete("MyDatabase.sqlite");
                }
                catch
                {

                }
            }
        }

        public static SQLiteConnection Connection => new SQLiteConnection("Data Source=MyDatabase.sqlite;");

        [TestMethod]
        public void TestXQueryScalar()
        {
            var result = Connection.XQueryScalar<int>("SELECT 1");
            Assert.AreEqual(result, 1);

            var result2 = Connection.XQueryScalar<DateTime>("SELECT '2019-01-01'");
            Assert.AreEqual(result2, new DateTime(2019, 01, 01));

            var result3 = Connection.XQueryScalar<bool>("SELECT 'Y'");
            Assert.AreEqual(result3, true);

            var result4 = Connection.XQueryScalar<string>("SELECT 'test'");
            Assert.AreEqual(result4, "test");
        }

        [TestMethod]
        public void TestEnum()
        {
            var result = Connection.XQueryScalar<TestEnum>("SELECT 'Value2'");
            Assert.AreEqual(result, Tests.TestEnum.Value2);

            var result2 = Connection.XQuery<TestType4>("SELECT 'Value3' AS TestEnum UNION SELECT 'Value2' AS TestEnum")
                .ToList();
            Assert.AreEqual(result2.Count, 2);
            Assert.AreEqual(result2.Any(x => x.TestEnum == Tests.TestEnum.Value2), true);
            Assert.AreEqual(result2.Any(x => x.TestEnum == Tests.TestEnum.Value3), true);
        }

        [TestMethod]
        public void TestTrimStrings()
        {
            XyapperManager.TrimStrings = true;
            XyapperManager.EmptyStringsToNull = true;

            var result = Connection.XQueryScalar<string>("SELECT '  test  '");
            Assert.AreEqual(result, "test");

            result = Connection.XQueryScalar<string>("SELECT '    '");
            Assert.AreEqual(result, null);

            XyapperManager.TrimStrings = false;
            XyapperManager.EmptyStringsToNull = true;

            result = Connection.XQueryScalar<string>("SELECT '  test  '");
            Assert.AreEqual(result, "  test  ");

            result = Connection.XQueryScalar<string>("SELECT '    '");
            Assert.AreEqual(result, null);

            XyapperManager.TrimStrings = false;
            XyapperManager.EmptyStringsToNull = false;

            result = Connection.XQueryScalar<string>("SELECT '  test  '");
            Assert.AreEqual(result, "  test  ");

            result = Connection.XQueryScalar<string>("SELECT '    '");
            Assert.AreEqual(result, "    ");

            XyapperManager.TrimStrings = true;
            XyapperManager.EmptyStringsToNull = false;

            result = Connection.XQueryScalar<string>("SELECT '  test  '");
            Assert.AreEqual(result, "test");

            result = Connection.XQueryScalar<string>("SELECT '    '");
            Assert.AreEqual(result, "");
        }

        [TestMethod]
        public void TestTrimStrings2()
        {
            XyapperManager.TrimStrings = true;
            XyapperManager.EmptyStringsToNull = true;

            DataTable result;

            result = Connection.XGetDataTable("SELECT '  test  ', '       '");
            Assert.AreEqual(result.Rows[0][0], "test");
            Assert.AreEqual(result.Rows[0][1], DBNull.Value);

            XyapperManager.TrimStrings = true;
            XyapperManager.EmptyStringsToNull = false;

            result = Connection.XGetDataTable("SELECT '  test  ', '       '");
            Assert.AreEqual(result.Rows[0][0], "test");
            Assert.AreEqual(result.Rows[0][1], "");

            XyapperManager.TrimStrings = false;
            XyapperManager.EmptyStringsToNull = false;

            result = Connection.XGetDataTable("SELECT '  test  ', '       '");
            Assert.AreEqual(result.Rows[0][0], "  test  ");
            Assert.AreEqual(result.Rows[0][1], "       ");

            XyapperManager.TrimStrings = false;
            XyapperManager.EmptyStringsToNull = true;

            result = Connection.XGetDataTable("SELECT '  test  ', '       '");
            Assert.AreEqual(result.Rows[0][0], "  test  ");
            Assert.AreEqual(result.Rows[0][1], DBNull.Value);
        }

        [TestMethod]
        public void TestXQueryScalarInt2()
        {
            var result = Connection.XQueryScalar<int>("SELECT 1 UNION SELECT 2 ORDER BY 1 ASC");
            Assert.AreEqual(result, 1);
        }


        [TestMethod]
        public void TestXQueryScalarNullable()
        {
            var result = Connection.XQueryScalar<DateTime?>("SELECT NULL");
            Assert.AreEqual(result.HasValue, false);
        }

        [TestMethod]
        public void TestXQueryGeneric()
        {
            var result = Connection.XQuery<TestType>(
                @"SELECT 
                    @Int AS ColumnInt, 
                    @String AS ColumnString, 
                    CAST('2019-01-01' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    @Double AS ColumnDouble,
                    2 AS ColumnInt2", new {Int = 1, String = "test", Double = 0.03}).ToList();

            Assert.AreEqual(result.Count, 1);

            var resultRow = result[0];
        
            Assert.AreEqual(resultRow.ColumnInt, 1);
            Assert.AreEqual(resultRow.ColumnDateNull.HasValue, false);
            Assert.AreEqual(resultRow.ColumnString, "test");
            Assert.AreEqual(resultRow.ColumnDouble, 0.03);
            Assert.AreEqual(resultRow.ColumnInt2.HasValue, false);
        }

        [TestMethod]
        public void TestXQuerySingle()
        {
            var result = Connection.XQuerySingle<TestType>(
                @"SELECT 
                    1 AS ColumnInt, 
                    'test' AS ColumnString, 
                    CAST('2019-01-01' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    0.03 AS ColumnDouble,
                    2 AS ColumnInt2
             UNION 
             SELECT 
                    2 AS ColumnInt, 
                    'test2' AS ColumnString, 
                    CAST('2019-01-02' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    0.01 AS ColumnDouble,
                    3 AS ColumnInt2");

            Assert.AreEqual(result.ColumnInt, 1);
            Assert.AreEqual(result.ColumnDateNull.HasValue, false);
            Assert.AreEqual(result.ColumnString, "test");
            Assert.AreEqual(result.ColumnDouble, 0.03);
            Assert.AreEqual(result.ColumnInt2.HasValue, false);
        }

        [TestMethod]
        public void TestXQuerySingleNull()
        {
            var result = Connection.XQuerySingle<TestType>(
                @"SELECT 
                    1 AS ColumnInt, 
                    'test' AS ColumnString, 
                    CAST('2019-01-01' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    0.03 AS ColumnDouble,
                    2 AS ColumnInt2
             WHERE
                    1 <> 1");

            Assert.AreEqual(result, null);
        }

        [TestMethod]
        public void TestXGetDataTable()
        {
            var result = Connection.XGetDataTable(
                @"SELECT 
                    1 AS ColumnInt, 
                    'test' AS ColumnString, 
                    CAST('2019-01-01' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    0.03 AS ColumnDouble,
                    2 AS ColumnInt2
             UNION
             SELECT 
                    2 AS ColumnInt, 
                    'test2' AS ColumnString, 
                    CAST('2019-01-02' AS DateTime) AS ColumnDate, 
                    NULL AS ColumnDateNull, 
                    0.01 AS ColumnDouble,
                    3 AS ColumnInt2");

            Assert.AreEqual(result.Rows.Count, 2);
            Assert.AreEqual(result.Rows[0]["ColumnInt"].ToString(), "1");
            Assert.AreEqual(result.Rows[0]["ColumnString"].ToString(), "test");
            Assert.AreEqual(result.Rows[1]["ColumnString"].ToString(), "test2");
            Assert.AreEqual(result.Rows[1]["ColumnInt2"].ToString(), "3");
        }

        [TestMethod]
        public void TestXExecuteNonQuery()
        {
            Connection.XExecuteNonQuery("SELECT @Value", new {Value = 1});   
        }

        [TestMethod]
        public void TestXGenerateClassCode()
        {
            var command = @"SELECT 
                            1 AS ColumnInt,
                            'test' AS ColumnString,
                            CAST('2019-01-01' AS DateTime) AS ColumnDate,
                            NULL AS ColumnDateNull,
                            0.03 AS ColumnDouble,
                            2 AS ColumnInt2";

            var code = Connection.XGenerateClassCode(command, null, "MyClass", true);

            Assert.AreEqual(code.Length > 0, true);
        }

        [TestMethod]
        [ExpectedException(typeof(XyapperException), "The ICustomDeserialized class cannot contain [ColumnMapping] attribute!")]
        public void TestICustomDeserialized()
        {
            var result = Connection.XQuery<TestType2>("SELECT 1 AS FieldInt").ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Custom deserializer invoked")]
        public void TestICustomDeserialized2()
        {
            var result = Connection.XQuery<TestType3>("SELECT 1 AS FieldInt").ToArray();
        }

        [TestMethod]
        public void TestXGetDatSet()
        {
            var dataSet = Connection.XGetDataSet("SELECT 1 AS [Col11], 'test' AS [Col12], 0.01 AS [Col13]; SELECT 2 AS [Col21], 'test2' AS [Col22], 0.02 AS [Col23];");

            Assert.AreEqual(dataSet.Tables.Count, 2);

            Assert.AreEqual(dataSet.Tables[0].Rows[0]["Col11"], (Int64)1);
            Assert.AreEqual(dataSet.Tables[0].Rows[0]["Col12"], "test");

            Assert.AreEqual(dataSet.Tables[1].Rows[0]["Col21"], (Int64)2);
            Assert.AreEqual(dataSet.Tables[1].Rows[0]["Col22"], "test2");
        }
    }
}
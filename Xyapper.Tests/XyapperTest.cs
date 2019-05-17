using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xyapper;
using Xyapper.Tests;

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
            File.Delete("MyDatabase.sqlite");
        }
    }

    public static SQLiteConnection Connection => new SQLiteConnection("Data Source=MyDatabase.sqlite;");

    [TestMethod]
    public void TestXQueryScalarInt()
    {
        var result = Connection.XQueryScalar<int>("SELECT 1");
        Assert.AreEqual(result, 1);
    }

    [TestMethod]
    public void TestXQueryScalarInt2()
    {
        var result = Connection.XQueryScalar<int>("SELECT 1 UNION SELECT 2");
        Assert.AreEqual(result, 1);
    }

    [TestMethod]
    public void TestXQueryScalarString()
    {
        var result = Connection.XQueryScalar<string>("SELECT 'test'");
        Assert.AreEqual(result, "test");
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
}
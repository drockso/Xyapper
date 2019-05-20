using System;
using System.Data;
using Xyapper;

public class MyClass : ICustomDeserialized
{
    public System.Int64? ColumnInt { get; set; }
    public System.String ColumnString { get; set; }
    public System.Int64? ColumnDate { get; set; }
    public System.Object ColumnDateNull { get; set; }
    public System.Double? ColumnDouble { get; set; }
    public System.Int64? ColumnInt2 { get; set; }


    public void Deserialize(IDataRecord record)
    {
        ColumnInt = record["ColumnInt"].ToType<System.Int64?>();


    }
}
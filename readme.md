# Xyapper
A lightweight ORM wrapper for ADO.NET providers

[![NuGet Version and Downloads count](https://buildstats.info/nuget/Xyapper)](https://www.nuget.org/packages/Xyapper)
[![](https://dev.azure.com/drockso/Xyapper/_apis/build/status/drockso.Xyapper)]()

# Advantages
**All-purpose.** Works great with any ADO.NET provider!

**Tiny and lighweight.** Has only one external dependency for logging. 

**Blazing fast.** It uses and runtime compile expressions to deserialize data faster than competitors!

**Supports logging out of the box.**

**Flexibility.** Developer can specify mappings and even write own deserializers.

**Legacy friendly.** Yes, DataTables are also supported.

# Usage examples

Create a connection singleton factory (here is MS SQL connection)
```csharp
public static SqlConnection Connection
{
	get
	{
		return new SqlConnection("Data Source=localhost;Database=mydatabase;User Id=sa;Password=mypassword");
	}
}
```

Set up logging (here we use NLog, but any logging provider with Microsoft.Extensions.Logging.ILogger interface will suit)
```csharp
XyapperManager.EnableLogging = true;
XyapperManager.Logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger("Xyapper");
XyapperManager.CommandLogLevel = LogLevel.Trace;
XyapperManager.ExceptionLogLevel = LogLevel.Error;
```

Select a single value from database
```csharp
int? result = Connection.XQueryScalar<int?>("SELECT @value", new {Value = 1});
```

Select a collection of objects
```csharp
var data = Connection.XQuery<MyCustomType>("SELECT * FROM [dbo].[MyCustomTypeTable]");
```

Or just a first record
```csharp
var data = Connection.XQuerySingle<MyCustomType>("SELECT * FROM [dbo].[MyCustomTypeTable]");
```

Declare types to be deserialized automatically
```csharp
public class MyClass
{
	public int FieldInt { get; set; }

	public string FieldString { get; set; }

	public DateTime FieldDateTime { get; set; }

	[ColumnMapping(ignore:true)]
	public float FieldToIgnore { get; set; }

	[ColumnMapping(columnName: "FieldInt")]
	public int FieldToMap { get; set; }
}
```
and query them easyly
```csharp
var myResult = Connection.XQuery<MyClass>(
	"SELECT @FieldInt AS [FieldInt], @FieldString AS [FieldString], @FieldDateTime AS [FieldDateTime]",
	new {FieldInt = 1, FieldString = "test", FieldDateTime = DateTime.Now}).ToArray();
```

Want to deserialize a type manually? No problem!
```csharp
using Xyapper;

public class MyClass : ICustomDeserialized
{
	public int FieldInt { get; set; }

	public string FieldString { get; set; }

	public DateTime? FieldDateTime { get; set; }

	public void Deserialize(IDataRecord record)
	{
		FieldInt = record["FieldInt"].ToType<int>();
		FieldString = record["FieldString"].ToType<string>();
		FieldDateTime = record["FieldDateTime"].ToType<DateTime?>();
	}
}
```

Too lazy even to declare a type? You're welcome!
```csharp
var code = Connection.XGenerateClassCode(@"SELECT * FROM [dbo].[MyTable]", null, "MyClass");
```

# Xyapper.MsSql
Microsoft SQL Server specific extensions for Xyapper

[![NuGet Version and Downloads count](https://buildstats.info/nuget/Xyapper.MsSql)](https://www.nuget.org/packages/Xyapper.MsSql)
[![](https://dev.azure.com/drockso/Xyapper/_apis/build/status/drockso.Xyapper)]()

# Usage examples
Create a SQL connection singleton factory (for Xyapper.MsSql only SqlConnection will fit)
```csharp
public static SqlConnection Connection
{
	get
	{
		return new SqlConnection("Data Source=localhost;Database=mydatabase;User Id=sa;Password=mypassword");
	}
}
```

Bulk copy an array to database
```csharp
Connection.XBulkCopy(tableName: "DestinationTableName", data: myDataArray, schema: "dbo", createTableIfNotExists: true, addColumnsIfNotExist: true);
```

Bulk copy a DataTable to database
```csharp
Connection.XBulkCopy(tableName: "DestinationTableName", dataTable: myDataTable , schema: "dbo", createTableIfNotExists: true, addColumnsIfNotExist: true);
```

Get columns list of a table in DB
```csharp
var columns = Connection.XGetColumns("MyTableName", "dbo");
```

Check if table exists
```csharp
var tableExists = Connection.XCheckIfTableExists("MyTableName", "dbo");
```

Create a new table in DB
```csharp
Connection.XCreateTable("MyNewTable", new List<SqlColumn>()
{
	new SqlColumn() {ColumnName = "Column1", ColumnType = SqlDbType.Int},
	new SqlColumn() {ColumnName = "Column2", ColumnType = SqlDbType.VarChar, ColumnSize = 100}
}, "dbo");
```

Drop a table in DB
```csharp
Connection.XDropTable("MyNewTable", "dbo");
```

Add a column to table
```csharp
Connection.XAddColumn("MyTableName", new SqlColumn() {ColumnName = "NewColumn", ColumnType = SqlDbType.Int});
```


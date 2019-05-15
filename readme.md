# Xyapper
A lightweight ORM wrapper for ADO.NET providers

[![NuGet Version and Downloads count](https://buildstats.info/nuget/Xyapper)](https://www.nuget.org/packages/Xyapper)
[![](https://dev.azure.com/drockso/Xyapper/_apis/build/status/drockso.Xyapper)]()

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
	public class MyClass : ICustomDeserialized
	{
		public int FieldInt { get; set; }

		public string FieldString { get; set; }

		public DateTime? FieldDateTime { get; set; }

		public void Deserialize(IDataRecord record)
		{
			FieldInt = Xyapper.Internal.TypeConverter.ToType<int>(record["FieldInt"]);
			FieldString = Xyapper.Internal.TypeConverter.ToType<string>(record["FieldString"]);
			FieldDateTime = Xyapper.Internal.TypeConverter.ToType<DateTime?>(record["FieldDateTime"]);
		}
	}
```

Too lazy even to declare a type? You're welcome!
```csharp
	var dataTable = Connection.XGetDataTable(
		"SELECT @FieldInt AS [FieldInt], @FieldString AS [FieldString], @FieldDateTime AS [FieldDateTime]",
		new {FieldInt = 1, FieldString = "test", FieldDateTime = DateTime.Now});

	var generatedCode = Xyapper.CodeGenerator.GenerateClassCode(dataTable, "MyClass");
```
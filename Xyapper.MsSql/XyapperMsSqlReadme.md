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


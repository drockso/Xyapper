using System;
using System.Data;
using System.Linq;

namespace Xyapper.Internal
{
    /// <summary>
    /// Strongly typed DataTable schema description
    /// </summary>
    public class SchemaItem
    {
        public string ColumnName { get; set; }
        public int? ColumnOrdinal { get; set; }
        public long? ColumnSize { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public bool? IsUnique { get; set; }
        public bool? IsKey { get; set; }
        public string BaseServerName { get; set; }
        public string BaseCatalogName { get; set; }
        public string BaseColumnName { get; set; }
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public Type DataType { get; set; }
        public bool? AllowDbNull { get; set; }
        public int? ProviderType { get; set; }
        public bool? IsAliased { get; set; }
        public bool? IsExpression { get; set; }
        public bool? IsIdentity { get; set; }
        public bool? IsAutoIncrement { get; set; }
        public bool? IsRowVersion { get; set; }
        public bool? IsHidden { get; set; }
        public bool? IsLong { get; set; }
        public bool? IsReadOnly { get; set; }
        public Type ProviderSpecificDataType { get; set; }
        public string DataTypeName { get; set; }


        private static SchemaItem FromDataRow(DataRow row)
        {
            var result = new SchemaItem();

            result.ColumnName = row.Table.Columns.Contains("ColumnName") ? row["ColumnName"].ToType<string>() : null;
            result.ColumnOrdinal = row.Table.Columns.Contains("ColumnOrdinal") ? row["ColumnOrdinal"].ToType<int?>() : null;
            result.ColumnSize = row.Table.Columns.Contains("ColumnSize") ? row["ColumnSize"].ToType<int?>() : null;
            result.NumericPrecision = row.Table.Columns.Contains("NumericPrecision") ? row["NumericPrecision"].ToType<int?>() : null;
            result.NumericScale = row.Table.Columns.Contains("NumericScale") ? row["NumericScale"].ToType<int?>() : null;
            result.IsUnique = row.Table.Columns.Contains("ColIsUniqueumnName") ? row["IsUnique"].ToType<bool?>() : null;
            result.IsKey = row.Table.Columns.Contains("IsKey") ? row["IsKey"].ToType<bool?>() : null;
            result.BaseServerName = row.Table.Columns.Contains("BaseServerName") ? row["BaseServerName"].ToType<string>() : null;
            result.BaseCatalogName = row.Table.Columns.Contains("BaseCatalogName") ? row["BaseCatalogName"].ToType<string>() : null;
            result.BaseColumnName = row.Table.Columns.Contains("BaseColumnName") ? row["BaseColumnName"].ToType<string>() : null;
            result.BaseSchemaName = row.Table.Columns.Contains("BaseSchemaName") ? row["BaseSchemaName"].ToType<string>() : null;
            result.BaseTableName = row.Table.Columns.Contains("BaseTableName") ? row["BaseTableName"].ToType<string>() : null;
            result.DataType = row.Table.Columns.Contains("DataType") ? Type.GetType(row["DataType"].ToString()) : null;
            result.AllowDbNull = row.Table.Columns.Contains("AllowDBNull") ? row["AllowDBNull"].ToType<bool?>() : null;
            result.ProviderType = row.Table.Columns.Contains("ProviderType") ? row["ProviderType"].ToType<int?>() : null;
            result.IsAliased = row.Table.Columns.Contains("IsAliased") ? row["IsAliased"].ToType<bool?>() : null;
            result.IsExpression = row.Table.Columns.Contains("IsExpression") ? row["IsExpression"].ToType<bool?>() : null;
            result.IsIdentity = row.Table.Columns.Contains("IsIdentity") ? row["IsIdentity"].ToType<bool?>() : null;
            result.IsAutoIncrement = row.Table.Columns.Contains("IsAutoIncrement") ? row["IsAutoIncrement"].ToType<bool?>() : null;
            result.IsRowVersion = row.Table.Columns.Contains("IsRowVersion") ? row["IsRowVersion"].ToType<bool?>() : null;
            result.IsHidden = row.Table.Columns.Contains("IsHidden") ? row["IsHidden"].ToType<bool?>() : null;
            result.IsLong = row.Table.Columns.Contains("IsLong") ? row["IsLong"].ToType<bool?>() : null;
            result.IsReadOnly = row.Table.Columns.Contains("IsReadOnly") ? row["IsReadOnly"].ToType<bool?>() : null;
            result.ProviderSpecificDataType = row.Table.Columns.Contains("ProviderSpecificDataType") ? Type.GetType(row["ProviderSpecificDataType"].ToString()) : null;
            result.DataTypeName = row.Table.Columns.Contains("DataTypeName") ? row["DataTypeName"].ToType<string>() : null;

            return result;
        }

        public static SchemaItem[] FromDataTable(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(FromDataRow).ToArray();
        }

        public static SchemaItem[] GetSchemas(IDataReader reader)
        {
            return FromDataTable(reader.GetSchemaTable());
        }
    }
}
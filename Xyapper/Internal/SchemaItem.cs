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
        //public bool? IsIdentity { get; set; }
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

            result.ColumnName = row["ColumnName"].ToType<string>();
            result.ColumnOrdinal = row["ColumnOrdinal"].ToType<int?>();
            result.ColumnSize = row["ColumnSize"].ToType<int?>();
            result.NumericPrecision = row["NumericPrecision"].ToType<int?>();
            result.NumericScale = row["NumericScale"].ToType<int?>();
            result.IsUnique = row["IsUnique"].ToType<bool?>();
            result.IsKey = row["IsKey"].ToType<bool?>();
            result.BaseServerName = row["BaseServerName"].ToType<string>();
            result.BaseCatalogName = row["BaseCatalogName"].ToType<string>();
            result.BaseColumnName = row["BaseColumnName"].ToType<string>();
            result.BaseSchemaName = row["BaseSchemaName"].ToType<string>();
            result.BaseTableName = row["BaseTableName"].ToType<string>();
            result.DataType = Type.GetType(row["DataType"].ToString());
            result.AllowDbNull = row["AllowDBNull"].ToType<bool?>();
            result.ProviderType = row["ProviderType"].ToType<int?>();
            result.IsAliased = row["IsAliased"].ToType<bool?>();
            result.IsExpression = row["IsExpression"].ToType<bool?>();
            //result.IsIdentity = row["IsIdentity"].ToType<bool?>();
            result.IsAutoIncrement = row["IsAutoIncrement"].ToType<bool?>();
            result.IsRowVersion = row["IsRowVersion"].ToType<bool?>();
            result.IsHidden = row["IsHidden"].ToType<bool?>();
            result.IsLong = row["IsLong"].ToType<bool?>();
            result.IsReadOnly = row["IsReadOnly"].ToType<bool?>();
            result.ProviderSpecificDataType = Type.GetType(row["ProviderSpecificDataType"].ToString());
            result.DataTypeName = row["DataTypeName"].ToType<string>();

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
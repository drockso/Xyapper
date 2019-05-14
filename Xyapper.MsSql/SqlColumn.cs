using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xyapper;

namespace Xyapper.MsSql
{
    public class SqlColumn : ICustomDeserialized, IEquatable<SqlColumn>
    {
        public string ColumnName { get; set; }
        public SqlDbType ColumnType { get; set; }
        public int ColumnSize { get; set; }

        public void Deserialize(IDataRecord record)
        {
            ColumnName = record["ColumnName"] == DBNull.Value ? null : (string)record["ColumnName"];
            ColumnType = Enum.Parse<SqlDbType>(Enum.GetNames(typeof(SqlDbType)).First(x => x.ToLower() == (string)record["ColumnType"]));
            ColumnSize = Convert.ToInt32(record["ColumnSize"]);

            if(IsDoubleBytesType(ColumnType) && ColumnSize > 0)
            {
                ColumnSize /= 2;
            }
        }

        public bool Equals(SqlColumn other)
        {
            return ColumnName == other.ColumnName;// && ColumnType == other.ColumnType;
        }

        private bool IsDoubleBytesType(SqlDbType type)
        {
            var types = new[] { SqlDbType.NChar, SqlDbType.NVarChar };
            return types.Contains(type);
        }
    }
}

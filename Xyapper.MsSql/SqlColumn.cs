using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xyapper;

namespace Xyapper.MsSql
{
    /// <summary>
    /// Sql column definition
    /// </summary>
    public class SqlColumn : ICustomDeserialized, IEquatable<SqlColumn>
    {
        /// <summary>
        /// Column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// ColumnType
        /// </summary>
        public SqlDbType ColumnType { get; set; }

        /// <summary>
        /// Size of column. -1 for MAX
        /// </summary>
        public int ColumnSize { get; set; }

        public void Deserialize(IDataRecord record)
        {
            ColumnName = record["ColumnName"] == DBNull.Value ? null : (string)record["ColumnName"];
            ColumnType = (SqlDbType)Enum.Parse(typeof(SqlDbType), Enum.GetNames(typeof(SqlDbType)).First(x => x.ToLower() == (string)record["ColumnType"]));
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

        /// <summary>
        /// For Unicode strings MsSql doubles size
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsDoubleBytesType(SqlDbType type)
        {
            var types = new[] { SqlDbType.NChar, SqlDbType.NVarChar };
            return types.Contains(type);
        }
    }
}

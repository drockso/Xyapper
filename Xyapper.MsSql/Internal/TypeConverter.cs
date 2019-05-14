using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Xyapper.MsSql.Internal
{
    public static class TypeConverter
    {
        /// <summary>
        /// Map Sql type to System type
        /// </summary>
        /// <param name="sqltype">Sql type</param>
        /// <returns></returns>
        public static Type GetSystemType(SqlDbType sqltype)
        {
            var types = new Dictionary<SqlDbType, Type>();
            types.Add(SqlDbType.BigInt, typeof(Int64));
            types.Add(SqlDbType.Binary, typeof(Byte[]));
            types.Add(SqlDbType.Bit, typeof(Boolean));
            types.Add(SqlDbType.Char, typeof(String));
            types.Add(SqlDbType.Date, typeof(DateTime));
            types.Add(SqlDbType.DateTime, typeof(DateTime));
            types.Add(SqlDbType.DateTime2, typeof(DateTime));
            types.Add(SqlDbType.DateTimeOffset, typeof(DateTimeOffset));
            types.Add(SqlDbType.Decimal, typeof(Decimal));
            types.Add(SqlDbType.Float, typeof(Double));
            types.Add(SqlDbType.Image, typeof(Byte[]));
            types.Add(SqlDbType.Int, typeof(Int32));
            types.Add(SqlDbType.Money, typeof(Decimal));
            types.Add(SqlDbType.NChar, typeof(String));
            types.Add(SqlDbType.NText, typeof(String));
            types.Add(SqlDbType.NVarChar, typeof(String));
            types.Add(SqlDbType.Real, typeof(Single));
            types.Add(SqlDbType.SmallDateTime, typeof(DateTime));
            types.Add(SqlDbType.SmallInt, typeof(Int16));
            types.Add(SqlDbType.SmallMoney, typeof(Decimal));
            types.Add(SqlDbType.Text, typeof(String));
            types.Add(SqlDbType.Time, typeof(TimeSpan));
            types.Add(SqlDbType.Timestamp, typeof(Byte[]));
            types.Add(SqlDbType.TinyInt, typeof(Byte));
            types.Add(SqlDbType.UniqueIdentifier, typeof(Guid));
            types.Add(SqlDbType.VarBinary, typeof(Byte[]));
            types.Add(SqlDbType.VarChar, typeof(String));
            types.TryGetValue(sqltype, out var resulttype);
            return resulttype;
        }

        public static SqlDbType? GetSqlDbType(Type type, bool exceptionIfCannotMap)
        {
            try
            {
                var typeMap = new Dictionary<Type, SqlDbType>();

                typeMap[typeof(string)] = SqlDbType.NVarChar;
                typeMap[typeof(char[])] = SqlDbType.NVarChar;
                typeMap[typeof(int)] = SqlDbType.Int;
                typeMap[typeof(Int32)] = SqlDbType.Int;
                typeMap[typeof(Int16)] = SqlDbType.SmallInt;
                typeMap[typeof(Int64)] = SqlDbType.BigInt;
                typeMap[typeof(Byte[])] = SqlDbType.VarBinary;
                typeMap[typeof(Boolean)] = SqlDbType.Bit;
                typeMap[typeof(DateTime)] = SqlDbType.DateTime2;
                typeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
                typeMap[typeof(Decimal)] = SqlDbType.Decimal;
                typeMap[typeof(Double)] = SqlDbType.Float;
                typeMap[typeof(Decimal)] = SqlDbType.Money;
                typeMap[typeof(Byte)] = SqlDbType.TinyInt;
                typeMap[typeof(TimeSpan)] = SqlDbType.Time;
                typeMap[typeof(Guid)] = SqlDbType.UniqueIdentifier;

                if (exceptionIfCannotMap)
                {
                    return typeMap[(type)];
                }
                else
                {
                    if (typeMap.ContainsKey(type)) return typeMap[(type)];
                    else return null;
                }
            }
            catch
            {
                throw new Exception($"Failed to map type {type.Name} to MsSql column type!");
            }

        }
    }



}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Xyapper
{
    public static class TypeConverter
    {
        /// <summary>
        /// Sql type aware cast object to specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Object to cast</param>
        /// <param name="formatProvider">Custom format provider</param>
        /// <returns></returns>
        public static T ToType<T>(this object value, IFormatProvider formatProvider = null)
        {
            if (formatProvider == null) formatProvider = CultureInfo.InvariantCulture;

            var field = default(T);
            var isNullableType = IsNullable(field);
            if (isNullableType)
            {
                if (value == DBNull.Value) field = default(T);
                else
                {
                    var baseType = typeof(T).GetGenericArguments().Any() ? typeof(T).GetGenericArguments()[0] : typeof(T);
                    field = (T)ChangeType(value, baseType, formatProvider);
                }
                return field;
            }

            field = (T)ChangeType(value, typeof(T), formatProvider);
            return field;
        }

        /// <summary>
        /// Advanced conversions
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        private static object ChangeType(object value, Type targetType, IFormatProvider formatProvider)
        {
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString());
            }

            if (value is long l && targetType == typeof(DateTime))
            {
                return new DateTime(l);
            }

            if (value is string && targetType == typeof(bool))
            {
                if (value.ToString().ToLower().Trim() == "y") return true;
                if (value.ToString().ToLower().Trim() == "n") return false;
                throw new XyapperException($"Failed to convert string '{value}' to boolean!");
            }

            if ((XyapperManager.TrimStrings || XyapperManager.EmptyStringsToNull) && targetType == typeof(string))
            {
                return TrimAndNull(value.ToString());
            }

            return Convert.ChangeType(value, targetType, formatProvider);
        }

        /// <summary>
        /// Trim and check for whitespace according to global settings
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimAndNull(string value)
        {
            var trimmed = value.Trim();
            if (XyapperManager.TrimStrings && !XyapperManager.EmptyStringsToNull) return trimmed;

            trimmed = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            if (XyapperManager.TrimStrings && XyapperManager.EmptyStringsToNull) return trimmed;

            var nonTrimmed = value;
            nonTrimmed = string.IsNullOrWhiteSpace(nonTrimmed) ? null : nonTrimmed;
            if (!XyapperManager.TrimStrings && XyapperManager.EmptyStringsToNull) return nonTrimmed;

            return value;
        }

        /// <summary>
        /// Check if object is nullable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullable<T>(T obj)
        {
            if (obj == null)
                return true;

            var type = typeof(T);
            if (!type.IsValueType)
                return true;

            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Check if object is nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(Type type)
        {
            if (!type.IsValueType)
                return true;

            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
        }
    }



}

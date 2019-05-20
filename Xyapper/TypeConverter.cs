using System;
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
            bool isNullableType = IsNullable(field);
            if (isNullableType)
            {
                if (value == DBNull.Value) field = default(T);
                else
                {
                    var baseType = typeof(T).GetGenericArguments().Any() ? typeof(T).GetGenericArguments()[0] : typeof(T);
                    field = (T)Convert.ChangeType(value, baseType, formatProvider);
                }
                return field;
            }

            field = (T)Convert.ChangeType(value, typeof(T), formatProvider);
            return field;
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

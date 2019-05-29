using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Xyapper.Internal
{
    public static class Utilities
    {
        /// <summary>
        /// Convert bunch of objects to DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Bunch of objects</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            var result = new DataTable();

            var properties = typeof(T).GetProperties();

            var columnMapping = new Dictionary<string, string>();

            foreach(var property in properties)
            {
                if(IsMappableToColumn(property))
                {
                    var mappingAttribute = property.GetCustomAttribute<ColumnMappingAttribute>();
                    var targetColumnName = mappingAttribute != null ? mappingAttribute.ColumnName : property.Name;

                    if (columnMapping.ContainsKey(targetColumnName))
                    {
                        throw new XyapperException($"Cannot map type {typeof(T).Name} to DataTable. More than one properties map to single column [{targetColumnName}]!");
                    }
                    columnMapping.Add(property.Name, targetColumnName);

                    Type targetType;
                    if (property.PropertyType.IsEnum)
                    {
                        targetType = typeof(string);
                    }
                    else
                    {
                        targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    }

                    result.Columns.Add(targetColumnName, targetType);
                }
            }

            if(!columnMapping.Any())
            {
                throw new XyapperException($"Cannot map type {typeof(T).Name} to DataTable. None of the properties could be mapped!");
            }

            foreach(var dataItem in data)
            {
                var newRow = result.NewRow();
                foreach (var property in properties)
                {
                    if (columnMapping.ContainsKey(property.Name))
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            newRow[columnMapping[property.Name]] =
                                Enum.GetName(property.PropertyType, property.GetValue(dataItem));
                        }
                        else
                        {
                            newRow[columnMapping[property.Name]] = property.GetValue(dataItem) ?? DBNull.Value;
                        }
                        
                    }
                }
                result.Rows.Add(newRow);
            }

            return result;
        }

        private static bool IsMappableToColumn(PropertyInfo property)
        {
            return property.PropertyType.Namespace == "System" || property.PropertyType.IsEnum;
        }
    }
}

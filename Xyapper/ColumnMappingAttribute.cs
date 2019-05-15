using System;

namespace Xyapper
{
    /// <summary>
    /// Attribute to map a property to specified column name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnMappingAttribute : Attribute
    {
        /// <summary>
        /// Column name to map
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Ignore this property by serializer/deserializer
        /// </summary>
        public bool Ignore { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name to map</param>
        /// <param name="ignore">Ignore this property by serializer/deserializer</param>
        public ColumnMappingAttribute(string columnName = null, bool ignore = false)
        {
            ColumnName = columnName;
            Ignore = ignore;
        }
    }
}

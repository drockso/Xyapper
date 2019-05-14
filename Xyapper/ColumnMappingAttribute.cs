using System;

namespace Xyapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnMappingAttribute : Attribute
    {
        public string ColumnName { get; private set; }

        public bool Ignore { get; private set; }

        public ColumnMappingAttribute(string columnName = null, bool ignore = false)
        {
            ColumnName = columnName;
            Ignore = ignore;
        }
    }
}

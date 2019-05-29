using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NestedMappingAttribute : Attribute
    {
        public int TableIndex { get; set; }
        public string ParentPropertyName { get; set; }
        
        public Type IsChildOf { get; set; }

        public NestedMappingAttribute(int tableIndex, string parentPropertyName = null, Type isChildOf = null)
        {
            TableIndex = tableIndex;
            ParentPropertyName = parentPropertyName;
            IsChildOf = isChildOf;
        }
    }
}

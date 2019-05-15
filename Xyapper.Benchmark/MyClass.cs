using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper.Benchmark
{
    public class MyClass
    {
        public int FieldInt { get; set; }

        public string FieldString { get; set; }

        public DateTime FieldDateTime { get; set; }

        [ColumnMapping(ignore:true)]
        public float FieldToIgnore { get; set; }

        [ColumnMapping(columnName: "FieldInt")]
        public int FieldToMap { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace Xyapper.Benchmark
{
    public class MyClass : ICustomDeserialized
    {
        public int FieldInt { get; set; }

        public string FieldString { get; set; }

        public DateTime? FieldDateTime { get; set; }

        public void Deserialize(IDataRecord record)
        {
            FieldInt = TypeConverter.ToType<int>(record["FieldInt"]);
            FieldString = TypeConverter.ToType<string>(record["FieldString"]);
            FieldDateTime = TypeConverter.ToType<DateTime?>(record["FieldDateTime"]);
        }
    }
}

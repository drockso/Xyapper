using System;
using System.Data;

namespace Xyapper.Tests
{
    class TestType3 : ICustomDeserialized
    {
        public int FieldInt { get; set; }

        public void Deserialize(IDataRecord record)
        {
            throw new Exception("Custom deserializer invoked");
        }
    }
}
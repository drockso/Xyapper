using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Xyapper.Tests
{
    class TestType2 : ICustomDeserialized
    {
        [ColumnMapping(columnName: "column1")]
        public int FieldInt { get; set; }

        public void Deserialize(IDataRecord record)
        {
            FieldInt = record["column2"].ToType<int>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper.Tests
{
    public class TestType
    {
        public long ColumnInt { get; set; }
        public string ColumnString { get; set; }
        public long ColumnDate { get; set; }
        public DateTime? ColumnDateNull { get; set; }
        public double? ColumnDouble { get; set; }

        [ColumnMapping(ignore: true)]
        public int? ColumnInt2 { get; set; }
    }
}

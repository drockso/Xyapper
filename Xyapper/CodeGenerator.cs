using System.Data;
using System.Text;

namespace Xyapper
{
    public static class CodeGenerator
    {
        public static string GenerateClassCode(this DataTable table, string className = "MyClassName", bool generateReader = false)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"public class {className}\r\n{{\r\n");

            foreach (DataColumn column in table.Columns)
            {
                stringBuilder.Append($"\tpublic {column.DataType} {column.ColumnName} {{ get; set; }}\r\n");
            }

            stringBuilder.Append("\r\n\r\n");

            if (generateReader)
            {
                stringBuilder.Append($"\tprivate static {className} FromDataRow(DataRow row)\r\n\t{{\r\n\t\tvar result = new {className}();\r\n\r\n");

                foreach (DataColumn column in table.Columns)
                {
                    stringBuilder.Append($"\t\tresult.{column.ColumnName} = row[\"{column.ColumnName}\"].ToType<{column.DataType}>();\r\n");
                }
                stringBuilder.Append("\r\n\t\treturn result;\r\n\t}\r\n\r\n");

                stringBuilder.Append($"\tpublic static {className}[] FromDataTable(DataTable table)\r\n\t{{\r\n\t\treturn table.Rows.Cast<DataRow>().Select(FromDataRow).ToArray();\r\n\t}}\r\n");
            }

            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}

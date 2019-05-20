using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Xyapper.Internal;

namespace Xyapper
{
    /// <summary>
    /// Code genearator
    /// </summary>
    static class CodeGenerator
    {
        /// <summary>
        /// Generate class definition from schema
        /// </summary>
        /// <param name="schema">List of columns</param>
        /// <param name="className">How to name a new class</param>
        /// <param name="generateCustomDeserializer">Make class ICustomDeserialized</param>
        /// <returns></returns>
        public static string GenerateClassCode(IEnumerable<SchemaItem> schema, string className = "MyClassName", bool generateCustomDeserializer = false)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("using System;\r\n");
            stringBuilder.Append("using System.Data;\r\n");
            stringBuilder.Append("using Xyapper;\r\n");

            stringBuilder.Append("\r\n");

            stringBuilder.Append($"public class {className} {(generateCustomDeserializer ? ": ICustomDeserialized" : null)}\r\n{{\r\n");

            var schemaItems = schema as SchemaItem[] ?? schema.ToArray();
            foreach (var column in schemaItems)
            {
                stringBuilder.Append($"\tpublic {column.DataType}{(TypeConverter.IsNullable(column.DataType) ? "" : "?")} {column.ColumnName} {{ get; set; }}\r\n");
            }

            stringBuilder.Append("\r\n\r\n");

            if (generateCustomDeserializer)
            {
                stringBuilder.Append("\tpublic void Deserialize(IDataRecord record)\r\n\t{\r\n");

                foreach (var column in schemaItems)
                {
                    stringBuilder.Append($"\t\t{column.ColumnName} = record[\"{column.ColumnName}\"].ToType<{column.DataType}{(TypeConverter.IsNullable(column.DataType) ? "" : "?")}>();\r\n");
                }

                stringBuilder.Append("\r\n\t}\r\n");
            }

            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}

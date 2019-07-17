using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Xyapper.Internal
{
    static class CommandLogger
    {
        private static readonly List<Type> QuotedTypes = new List<Type> { typeof(string), typeof(DateTime), typeof(Guid)  };

        public static string DBCommandToString(IDbCommand command)
        {
            if (command.CommandType == CommandType.StoredProcedure)
            {
                return $"EXEC {command.CommandText} " + string.Join(", ",
                           command.Parameters.Cast<IDataParameter>().Select(p =>
                               $"@{p.ParameterName} = {DataParameterValueToString(p)}"));
            }

            if (command.CommandType == CommandType.Text)
            {
                var commandText = command.CommandText;
                foreach (IDataParameter parameter in command.Parameters)
                {
                    commandText = commandText.Replace($"@{parameter.ParameterName}",
                        DataParameterValueToString(parameter));
                }

                return commandText;
            }
            return null;
        }

        private static string DataParameterValueToString(IDataParameter parameter)
        {
            if (parameter.Value == null || parameter.Value == DBNull.Value)
            {
                return "NULL";
            }

            if (QuotedTypes.Contains(parameter.Value.GetType()))
            {
                return $"'{parameter.Value}'";
            }
            else
            {
                return parameter.Value.ToString();
            }

        }
    }
}

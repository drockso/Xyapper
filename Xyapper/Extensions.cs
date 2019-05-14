using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xyapper.Internal;

namespace Xyapper
{
    public static class Extensions
    {
        public static IEnumerable<T> XQuery<T>(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null) where T : new()
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            LogCommand(command);

            using (var reader = command.ExecuteReader())
            {
                var deserializer = DeserializerFactory.GetDeserializer<T>(reader);

                while (reader.Read())
                {
                    yield return deserializer(reader);
                }
            }
        }

        public static IEnumerable<T> XQuery<T>(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null) where T : new()
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                command.Transaction = transaction;

                AddParameters(command, parameterSet);

                foreach(var item in connection.XQuery<T>(command, transaction))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> XQueryProcedure<T>(this IDbConnection connection, string procedureName, object parameterSet = null, IDbTransaction transaction = null) where T : new()
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedureName;
                command.Transaction = transaction;

                AddParameters(command, parameterSet);

                foreach(var item in connection.XQuery<T>(command, transaction))
                {
                    yield return item;
                }
            }
        }

        public static void XExecuteNonQuery(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                command.Transaction = transaction;

                AddParameters(command, parameterSet);

                command.Connection = connection;

                LogCommand(command);

                command.ExecuteNonQuery();
            }
        }


        public static void XExecuteNonQuery(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            LogCommand(command);
            command.ExecuteNonQuery();
        }


        public static DataTable XGetDataTable(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            LogCommand(command);
            using (var reader = command.ExecuteReader())
            {
                return ReadDataTable(reader);
            }
        }

        public static DataTable XGetDataTable(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                command.Transaction = transaction;

                AddParameters(command, parameterSet);
                command.Connection = connection;

                LogCommand(command);

                using (var reader = command.ExecuteReader())
                {
                    return ReadDataTable(reader);
                }
            }
        }

        private static void AddParameters(IDbCommand command, object parameterSet)
        {
            if (parameterSet == null) return;

            var fields = parameterSet.GetType().GetProperties();

            foreach(var field in fields)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = field.Name;
                parameter.Value = field.GetValue(parameterSet);

                command.Parameters.Add(parameter);
            }
        }

        private static DataTable ReadDataTable(IDataReader reader)
        {
            var schemaColumns = SchemaItem.FromDataTable(reader.GetSchemaTable());

            var result = new DataTable();
            foreach(var column in schemaColumns)
            {
                result.Columns.Add(column.ColumnName, column.DataType);
            }

            foreach(var rowArray in ReadRowArray(reader, schemaColumns.Length))
            {
                result.Rows.Add(rowArray);
            }

            return result;
        }

        private static IEnumerable<object[]> ReadRowArray(IDataReader reader, int columns)
        {
            while (reader.Read())
            {
                var rowArray = new object[columns];
                reader.GetValues(rowArray);
                yield return rowArray;
            }
        }

       
        private static void OpenIfNot(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        private static void LogCommand(IDbCommand command)
        {
            if (!XyapperManager.EnableLogging) return;

            XyapperManager.Logger.Log(XyapperManager.CommandLogLevel, new EventId(), command, null, (cmd, exception) => 
            {
                var message = command.CommandText;
                if (command.Parameters.Count > 0)
                {
                    message += $"\r\nPARAMETERS: \r\n{string.Join("\r\n", command.Parameters.Cast<IDbDataParameter>().Select(parameter => $"{parameter.ParameterName} = '{parameter.Value.ToString()}'"))}";
                }
                return message;
            });
        }

        private static void LogException(Exception exception)
        {
            if (!XyapperManager.EnableLogging) return;

            XyapperManager.Logger.Log<IDbCommand>(XyapperManager.ExceptionLogLevel, new EventId(), null, exception, (cmd, ex) =>
            {
                return $"Execution exception: {ex.Message}";
            });
        }
    }
}

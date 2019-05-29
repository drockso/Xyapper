using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xyapper.Internal;

namespace Xyapper
{
    /// <summary>
    /// Xyapper main extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get list of objects from database query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static IEnumerable<T> XQuery<T>(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null) where T : new()
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();

            using (var reader = command.ExecuteReader())
            {
                var deserializer = DeserializerFactory.GetDeserializer<T>(reader);

                while (reader.Read())
                {
                    yield return deserializer(reader);
                }
            }
        }

        /// <summary>
        /// Get list of objects from database query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static IEnumerable<T> XQuery<T>(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null) where T : new()
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                foreach(var item in connection.XQuery<T>(command, transaction))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Get list of objects from database stored procedure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="procedureName">Stored procedure name</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
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

        /// <summary>
        /// Execute command with no return data
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        public static void XExecuteNonQuery(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                connection.XExecuteNonQuery(command, transaction);
            }
        }

        /// <summary>
        /// Execute command with no return data
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        public static void XExecuteNonQuery(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Get DataTable from DB
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static DataTable XGetDataTable(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();

            using (var reader = command.ExecuteReader())
            {
                return ReadDataTable(reader);
            }
        }


        /// <summary>
        /// Get DataTable from DB
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static DataTable XGetDataTable(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                return connection.XGetDataTable(command, transaction);
            }
        }

        /// <summary>
        /// Get a DataSet from multi-statement query
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static DataSet XGetDataSet(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();
            
            var result = new DataSet();

            using (var reader = command.ExecuteReader())
            {
                do
                {
                    result.Tables.Add(ReadDataTable(reader));
                } while (reader.NextResult());
            }

            return result;
        }

        /// <summary>
        /// Get a DataSet from multi-statement query
        /// </summary>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <param name="commandType">Query or stored procedure</param>
        /// <returns></returns>
        public static DataSet XGetDataSet(this IDbConnection connection, string commandText, object parameterSet = null, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                command.CommandType = commandType;
                return connection.XGetDataSet(command, transaction);
            }
        }

        /// <summary>
        /// Get a single record from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static T XQuerySingle<T>(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null) where T : new()
        {
            return connection.XQuery<T>(command, transaction).FirstOrDefault();
        }

        /// <summary>
        /// Get a single record from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static T XQuerySingle<T>(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null) where T : new()
        {
            return connection.XQuery<T>(commandText, parameterSet, transaction).FirstOrDefault();
        }

        /// <summary>
        /// Get a single value from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static T XQueryScalar<T>(this IDbConnection connection, IDbCommand command, IDbTransaction transaction)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();

            return command.ExecuteScalar().ToType<T>();
        }

        /// <summary>
        /// Get a single value from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static T XQueryScalar<T>(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                return connection.XQueryScalar<T>(command, transaction);
            }
        }

        /// <summary>
        /// Get schema info for specified command
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static SchemaItem[] XGetSchema(this IDbConnection connection, IDbCommand command, IDbTransaction transaction)
        {
            connection.OpenIfNot();
            command.Connection = connection;
            command.Transaction = transaction;

            command.Log();

            using (var reader = command.ExecuteReader())
            {
                var schemaColumns = SchemaItem.FromDataTable(reader.GetSchemaTable());
                return schemaColumns;
            }
        }

        /// <summary>
        /// Get schema info for specified command
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static SchemaItem[] XGetSchema(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                return connection.XGetSchema(command, transaction);
            }
        }

        /// <summary>
        /// Create a class from DB table result
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="command">DB Command to execute</param>
        /// <param name="transaction">Transaction to use</param>
        /// <param name="className">How to name a new class</param>
        /// <param name="generateCustomDeserializer">Make a class ICustomDeserialized</param>
        /// <returns></returns>
        public static string XGenerateClassCode(
            this IDbConnection connection, 
            IDbCommand command, 
            string className = "MyClassName", 
            bool generateCustomDeserializer = false, 
            IDbTransaction transaction = null)
        {
            var schema = connection.XGetSchema(command, transaction);
            return CodeGenerator.GenerateClassCode(schema, className, generateCustomDeserializer);
        }

        /// <summary>
        /// Create a class from DB table result
        /// </summary>
        /// <param name="connection">DB Connection</param>
        /// <param name="commandText">Plain command text</param>
        /// <param name="parameterSet">Anonymous type object with parameters</param>
        /// <param name="className">How to name a new class</param>
        /// <param name="generateCustomDeserializer">Make a class ICustomDeserialized</param>
        /// <param name="transaction">Transaction to use</param>
        /// <returns></returns>
        public static string XGenerateClassCode(this IDbConnection connection, string commandText,
            object parameterSet = null, string className = "MyClassName", bool generateCustomDeserializer = false,
            IDbTransaction transaction = null)
        {
            using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
            {
                return connection.XGenerateClassCode(command, className, generateCustomDeserializer, transaction);
            }
        }

        //public static List<T> XQueryNested<T>(this IDbConnection connection, IDbCommand command, IDbTransaction transaction = null)
        //{
        //    connection.OpenIfNot();
        //    command.Connection = connection;
        //    command.Transaction = transaction;

        //    command.Log();

        //    using (var reader = command.ExecuteReader())
        //    {
        //        return NestedReader.ReadNested<T>(reader);
        //    }
        //}

        //public static List<T> XQueryNested<T>(this IDbConnection connection, string commandText, object parameterSet = null, IDbTransaction transaction = null)
        //{
        //    using (var command = connection.CreateCommandWithParameters(commandText, parameterSet))
        //    {
        //        return connection.XQueryNested<T>(command, transaction);
        //    }
        //}


        /// <summary>
            /// Add parameters to command from anonymous type
            /// </summary>
            /// <param name="command"></param>
            /// <param name="parameterSet"></param>
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

        /// <summary>
        /// Create an IDCommand from text and parameter set
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="parameterSet"></param>
        /// <returns></returns>
        private static IDbCommand CreateCommandWithParameters(this IDbConnection connection, string commandText, object parameterSet)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            AddParameters(command, parameterSet);

            return command;
        }

        /// <summary>
        /// Read data to DataTable from IDataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Read a collections of arrays of objects from IDataReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        private static IEnumerable<object[]> ReadRowArray(IDataReader reader, int columns)
        {
            while (reader.Read())
            { 
                var rowArray = new object[columns];
                reader.GetValues(rowArray);

                if (XyapperManager.TrimStrings)
                {
                    for(int i = 0; i < rowArray.Length; i++)
                    {
                        var value = rowArray[i];
                        if (value is string)
                        {
                            rowArray[i] = value.ToString().Trim();
                        }
                    }
                }
                
                yield return rowArray;
            }
        }

        /// <summary>
        /// Open a connection of it is not open
        /// </summary>
        /// <param name="connection"></param>
        private static void OpenIfNot(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        /// <summary>
        /// Log command to a logging provider
        /// </summary>
        /// <param name="command"></param>
        private static void Log(this IDbCommand command)
        {
            if (!XyapperManager.EnableLogging) return;

            XyapperManager.Logger.Log(XyapperManager.CommandLogLevel, new EventId(), command, null,
                (cmd, exception) => CommandLogger.DBCommandToString(cmd));

        }
    }
}

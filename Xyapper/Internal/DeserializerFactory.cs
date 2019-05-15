using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace Xyapper.Internal
{
    /// <summary>
    /// Factory to create runtime deserializers for requested type
    /// </summary>
    public static class DeserializerFactory
    {
        private static Dictionary<Type, object> Deserializers = new Dictionary<Type, object>();

        private static object LockObject = new object();

        /// <summary>
        /// Get or create a deserializer for requested type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static Func<IDataRecord, T> GetDeserializer<T>(IDataReader dataReader) where T : new()
        {
            var requiredType = typeof(T);

            lock (LockObject)
            {
                if (Deserializers.ContainsKey(requiredType))
                {
                    return (Func<IDataRecord, T>)Deserializers[requiredType];
                }

                Func<IDataRecord, T> newDeserializer;
                if (requiredType.GetInterfaces().Contains(typeof(ICustomDeserialized)))
                {
                    newDeserializer = CreateCustomDeserializer<T>(dataReader);
                }
                else
                {
                    newDeserializer = CreateExpression<T>(dataReader).Compile();
                }
             
                Deserializers.Add(requiredType, newDeserializer);

                return newDeserializer;
            }
        }

        /// <summary>
        /// Geneaate expression with the deserializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private static Expression<Func<IDataRecord, T>> CreateExpression<T>(IDataReader dataReader) where T : new()
        {
            var returnType = typeof(T);
            var dataReaderParam = Expression.Parameter(typeof(IDataRecord), "dataReader");
            var indexProperty = typeof(IDataRecord).GetProperty("Item", new[] { typeof(string) });

            var readerColumns = SchemaItem.GetSchemas(dataReader).Select(column => column.ColumnName).ToArray();

            var constructor = Expression.New(typeof(T));

            var assignments = new List<MemberAssignment>();

            var properties = returnType.GetProperties();
            foreach (var property in properties)
            {
                var mappingAttribute = property.GetCustomAttribute<ColumnMappingAttribute>();
                var targetColumnName = mappingAttribute != null ? mappingAttribute.ColumnName : property.Name;
                if (!readerColumns.Contains(targetColumnName)) continue;

                var propertyExpression = Expression.Property(dataReaderParam, indexProperty, Expression.Constant(targetColumnName));

                var conditionalExpression = Expression.Condition(Expression.Equal(propertyExpression, Expression.Constant(DBNull.Value)), Expression.Constant(null), propertyExpression);

                var convertExpression = Expression.Convert(conditionalExpression, property.PropertyType);

                assignments.Add(Expression.Bind(property, convertExpression));
            }
        
            var block = Expression.MemberInit(constructor, assignments.ToArray());
            return Expression.Lambda<Func<IDataRecord, T>>(block, dataReaderParam);
        }

        /// <summary>
        /// Create a deserializer for ICustomDeserialized derived type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private static Func<IDataRecord, T> CreateCustomDeserializer<T>(IDataReader dataReader) where T : new()
        {
            Func<IDataRecord, T> deserializer = (record) => 
            {
                var newItem = new T();
                ((ICustomDeserialized) newItem).Deserialize(record);
                return newItem;
            };

            return deserializer;
        }
    }
}

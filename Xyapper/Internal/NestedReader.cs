using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xyapper.Internal
{
    static class NestedReader
    {
        public static List<T> ReadNested<T>(IDataReader reader)
        {
            if (!typeof(T).GetCustomAttributes<NestedMappingAttribute>().Any())
            {
                throw new XyapperException("Root class for nested deserialization must have [NestedMappingAttribute]!");
            }

            var nestedAttributes = CollectAttributes(typeof(T));
            var dataSet = new Dictionary<Type, List<object>>();

            var readerPosition = 0;
            do
            {
                var type = nestedAttributes.FirstOrDefault(a => a.Item2.TableIndex == readerPosition);
                if (type != null)
                {
                    var userType = type.Item1;
                    Func<IDataRecord, object> deserializer = (Func<IDataRecord, object>) typeof(DeserializerFactory)
                        .GetMethod(nameof(DeserializerFactory.GetDeserializer)).MakeGenericMethod(userType)
                        .Invoke(null, new[] {reader});

                    var data = new List<object>();
                    while (reader.Read())
                    {
                        data.Add(deserializer(reader));
                    }

                    dataSet.Add(userType, data);
                }
                readerPosition++;
            } while (reader.NextResult());

            for (int i = 0; i < readerPosition; i++)
            {
                var ty = nestedAttributes.FirstOrDefault(a => a.Item2.TableIndex == i);
                if(ty == null || ty.Item2.IsChildOf == null) continue;
                

            }

            return dataSet[typeof(T)].Cast<T>().ToList();
        }

        public static List<Tuple<Type, NestedMappingAttribute>> CollectAttributes(Type type)
        {
            var result = new List<Tuple<Type, NestedMappingAttribute>>();
            result.Add(new Tuple<Type, NestedMappingAttribute>(type, type.GetCustomAttribute<NestedMappingAttribute>()));

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.GetCustomAttributes<NestedMappingAttribute>().Any())
                {
                    result = result.Concat(CollectAttributes(property.PropertyType)).ToList();
                }

                if (property.PropertyType.GetGenericArguments().Any() && property.PropertyType.GetGenericArguments()[0].GetCustomAttributes<NestedMappingAttribute>().Any())
                {
                    result = result.Concat(CollectAttributes(property.PropertyType.GetGenericArguments()[0])).ToList();
                }
            }

            return result;
        }
    }
}

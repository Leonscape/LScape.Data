using LScape.Data.Mapping;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace LScape.Data.Extensions
{
    /// <summary>
    /// Extension methods for IDataReader
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Performs async read on a IDataReader
        /// </summary>
        /// <param name="reader">The reader to perform async read on</param>
        /// <returns></returns>
        public static async Task<bool> ReadAsync(this IDataReader reader)
        {
            if (reader is DbDataReader dbReader)
                return await dbReader.ReadAsync();

            return await Task.Run(() => reader.Read());
        }

        /// <summary>
        /// Gets the value of a field in the current IDataReader row
        /// </summary>
        /// <typeparam name="T">The type of the field</typeparam>
        /// <param name="reader">The reader to get the value from</param>
        /// <param name="name">The name of the field</param>
        public static T GetValue<T>(this IDataReader reader, string name)
        {
            var rst = GetValue(reader, name, typeof(T));
            if (rst is T result)
                return result;
            return default(T);
        }

        /// <summary>
        /// Gets the value of a field in the current IDataReader row
        /// </summary>
        /// <param name="reader">The reader to get the value from</param>
        /// <param name="i">The index of the field</param>
        /// <param name="type">The type of the field</param>
        public static object GetValue(this IDataReader reader, int i, Type type)
        {
            var value = reader[i];
            return GetValue(type, value);
        }

        /// <summary>
        /// Gets the value of a field in the current IDataReader row
        /// </summary>
        /// <param name="reader">The reader to get the value from</param>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The type of the field</param>
        /// <returns></returns>
        public static object GetValue(this IDataReader reader, string name, Type type)
        {
            var value = reader[name];
            return GetValue(type, value);
        }

        private static object GetValue(Type type, object value)
        {
            if (value == DBNull.Value)
                return null;

            if (TypeMapping.IsEnum(type, out var enumType) && Enum.IsDefined(enumType, value))
                value = Enum.ToObject(enumType, value);

            return value;
        }
    }
}

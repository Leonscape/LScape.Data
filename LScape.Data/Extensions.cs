using LScape.Data.Mapping;
using System;
using System.Data;

namespace LScape.Data
{
    /// <summary>
    /// Extension methods for reader and writing to a database
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add a parameter to a command
        /// </summary>
        /// <param name="command">The command to add the parameter to</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddParameter(this IDbCommand command, string name, object value)
        {
            AddParameter(command, name, TypeMapping.GetDbType(value.GetType()), value);
        }

        /// <summary>
        /// Add a parameter to a command
        /// </summary>
        /// <param name="command">The command to add the parameter to</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="type">The type of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public static void AddParameter(this IDbCommand command, string name, DbType type, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = type;
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Gets the value of a field in the current datareader row
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
        /// Gets the value of a field in the current datareader row
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
        /// Gets the value of a field in the current datareader row
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

            if (TypeMapping.IsEnum(ref type) && Enum.IsDefined(type, value))
                value = Enum.ToObject(type, value);

            return value;
        }
    }
}

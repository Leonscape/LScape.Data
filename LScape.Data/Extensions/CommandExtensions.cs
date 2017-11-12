using LScape.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;

namespace LScape.Data.Extensions
{
    /// <summary>
    /// Provides extensions for IDbCommand
    /// </summary>
    public static class CommandExtensions
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
        /// Adds parameters from an anonymous type
        /// </summary>
        /// <param name="command">The command to add the properties to</param>
        /// <param name="parameters">The anonymous object that contains the parameters</param>
        public static void AddParameters(this IDbCommand command, object parameters)
        {
            if (parameters == null)
                return;

            var paramType = parameters.GetType();
            foreach (var propertyInfo in paramType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = propertyInfo.Name;
                parameter.Value = propertyInfo.GetValue(parameters) ?? DBNull.Value;
                parameter.DbType = TypeMapping.GetDbType(propertyInfo.PropertyType);
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Add parameters from an anonymous type and add to where clause
        /// </summary>
        /// <param name="command">THe command to add the parameters and where clause to</param>
        /// <param name="parameters">The anonymous object that contains the parameters</param>
        /// <remarks>Only works for sql command text. appends the where clause to the end with the property names</remarks>
        public static void AddParametersWithWhere(this IDbCommand command, object parameters)
        {
            if (parameters == null)
                return;

            var paramType = parameters.GetType();
            var whereList = new List<string>();
            foreach (var propertyInfo in paramType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = propertyInfo.Name;
                parameter.Value = propertyInfo.GetValue(parameters) ?? DBNull.Value;
                parameter.DbType = TypeMapping.GetDbType(propertyInfo.PropertyType);
                command.Parameters.Add(parameter);

                if (parameter.Value is string valStr && valStr.Contains("%"))
                    whereList.Add($"[{propertyInfo.Name}] LIKE @{propertyInfo.Name}");
                else
                    whereList.Add($"[{propertyInfo.Name}] = @{propertyInfo.Name}");
            }

            if (whereList.Count > 0)
            {
                if (command.CommandText.Trim().EndsWith("and", StringComparison.InvariantCultureIgnoreCase))
                    command.CommandText += string.Join(" AND ", whereList);
                else if (command.CommandText.IndexOf("where", 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                    command.CommandText += $" AND {string.Join(" AND ", whereList)}";
                else
                    command.CommandText += $" WHERE {string.Join(" AND ", whereList)}";
            }

        }

        /// <summary>
        /// Provides Async for ExecuteReader
        /// </summary>
        /// <param name="command">The command to perform async ExecuteReader on</param>
        public static async Task<IDataReader> ExecuteReaderAsync(this IDbCommand command)
        {
            if (command is DbCommand dbCommand)
                return await dbCommand.ExecuteReaderAsync();

            return await Task.Run(() => command.ExecuteReader());
        }

        /// <summary>
        /// Provides Async for ExecuteNonQuery
        /// </summary>
        /// <param name="command">The command to perform async ExecuteNonQuery on</param>
        public static async Task<int> ExecuteNonQueryAsync(this IDbCommand command)
        {
            if (command is DbCommand dbCommand)
                return await dbCommand.ExecuteNonQueryAsync();

            return await Task.Run(() => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Provides Async for ExecuteScalar
        /// </summary>
        /// <param name="command">The command to perform async ExecuteScalar on</param>
        public static async Task<object> ExecuteScalarAsync(this IDbCommand command)
        {
            if (command is DbCommand dbCommand)
                return await dbCommand.ExecuteScalarAsync();

            return await Task.Run(() => command.ExecuteScalar());
        }
    }
}

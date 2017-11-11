using LScape.Data.Mapping;
using System;
using System.Data;
using System.Data.Common;
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

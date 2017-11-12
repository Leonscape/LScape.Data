using LScape.Data.Mapping;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;

namespace LScape.Data.Extensions
{
    /// <summary>
    /// Extensions to the <see cref="IDbConnection" /> uinterface for direct querying
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Queries the database and attempts to create an object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Any parameters to the query</param>
        public static T Query<T>(this IDbConnection connection, string query, object parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(query))
            {
                cmd.AddParameters(parameters);
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? map.Create(reader) : null;
            }
        }
        
        /// <summary>
        /// Queries the database asynchronously and attempts to create an object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// /// <param name="parameters">Any parameters to the query</param>
        public static async Task<T> QueryAsync<T>(this IDbConnection connection, string query, object parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(query))
            {
                cmd.AddParameters(parameters);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await reader.ReadAsync() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Queries the database and attempts to create an enumerable of object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Any parameters to the query</param>
        public static IEnumerable<T> QueryMany<T>(this IDbConnection connection, string query, object parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(query))
            {
                cmd.AddParameters(parameters);
                using (var reader = cmd.ExecuteReader())
                    return map.CreateEnumerable(reader);
            }
        }

        /// <summary>
        /// Queries the database asynchronously and attempts to create an enumerable of object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Any parameters to the query</param>
        public static async Task<IEnumerable<T>> QueryManyAsync<T>(this IDbConnection connection, string query, object parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(query))
            {
                cmd.AddParameters(parameters);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await map.CreateEnumerableAsync(reader);
            }
        }
        
        /// <summary>
        /// Gets a specific object from the database that matches the condition, or if not found null
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="connection">The connection to quuery against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns</param>
        public static T Get<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand(map.SelectStatement))
            {
                cmd.AddParametersWithWhere(condition);
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Gets a specific object from the database that matches the condition, or if not found null
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="connection">The connection to quuery against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns</param>
        public static async Task<T> GetAsync<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand(map.SelectStatement))
            {
                cmd.AddParametersWithWhere(condition);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await reader.ReadAsync() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Gets all the objects from the database that match the condition
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns, or null for all</param>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand(map.SelectStatement))
            {
                cmd.AddParametersWithWhere(condition);
                using (var reader = cmd.ExecuteReader())
                    return map.CreateEnumerable(reader);
            }
        }

        /// <summary>
        /// Gets all the objects from the database that match the condition
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns, or null for all</param>
        public static async Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand(map.SelectStatement))
            {
                cmd.AddParametersWithWhere(condition);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await map.CreateEnumerableAsync(reader);
            }
        }

        /// <summary>
        /// Get the number of objects in the database that match a condition
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">An anon type with the name of columns, or null for all</param>
        public static int Count<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            
            using (var cmd = connection.TextCommand(map.CountStatement))
            {
                cmd.AddParametersWithWhere(condition);
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Get the number of objects in the database that match a condition
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">An anon type with the name of columns, or null for all</param>
        public static async Task<int> CountAsync<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);

            using (var cmd = connection.TextCommand(map.CountStatement))
            {
                cmd.AddParametersWithWhere(condition);
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        /// <summary>
        /// Inserts an object into the database
        /// </summary>
        /// <typeparam name="T">The type of object to insert</typeparam>
        /// <param name="connection">The connection to insert on</param>
        /// <param name="entity">The actual object to insert</param>
        public static T Insert<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.InsertStatement))
            {
                map.AddParameters(cmd, entity);
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Inserts an object into the database asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to insert</typeparam>
        /// <param name="connection">The connection to insert on</param>
        /// <param name="entity">The actual object to insert</param>
        public static async Task<T> InsertAsync<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.InsertStatement))
            {
                map.AddParameters(cmd, entity);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await reader.ReadAsync() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Updates an object in the database
        /// </summary>
        /// <typeparam name="T">The type of object to update</typeparam>
        /// <param name="connection">The connection to update on</param>
        /// <param name="entity">The actual object to update</param>
        public static T Update<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.UpdateStatement))
            {
                map.AddParameters(cmd, entity, true);
                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Updates an object in the database asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to update</typeparam>
        /// <param name="connection">The connection to update on</param>
        /// <param name="entity">The actual object to update</param>
        public static async Task<T> UpdateAsync<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.UpdateStatement))
            {
                map.AddParameters(cmd, entity, true);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await reader.ReadAsync() ? map.Create(reader) : null;
            }
        }

        /// <summary>
        /// Deletes an entity from the database
        /// </summary>
        /// <typeparam name="T">The type of object to delete</typeparam>
        /// <param name="connection">The database connection</param>
        /// <param name="entity">The object to delete</param>
        public static int Delete<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.DeleteStatement))
            {
                map.AddKeyParameters(cmd, entity);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delete object from a database that match the condition
        /// </summary>
        /// <typeparam name="T">The type of objects to delete</typeparam>
        /// <param name="connection">The connection to the database</param>
        /// <param name="condition">an anonymous object holding the condition for deletion</param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand($"DELETE FROM [{map.TableName}]"))
            {
                cmd.AddParametersWithWhere(condition);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes an entity from the database
        /// </summary>
        /// <typeparam name="T">The type of object to delete</typeparam>
        /// <param name="connection">The database connection</param>
        /// <param name="entity">The object to delete</param>
        public static async Task<int> DeleteAsync<T>(this IDbConnection connection, T entity) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.TextCommand(map.DeleteStatement))
            {
                map.AddKeyParameters(cmd, entity);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Delete object from a database that match the condition
        /// </summary>
        /// <typeparam name="T">The type of objects to delete</typeparam>
        /// <param name="connection">The connection to the database</param>
        /// <param name="condition">an anonymous object holding the condition for deletion</param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            using (var cmd = connection.TextCommand($"DELETE FROM [{map.TableName}]"))
            {
                cmd.AddParametersWithWhere(condition);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Creates a sql text command
        /// </summary>
        /// <param name="connection">The connection for the command</param>
        /// <param name="text">The command text</param>
        /// <param name="parameters">The parameters for the command</param>
        public static IDbCommand TextCommand(this IDbConnection connection, string text, object parameters = null)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = text;
            
            if (parameters != null)
                cmd.AddParameters(parameters);

            return cmd;
        }

        /// <summary>
        /// Creates a stored procedure command
        /// </summary>
        /// <param name="connection">The connection for the command</param>
        /// <param name="procedure">The procedure name</param>
        /// <param name="parameters">The parameters for the command</param>
        public static IDbCommand ProcCommand(this IDbConnection connection, string procedure, object parameters = null)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = procedure;
            cmd.CommandType = CommandType.StoredProcedure;
            
            if (parameters != null)
                cmd.AddParameters(parameters);

            return cmd;
        }

        private static object CheckKey(object inKey, string keyName)
        {
            if (inKey == null)
                return null;

            var outKey = inKey;
            if (TypeMapping.IsMappableType(inKey.GetType()))
            {
                var key = new ExpandoObject();
                ((IDictionary<string, object>)key).Add(keyName, inKey);
                outKey = key;
            }

            return outKey;
        }
    }
}

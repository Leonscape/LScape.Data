using LScape.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
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
        public static T Query<T>(this IDbConnection connection, string query, dynamic parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            return Reader(connection, query, r => r.Read() ? map.Create(r) : null, (object)parameters);
        }
        
        /// <summary>
        /// Queries the database asynchronously and attempts to create an object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// /// <param name="parameters">Any parameters to the query</param>
        public static async Task<T> QueryAsync<T>(this IDbConnection connection, string query, dynamic parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            return await ReaderAsync(connection, query, async r => await r.ReadAsync() ? map.Create(r) : null, (object)parameters);
        }

        /// <summary>
        /// Queries the database and attempts to create an enumerable of object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Any parameters to the query</param>
        public static IEnumerable<T> QueryMany<T>(this IDbConnection connection, string query, dynamic parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            return Reader(connection, query, r => map.CreateEnumerable(r), (object)parameters);
        }

        /// <summary>
        /// Queries the database asynchronously and attempts to create an enumerable of object from the result
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="connection">The connection to query on</param>
        /// <param name="query">The query</param>
        /// <param name="parameters">Any parameters to the query</param>
        public static async Task<IEnumerable<T>> QueryManyAsync<T>(this IDbConnection connection, string query, dynamic parameters = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            return await ReaderAsync(connection, query, async r => await map.CreateEnumerableAsync(r), (object)parameters);
        }

        /// <summary>
        /// Gets a specific object from the database with a key
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="connection">The connection to quuery against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns</param>
        public static T Get<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            return Reader(connection, $"SELECT {map.SelectColumnList} FROM [{map.TableName}] WHERE {AddWhere(condition)}", r => r.Read() ? map.Create(r) : null, condition);
        }

        /// <summary>
        /// Gets a specific object from the database asynchronously with a key
        /// </summary>
        /// <typeparam name="T">The type of object to get</typeparam>
        /// <param name="connection">The connection to quuery against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns</param>
        public static async Task<T> GetAsync<T>(this IDbConnection connection, object condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            condition = CheckKey(condition, map.KeyName);
            return await ReaderAsync(connection, $"SELECT {map.SelectColumnList} FROM [{map.TableName}] WHERE {AddWhere(condition)}", async r => await r.ReadAsync() ? map.Create(r) : null, condition);
        }

        /// <summary>
        /// Gets all the objects from the database
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns, or null for all</param>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            var where = "";
            if (condition != null)
            {
                condition = CheckKey(condition, map.KeyName);
                where = $" WHERE {AddWhere(condition)}";
            }
            return Reader(connection, $"SELECT {map.SelectColumnList} FROM [{map.TableName}]{where}", r => map.CreateEnumerable(r), condition);
        }

        /// <summary>
        /// Gets all the objects from the database asynchronously
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">Either the value of a single key entity, or a anon type with the name of columns, or null for all</param>
        public static async Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, object condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            var where = "";
            if (condition != null)
            {
                condition = CheckKey(condition, map.KeyName);
                where = $" WHERE {AddWhere(condition)}";
            }
            return await ReaderAsync(connection, $"SELECT {map.SelectColumnList} FROM [{map.TableName}]{where}", async r => await map.CreateEnumerableAsync(r), condition);
        }

        /// <summary>
        /// Get the number of objects in the database
        /// </summary>
        /// <typeparam name="T">The typ eof object to get</typeparam>
        /// <param name="connection">The connection to query against</param>
        /// <param name="condition">An anon type with the name of columns, or null for all</param>
        public static int Count<T>(this IDbConnection connection, dynamic condition = null) where T : class, new()
        {
            var map = Mapper.Map<T>();
            var where = "";
            if (condition != null)
            {
                condition = CheckKey(condition, map.KeyName);
                where = $" WHERE {AddWhere(condition)}";
            }
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT({map.KeyName} FROM [{map.TableName}]{where}";
                if(condition != null)
                    cmd.AddParameters((object)condition);

                return (int)cmd.ExecuteScalar();
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
            return Reader(connection, $"INSERT INTO [{map.TableName}] ({map.InsertColumnList}) OUTPUT INSERTED.* VALUES({map.InsertParameterList})", r => r.Read() ? map.Create(r) : null, entity, map);
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
            return await ReaderAsync(connection, $"INSERT INTO [{map.TableName}] ({map.InsertColumnList}) OUTPUT INSERTED.* VALUES({map.InsertParameterList})", async r => await r.ReadAsync() ? map.Create(r) : null, entity, map);
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
            return Reader(connection, $"UPDATE [{map.TableName}] SET {map.UpdateSetString} OUTPUT INSERTED.* WHERE {map.KeyWhere}", r => r.Read() ? map.Create(r) : null, entity, map, true);
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
            return await ReaderAsync(connection, $"UPDATE [{map.TableName}] SET {map.UpdateSetString} OUTPUT INSERTED.* WHERE {map.KeyWhere}", async r => await r.ReadAsync() ? map.Create(r) : null, entity, map, true);
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
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM [{map.TableName}] WHERE {map.KeyWhere}";
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
        public static int Delete<T>(this IDbConnection connection, dynamic condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.CreateCommand())
            {
                condition = CheckKey(condition, map.KeyName);
                cmd.CommandText = $"DELETE FROM [{map.TableName}] WHERE {AddWhere(condition)}";
                cmd.AddParameters((object)condition);
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
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"DELTE FROM [{map.TableName}] WHERE {map.KeyWhere}";
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
        public static async Task<int> DeleteAsync<T>(this IDbConnection connection, dynamic condition) where T : class, new()
        {
            var map = Mapper.Map<T>();
            using (var cmd = connection.CreateCommand())
            {
                condition = CheckKey(condition, map.KeyName);
                cmd.CommandText = $"DELETE FROM [{map.TableName}] WHERE {AddWhere(condition)}";
                cmd.AddParameters((object)condition);
                return await cmd.ExecuteNonQueryAsync();
            }
        }


        private static T Reader<T>(IDbConnection connection, string query, Func<IDataReader, T> readFunc, object parameters = null, IMap map = null, bool includeKeys = false)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                SetParameters(parameters, map, cmd, includeKeys);
                using (var reader = cmd.ExecuteReader())
                    return readFunc(reader);
            }
        }

        private static async Task<T> ReaderAsync<T>(IDbConnection connection, string query, Func<IDataReader, Task<T>> readFunc, object parameters = null, IMap map = null, bool includeKeys = false)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                SetParameters(parameters, map, cmd, includeKeys);
                using (var reader = await cmd.ExecuteReaderAsync())
                    return await readFunc(reader);
            }
        }

        private static void SetParameters(object parameters, IMap map, IDbCommand cmd, bool includeKeys)
        {
            if (map != null)
                map.AddParameters(cmd, parameters, includeKeys);
            else if (parameters != null)
                cmd.AddParameters(parameters);
        }

        private static object CheckKey(object inKey, string keyName)
        {
            var outKey = inKey;
            if (TypeMapping.IsMappableType(inKey.GetType()))
            {
                var key = new ExpandoObject();
                ((IDictionary<string, object>)key).Add(keyName, inKey);
                outKey = key;
            }

            return outKey;
        }

        private static string AddWhere(object keys)
        {
            var keysType = keys.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return string.Join(", ", keysType.Select(pi => $"[{pi.Name}] = @{pi.Name}"));
        }
    }
}

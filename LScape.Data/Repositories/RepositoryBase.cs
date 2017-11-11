using LScape.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LScape.Data.Extensions;

namespace LScape.Data.Repositories
{
    /// <summary>
    /// A basic repository for use with dbcommands
    /// </summary>
    /// <typeparam name="T">The type of object his repository is for</typeparam>
    /// <typeparam name="TKey">The type of the key for this repository</typeparam>
    public abstract class RepositoryBase<T, TKey> : IRepository<T, TKey> where T : class, new() where TKey : struct
    {
        /// <summary>
        /// The connection provider to use
        /// </summary>
        protected readonly IConnectionProvider Provider;

        /// <summary>
        /// The object map to use
        /// </summary>
        protected readonly Map<T> Map;

        /// <summary>
        /// Basic Constructor 
        /// </summary>
        /// <param name="provider">The connection provider to use</param>
        protected RepositoryBase(IConnectionProvider provider)
        {
            Provider = provider;
            Map = Mapper.Map<T>();
        }

        /// <inheritdoc/>
        public virtual T Find(TKey key)
        {
            using (var conn = Provider.Connection())
            {
                using (var command = SelectCommand(conn))
                {
                    command.AddParameter(Map.KeyName, TypeMapping.GetDbType(typeof(TKey)), key);
                    using (var reader = command.ExecuteReader())
                        return reader.Read() ? Map.Create(reader) : null;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<T> FindAsync(TKey key)
        {
            using (var conn = await Provider.ConnectionAsync())
            {
                using (var command = SelectCommand(conn))
                {
                    command.AddParameter(Map.KeyName, TypeMapping.GetDbType(typeof(TKey)), key);
                    using (var reader = await command.ExecuteReaderAsync())
                        return await reader.ReadAsync() ? Map.Create(reader) : null;
                }
            }
        }

        /// <inheritdoc/>
        public virtual int Count()
        {
            using (var conn = Provider.Connection())
            {
                using (var command = CountCommand(conn))
                {
                    return (int)command.ExecuteScalar();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync()
        {
            using (var conn = await Provider.ConnectionAsync())
            {
                using (var command = CountCommand(conn))
                {
                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> All()
        {
            using (var conn = Provider.Connection())
            {
                using (var command = AllCommand(conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        return Map.CreateEnumerable(reader);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> AllAsync()
        {
            using (var conn = await Provider.ConnectionAsync())
            {
                using (var command = AllCommand(conn))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await Map.CreateEnumerableAsync(reader);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual T Save(T entity)
        {
            using (var conn = Provider.Connection())
            {
                IDbCommand command;
                if (Map.KeyValue<TKey>(entity).Equals(default(TKey)))
                {
                    command = InsertCommand(conn);
                    Map.AddParameters(command, entity);
                }
                else
                {
                    command = UpdateCommand(conn);
                    Map.AddParameters(command, entity, true);
                }

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return Map.Create(reader);

                        throw new ArgumentException($"Error while saving the entity of type {typeof(T)}", nameof(entity));
                    }
                }
                finally
                {
                    command.Dispose();
                }

            }
        }

        /// <inheritdoc/>
        public virtual async Task<T> SaveAsync(T entity)
        {
            using (var conn = await Provider.ConnectionAsync())
            {
                IDbCommand command;
                if (Map.KeyValue<TKey>(entity).Equals(default(TKey)))
                {
                    command = InsertCommand(conn);
                    Map.AddParameters(command, entity);
                }
                else
                {
                    command = UpdateCommand(conn);
                    Map.AddParameters(command, entity, true);
                }

                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return Map.Create(reader);

                        throw new ArgumentException($"Error while saving the entity of type {typeof(T)}", nameof(entity));
                    }
                }
                finally
                {
                    command.Dispose();
                }

            }
        }


        /// <inheritdoc/>
        public virtual void Delete(T entity)
        {
            Delete(Map.KeyValue<TKey>(entity));
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(T entity)
        {
            await DeleteAsync(Map.KeyValue<TKey>(entity));
        }

        /// <inheritdoc/>
        public virtual void Delete(TKey key)
        {
            using (var conn = Provider.Connection())
            {
                using (var command = DeleteCommand(conn))
                {
                    command.AddParameter(Map.KeyName, TypeMapping.GetDbType(typeof(TKey)), key);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(TKey key)
        {
            using (var conn = await Provider.ConnectionAsync())
            {
                using (var command = DeleteCommand(conn))
                {
                    command.AddParameter(Map.KeyName, TypeMapping.GetDbType(typeof(TKey)), key);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Creates a command that selects a single entity from the repository
        /// </summary>
        /// <param name="connection">The connection the command should be on</param>
        protected abstract IDbCommand SelectCommand(IDbConnection connection);

        /// <summary>
        /// Creates a command that counts all the entities in the repository
        /// </summary>
        /// <param name="connection">The connection the command should be on</param>
        protected abstract IDbCommand CountCommand(IDbConnection connection);

        /// <summary>
        /// Creates a command that returns all the entities in the repository
        /// </summary>
        /// <param name="connection"></param>
        protected abstract IDbCommand AllCommand(IDbConnection connection);

        /// <summary>
        /// Creates a command that inserts an entity into the repository
        /// </summary>
        /// <param name="connection">The connection the command should be on</param>
        protected abstract IDbCommand InsertCommand(IDbConnection connection);

        /// <summary>
        /// Creates a command for updating the entities in the repository
        /// </summary>
        /// <param name="connection">The connection the command should be on</param>
        protected abstract IDbCommand UpdateCommand(IDbConnection connection);

        /// <summary>
        /// Creates a command for deleting an entity from the repository
        /// </summary>
        /// <param name="connection">The connection the command should be on</param>
        protected abstract IDbCommand DeleteCommand(IDbConnection connection);
    }
}

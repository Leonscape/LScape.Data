using System.Data;

namespace LScape.Data.Repositories
{
    /// <summary>
    /// Basic Sql string repository
    /// </summary>
    /// <typeparam name="T">The type of object this repository is for</typeparam>
    /// <typeparam name="TKey">The type of the key for the object</typeparam>
    public class SqlRepository<T, TKey> : RepositoryBase<T, TKey> where T : class, new() where TKey : struct
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The connection provider to use for this repository</param>
        protected SqlRepository(IConnectionProvider provider) : base(provider) { }

        /// <inheritdoc/>
        protected override IDbCommand SelectCommand(IDbConnection connection)
        {
            return Command(connection, $"SELECT {Map.SelectColumnList} FROM [{Map.TableName}] WHERE [{Map.KeyName}] = @{Map.KeyName}");
        }

        /// <inheritdoc/>
        protected override IDbCommand CountCommand(IDbConnection connection)
        {
            return Command(connection, $"SELECT COUNT ([{Map.KeyName}]) FROM [{Map.TableName}]");
        }

        /// <inheritdoc/>
        protected override IDbCommand AllCommand(IDbConnection connection)
        {
            return Command(connection, $"SELECT {Map.SelectColumnList} FROM [{Map.TableName}]");
        }

        /// <inheritdoc/>
        protected override IDbCommand InsertCommand(IDbConnection connection)
        {
            return Command(connection, $"INSERT INTO [{Map.TableName}] ({Map.InsertColumnList}) OUTPUT INSERTED.* VALUES({Map.InsertParameterList})");
        }

        /// <inheritdoc/>
        protected override IDbCommand UpdateCommand(IDbConnection connection)
        {
            return Command(connection, $"UPDATE [{Map.TableName}] SET {Map.UpdateSetString} OUTPUT INSERTED.* WHERE [{Map.KeyName}] = @{Map.KeyName}");
        }

        /// <inheritdoc/>
        protected override IDbCommand DeleteCommand(IDbConnection connection)
        {
            return Command(connection, $"DELETE FROM [{Map.TableName}] WHERE [{Map.KeyName}] = @{Map.KeyName}");
        }

        private static IDbCommand Command(IDbConnection connection, string text)
        {
            var command = connection.CreateCommand();
            command.CommandText = text;
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }
    }
}

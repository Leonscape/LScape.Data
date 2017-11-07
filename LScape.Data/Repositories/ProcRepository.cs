using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LScape.Data.Repositories
{
    /// <summary>
    /// Repository for accessing a database through stored procedures
    /// </summary>
    /// <typeparam name="T">The type of object this repository is for</typeparam>
    /// <typeparam name="TKey">The type of the key for the object</typeparam>
    public class ProcRepository<T, TKey> : RepositoryBase<T, TKey> where T : class, new() where TKey : struct
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The connection provider to use for this repository</param>
        protected ProcRepository(IConnectionProvider provider) : base(provider) { }

        /// <summary>
        /// Calculates the stored procedure name from the action
        /// </summary>
        /// <param name="action">The action</param>
        protected virtual string ProcedureName(string action)
        {
            return $"sp_{Map.TableName}{action}";
        }

        /// <inheritdoc/>
        protected override IDbCommand AllCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("All"));
        }

        /// <inheritdoc/>
        protected override IDbCommand CountCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("Count"));
        }

        /// <inheritdoc/>
        protected override IDbCommand DeleteCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("Delete"));
        }

        /// <inheritdoc/>
        protected override IDbCommand InsertCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("Insert"));
        }

        /// <inheritdoc/>
        protected override IDbCommand SelectCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("Get"));
        }

        /// <inheritdoc/>
        protected override IDbCommand UpdateCommand(IDbConnection connection)
        {
            return Command(connection, ProcedureName("Update"));
        }

        /// <summary>
        /// Creates a sql stored procedure command
        /// </summary>
        /// <param name="connection">The connection to created the command on</param>
        /// <param name="storedProcedure">The stored procedure name</param>
        /// <returns></returns>
        protected static IDbCommand Command(IDbConnection connection, string storedProcedure)
        {
            var command = connection.CreateCommand();
            command.CommandText = storedProcedure;
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }
    }
}

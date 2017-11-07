using System.Collections.Generic;
using System.Data;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Basic IMap interface
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// The name of the table to use in the mapping
        /// </summary>
        string TableName { get; set; }

        #region Sql String Parts

        /// <summary>
        /// Select column list for use in a sql string
        /// </summary>
        string SelectColumnList { get; }

        /// <summary>
        /// Selects column list with an alias for use in a sql string
        /// </summary>
        /// <param name="alias">THe alias to use</param>
        string SelectColumnWithAlias(string alias);

        /// <summary>
        /// Insert column list for use in an insert sql string
        /// </summary>
        string InsertColumnList { get; }

        /// <summary>
        /// Insert parameter list for use in a sql string
        /// </summary>
        string InsertParameterList { get; }

        /// <summary>
        /// Update set string for use in a sql string
        /// </summary>
        string UpdateSetString { get; }

        /// <summary>
        /// Returns the key name for the map
        /// </summary>
        /// <remarks>If there are multiple keys only returns the first one</remarks>
        string KeyName { get; }

        #endregion

        #region object creation

        /// <summary>
        /// Create object from a data reader
        /// </summary>
        /// <param name="reader">The reader to get the object from</param>
        object Create(IDataReader reader);

        /// <summary>
        /// Creates an enumerable of objects for the data reader which are mapped
        /// </summary>
        /// <param name="reader">The reader to get the objects from</param>
        /// <returns></returns>
        IEnumerable<object> CreateEnumerable(IDataReader reader);

        #endregion

        #region Parameters

        /// <summary>
        /// Adds parameters from the object to the command which are mapped
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">Entity to get data from</param>
        /// <param name="includeKeys">Whether to include keys in the list</param>
        void AddParameters(IDbCommand command, object entity, bool includeKeys = false);

        /// <summary>
        /// Adds parameters from the object to the command which are specified
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the data from</param>
        /// <param name="properties">The names of the properties to add</param>
        void AddParameters(IDbCommand command, object entity, params string[] properties);

        /// <summary>
        /// Adds the key parameters to a command
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the values form</param>
        void AddKeyParameters(IDbCommand command, object entity);

        #endregion
    }
}

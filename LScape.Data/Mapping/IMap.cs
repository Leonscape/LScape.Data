using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

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

        /// <summary>
        /// Returns key parts for a where statement
        /// </summary>
        /// <remarks>Returns multi key parts for a where statement</remarks>
        string KeyWhere { get; }

        /// <summary>
        /// Returns a full select statement
        /// </summary>
        /// <remarks>
        /// No conditions are placed on the statement automatically, so use AddParametersWithWhere, or simply
        /// append your own where statement, or change it to your preferences
        /// </remarks>
        string SelectStatement { get; set; }

        /// <summary>
        /// Returns a count statement
        /// </summary>
        string CountStatement { get; set; }

        /// <summary>
        /// Returns a full insert statement
        /// </summary>
        string InsertStatement { get; set; }

        /// <summary>
        /// Returns a full update statement
        /// </summary>
        string UpdateStatement { get; set; }

        /// <summary>
        /// Returns a full delete statement
        /// </summary>
        string DeleteStatement { get; set; }

        #endregion

        #region Configuration

        /// <summary>
        /// The properties to be ignored
        /// </summary>
        /// <param name="properties">The name of the properties to ignore</param>
        /// <remarks>
        /// Can be called multiple times, and all will be ignored
        /// These properties are then completed ignored by the mapper
        /// </remarks>
        IMap Ignore(params string[] properties);

        /// <summary>
        /// The properties to set to calculated
        /// </summary>
        /// <param name="properties">The name of the properties to set as calculated</param>
        /// <remarks>
        /// Usually for when properties are calculated on the database,
        /// Date stamps, RowVersion, etc...
        /// </remarks>
        IMap Calculated(params string[] properties);

        /// <summary>
        /// The properties to set to Key
        /// </summary>
        /// <param name="properties">The name of the properties to set as Key</param>
        /// <remarks>
        /// For properties that are calculated keys of the object. Does not take part
        /// like calculated keys but are sent with parameters for updates.
        /// </remarks>
        IMap Key(params string[] properties);

        /// <summary>
        /// Tells the map fluent configuration is complete,
        /// and we can regenerate sql strings
        /// </summary>
        IMap CompleteConfiguration();

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

        /// <summary>
        /// Creates an enumerable of objects for the data reader which are mapped
        /// </summary>
        /// <param name="reader">The reader to get the objects from</param>
        /// <returns></returns>
        Task<IEnumerable<object>> CreateEnumerableAsync(IDataReader reader);

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

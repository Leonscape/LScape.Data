namespace LScape.Data.Mapping
{
    /// <summary>
    /// Interface for SqlMaps
    /// </summary>
    public interface ISqlMap
    {
        /// <summary>
        /// The name of the table to use in the mapping
        /// </summary>
        string TableName { get; set; }

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
    }
}

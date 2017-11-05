using System;
using System.Collections.Generic;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Basic IMap interface
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// The Clr type the map is for
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        /// The name of the table to use in the mapping
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// The mapping fields
        /// </summary>
        IEnumerable<Field> Fields { get; }
    }
}

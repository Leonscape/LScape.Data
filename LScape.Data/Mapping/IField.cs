using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Interface for fields
    /// </summary>
    public interface IField
    {
        /// <summary>
        /// The type of mapping for the field
        /// </summary>
        FieldType FieldType { get; set; }

        /// <summary>
        /// The name of the property
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// The name of the column its mapped to
        /// </summary>
        string ColumnName { get; set; }

        /// <summary>
        /// The value of the property from an entity
        /// </summary>
        object GetPropertyValue(object entity);

        /// <summary>
        /// Sets the properties value from the datareader
        /// </summary>
        /// <param name="entity">The entity to set the value for</param>
        /// <param name="reader">The reader to get the value from</param>
        void SetPropertyValue(object entity, IDataReader reader);

        /// <summary>
        /// Adds this field as a parameter
        /// </summary>
        /// <param name="command">The command to add the parameter to</param>
        /// <param name="entity">The entity to get the value from</param>
        void AddParameter(IDbCommand command, object entity);
    }
}

using System.Data;
using System.Reflection;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Holds the mapping details of a field
    /// </summary>
    public class Field
    {
        /// <summary>
        /// The type of mapping for the field
        /// </summary>
        public FieldType FieldType { get; set; }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The name of the column its mapped to
        /// </summary>
        public string ColumnName { get; set; }


        internal PropertyInfo PropertyInfo { get; set; }

        internal DbType DbType { get; set; }
    }
}

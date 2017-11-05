using System;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Sets the column name for mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Sets a column name for mapping
        /// </summary>
        /// <param name="column">The column name</param>
        public ColumnAttribute(string column)
        {
            ColumnName = column;
        }

        /// <summary>
        /// The column name for this property
        /// </summary>
        public string ColumnName { get; }
    }
}

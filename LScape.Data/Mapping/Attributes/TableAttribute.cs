using System;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Sets the table name for the mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Sets the table name for the mapping
        /// </summary>
        /// <param name="name">The table name</param>
        public TableAttribute(string name)
        {
            TableName = name;
        }

        /// <summary>
        /// The tables name
        /// </summary>
        public string TableName { get; }
    }
}

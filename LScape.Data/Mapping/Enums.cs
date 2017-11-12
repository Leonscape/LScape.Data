namespace LScape.Data.Mapping
{
    /// <summary>
    /// The different types of mappings for a field
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        /// Field is mapped for reading and writing
        /// </summary>
        Map,
        /// <summary>
        /// Property is mapped only for reading
        /// </summary>
        Calculated,
        /// <summary>
        /// Property is mapped only for reading and updating for key field
        /// </summary>
        Key,
        /// <summary>
        /// Property is ignored by mapping
        /// </summary>
        Ignore
    }

    /// <summary>
    /// Basic naming conversions
    /// </summary>
    public enum NameConvention
    {
        /// <summary>
        /// Column names are the same as property names
        /// </summary>
        Exact,
        /// <summary>
        /// Column names are lowercase
        /// </summary>
        Lowercase,
        /// <summary>
        /// Column names are uppercase
        /// </summary>
        Uppercase,
        /// <summary>
        /// Column names are split with under scores
        /// </summary>
        SplitCase,
        /// <summary>
        /// Column names are split and lowercase
        /// </summary>
        SplitCaseLower,
        /// <summary>
        /// Column names are split and uppercase
        /// </summary>
        SplitCaseUpper
    }
}

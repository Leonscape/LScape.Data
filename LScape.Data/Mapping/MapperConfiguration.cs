using System;
using System.Text.RegularExpressions;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// The configuration to use when mapping
    /// </summary>
    public class MapperConfiguration
    {
        /// <summary>
        /// Standard constructor set conventions to exact
        /// </summary>
        public MapperConfiguration()
        {
            TableNameConvention = NameConvention.Exact;
            ColumnNameConvention = NameConvention.Exact;
        }

        /// <summary>
        /// Naming convention to use for the table names
        /// </summary>
        /// <remarks>Ignored if <see cref="TableNameConvert"/> is set</remarks>
        public NameConvention TableNameConvention { get; set; }

        /// <summary>
        /// Function to use to convert class names to table names
        /// </summary>
        public Func<string, string> TableNameConvert { get; set; }

        /// <summary>
        /// The name convention to use when mapping between properties and columns
        /// </summary>
        /// <remarks>Ignored if <see cref="ColumnNameConvert"/> is set</remarks>
        public NameConvention ColumnNameConvention { get; set; }

        /// <summary>
        /// Function to use to convert property names to column names
        /// </summary>
        public Func<string, string> ColumnNameConvert { get; set; }

        /// <summary>
        /// Use this to match property names that you wish to ignore
        /// </summary>
        public Func<string, Type, bool> IgnoreMatch { get; set; }

        /// <summary>
        /// Use this to match property names that are calculated
        /// </summary>
        public Func<string, Type, bool> CalculatedMatch { get; set; }

        /// <summary>
        /// Use this to match properties that are key fields, won't
        /// be used in inserts, put will be in parameters for updates
        /// </summary>
        public Func<string, Type, bool> KeyMatch { get; set; }

        internal string TableName(string originalName)
        {
            return TableNameConvert == null ? ConvertName(originalName, TableNameConvention) : TableNameConvert(originalName);
        }

        internal string ColumnName(string originalName)
        {
            return ColumnNameConvert == null ? ConvertName(originalName, ColumnNameConvention) : ColumnNameConvert(originalName);
        }

        private static string ConvertName(string originalName, NameConvention convention)
        {
            var name = originalName;
            switch (convention)
            {
                case NameConvention.Lowercase:
                    name = originalName.ToLowerInvariant();
                    break;
                case NameConvention.Uppercase:
                    name = originalName.ToUpperInvariant();
                    break;
                case NameConvention.SplitCase:
                    name = Regex.Replace(originalName, "(?<=[a-z])([A-Z])", "_$1", RegexOptions.Compiled);
                    break;
                case NameConvention.SplitCaseLower:
                    name = Regex.Replace(originalName, "(?<=[a-z])([A-Z])", "_$1", RegexOptions.Compiled).ToLowerInvariant();
                    break;
                case NameConvention.SplitCaseUpper:
                    name = Regex.Replace(originalName, "(?<=[a-z])([A-Z])", "_$1", RegexOptions.Compiled).ToUpperInvariant();
                    break;
            }

            return name;
        }
    }
}

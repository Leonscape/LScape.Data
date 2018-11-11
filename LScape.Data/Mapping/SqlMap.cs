using System.Linq;
using System.Reflection;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Sql Map Class, provides extra functionality for Sql
    /// </summary>
    /// <typeparam name="T">The type the map is for</typeparam>
    /// <remarks>This class adds extra functionality for sql statements</remarks>
    public class SqlMap<T> : Map<T>, ISqlMap where T : class, new()
    {
        private string _selectColumnList;
        private string _insertColumnList;
        private string _insertParameterList;
        private string _updateSetString;
        private string _keyName;
        private string _keyWhere;

        private string _selectStatement;
        private string _countStatement;
        private string _insertStatement;
        private string _updateStatement;
        private string _deleteStatement;

        /// <summary>
        /// Constructs a new Map for a type
        /// Using the configuration in the mapper
        /// </summary>
        public SqlMap() : this(Mapper.Configuration)
        {}

        /// <summary>
        /// Creates a SqlMap from a simple Map
        /// </summary>
        /// <param name="map">The simple map to base this on</param>
        public SqlMap(Map<T> map)
        {
            CalculateTableName(Mapper.Configuration);
            _fields.AddRange(map.Fields);
        }

        /// <summary>
        /// Constructs a map for a type based on the configuration
        /// provided
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        public SqlMap(MapperConfiguration configuration) : base(configuration)
        {
            CalculateTableName(configuration);
        }

        /// <inheritdoc/>
        public override Map<T> CompleteConfiguration()
        {
            ClearStrings();
            return base.CompleteConfiguration();
        }

        /// <inheritdoc/>
        public override Map<T> Mapping(params IField[] fields)
        {
            ClearStrings();
            return base.Mapping(fields);
        }

        /// <inheritdoc/>
        public string TableName { get; set; }

        /// <inheritdoc/>
        public string SelectColumnList => _selectColumnList ?? (_selectColumnList = string.Join(", ", _fields.Where(f => f.FieldType != FieldType.Ignore).Select(f => $"[{f.ColumnName}]")));

        /// <inheritdoc/>
        public string SelectColumnWithAlias(string alias)
        {
            return string.Join(", ", _fields.Where(f => f.FieldType != FieldType.Ignore).Select(f => $"{alias}.[{f.ColumnName}]"));
        }

        /// <inheritdoc/>
        public string InsertColumnList => _insertColumnList ?? (_insertColumnList = string.Join(", ", _fields.Where(p => p.FieldType == FieldType.Map).Select(p => $"[{p.ColumnName}]")));

        /// <inheritdoc/>
        public string InsertParameterList => _insertParameterList ?? (_insertParameterList = string.Join(", ", _fields.Where(f => f.FieldType == FieldType.Map).Select(f => $"@{f.ColumnName}")));

        /// <inheritdoc/>
        public string UpdateSetString => _updateSetString ?? (_updateSetString = string.Join(", ", _fields.Where(f => f.FieldType == FieldType.Map).Select(f => $"[{f.ColumnName}] = @{f.ColumnName}")));

        /// <inheritdoc/>
        public string KeyName => _keyName ?? (_keyName = _fields.First(f => f.FieldType == FieldType.Key).ColumnName);

        /// <inheritdoc/>
        public string KeyWhere => _keyWhere ?? (_keyWhere = string.Join(" AND ", _fields.Where(f => f.FieldType == FieldType.Key).Select(f => $"[{f.ColumnName}] = @{f.ColumnName}")));

        /// <inheritdoc/>
        public string SelectStatement
        {
            get => _selectStatement ?? (_selectStatement = $"SELECT {SelectColumnList} FROM [{TableName}]");
            set => _selectStatement = value;
        }

        /// <inheritdoc/>
        public string CountStatement
        {
            get => _countStatement ?? (_countStatement = $"SELECT COUNT(*) FROM [{TableName}]");
            set => _countStatement = value;
        }

        /// <inheritdoc/>
        public string InsertStatement
        {
            get => _insertStatement ?? (_insertStatement = $"INSERT INTO [{TableName}] ({InsertColumnList}) OUTPUT INSERTED.* VALUES ({InsertParameterList})");
            set => _insertStatement = value;
        }

        /// <inheritdoc/>
        public string UpdateStatement
        {
            get => _updateStatement ?? (_updateStatement = $"UPDATE [{TableName}] SET {UpdateSetString} OUTPUT INSERTED.* WHERE {KeyWhere}");
            set => _updateStatement = value;
        }

        /// <inheritdoc/>
        public string DeleteStatement
        {
            get => _deleteStatement ?? (_deleteStatement = $"DELETE FROM [{TableName}] WHERE {KeyWhere}");
            set => _deleteStatement = value;
        }

        private void ClearStrings()
        {
            _selectColumnList = null;
            _insertColumnList = null;
            _insertParameterList = null;
            _updateSetString = null;
            _keyName = null;
            _keyWhere = null;

            _selectStatement = null;
            _countStatement = null;
            _insertStatement = null;
            _updateStatement = null;
            _deleteStatement = null;
        }

        private void CalculateTableName(MapperConfiguration configuration)
        {
            var type = typeof(T);
            TableName = configuration.TableName(type.Name);
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                TableName = tableAttribute.TableName;
        }
    }
}

using LScape.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Gemeric map class automatically 
    /// </summary>
    public class Map<T> : IMap where T : class, new()
    {
        private readonly List<IField> _fields = new List<IField>();
        private string _selectColumnList;
        private string _insertColumnList;
        private string _insertParameterList;
        private string _updateSetString;
        private string _keyName;
        private string _keyWhere;

        /// <summary>
        /// Constructs a new Map for a type
        /// Using the configuration in the mapper
        /// </summary>
        public Map() : this(Mapper.Configuration)
        {}

        /// <summary>
        /// Constructs a map for a type based on the configuration
        /// provided
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        public Map(MapperConfiguration configuration)
        {
            var type = typeof(T);
            TableName = configuration.TableName(type.Name);
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                TableName = tableAttribute.TableName;

            MapProperties(type, configuration);
        }

        /// <inheritdoc/>
        public string TableName { get; set; }

        /// <summary>
        /// The fields of the map
        /// </summary>
        public IEnumerable<IField> Fields => _fields;

        #region Sql String Parts

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
        public string KeyWhere => _keyWhere ?? (_keyWhere = string.Join(", ", _fields.Where(f => f.FieldType == FieldType.Key).Select(f => $"[{f.ColumnName}] = @{f.ColumnName}")));

        #endregion

        #region Set Field Types

        /// <summary>
        /// The properties to be ignored
        /// </summary>
        /// <param name="properties">The properties to ignore</param>
        /// <remarks>
        /// Can be called multiple times, and all will be ignored
        /// These properties are then completed ignored by the mapper
        /// </remarks>
        public Map<T> Ignore(params Expression<Func<T, object>>[] properties)
        {
            return Ignore(properties.Select(PropertyName).ToArray());
        }

        /// <summary>
        /// The properties to be ignored
        /// </summary>
        /// <param name="properties">The name of the properties to ignore</param>
        /// <remarks>
        /// Can be called multiple times, and all will be ignored
        /// These properties are then completed ignored by the mapper
        /// </remarks>
        public Map<T> Ignore(params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
                field.FieldType = FieldType.Ignore;

            return this;
        }

        /// <summary>
        /// The properties to set to calculated
        /// </summary>
        /// <param name="properties">The properties to set as calculated</param>
        /// <remarks>
        /// Usually for when properties are calculated on the database,
        /// Identity Keys, Datestamps, etc...
        /// </remarks>
        public Map<T> Calculated(params Expression<Func<T, object>>[] properties)
        {
            return Calculated(properties.Select(PropertyName).ToArray());
        }

        /// <summary>
        /// The properties to set to calculated
        /// </summary>
        /// <param name="properties">The name of the properties to set as calculated</param>
        /// <remarks>
        /// Usually for when properties are calculated on the database,
        /// Datestamps, RowVersion, etc...
        /// </remarks>
        public Map<T> Calculated(params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
                field.FieldType = FieldType.Calculated;

            return this;
        }

        /// <summary>
        /// The properties to set to Key
        /// </summary>
        /// <param name="properties">The properties to set as Key</param>
        /// <remarks>
        /// For properties that are calculated keys of the object. Does not take part
        /// like calculated keys but are sent with parameters for updates.
        /// </remarks>
        public Map<T> Key(params Expression<Func<T, object>>[] properties)
        {
            return Key(properties.Select(PropertyName).ToArray());
        }

        /// <summary>
        /// The properties to set to Key
        /// </summary>
        /// <param name="properties">The name of the properties to set as Key</param>
        /// <remarks>
        /// For properties that are calculated keys of the object. Does not take part
        /// like calculated keys but are sent with parameters for updates.
        /// </remarks>
        public Map<T> Key(params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
                field.FieldType = FieldType.Key;

            return this;
        }

        #endregion

        /// <summary>
        /// Manually set a mapping for a property
        /// </summary>
        /// <param name="fields">The field to use</param>
        /// <remarks>
        /// Can be called multiple times, any properties not given
        /// a manual mapping will be automatically mapped
        /// </remarks>
        public Map<T> Mapping(params IField[] fields)
        {
            foreach (var newField in fields)
            {
                var field = _fields.FirstOrDefault(f => f.PropertyName == newField.PropertyName);
                if (field != null)
                {
                    field.ColumnName = newField.ColumnName;
                    field.FieldType = newField.FieldType;
                }
            }

            ClearStrings();
            return this;
        }

        #region Create Objects

        /// <summary>
        /// Create object from a data reader
        /// </summary>
        /// <param name="reader">The reader to get the object from</param>
        public T Create(IDataReader reader)
        {
            var result = new T();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var field = _fields.FirstOrDefault(f => f.ColumnName == name);
                field?.SetPropertyValue(result, reader);
            }

            return result;
        }

        /// <summary>
        /// Creates an enumerable of objects for the data reader which are mapped
        /// </summary>
        /// <param name="reader">The reader to get the objects from</param>
        /// <returns></returns>
        public IEnumerable<T> CreateEnumerable(IDataReader reader)
        {
            var readProps = ReadProperties(reader);
            var items = new List<T>();
            if (readProps != null)
            {
                while (reader.Read())
                {
                    var entity = new T();
                    readProps(entity, reader);
                    items.Add(entity);
                }
            }

            return items;
        }

        /// <summary>
        /// Creates an enumerable of objects for the data reader which are mapped
        /// </summary>
        /// <param name="reader">The reader to get the objects from</param>
        public async Task<IEnumerable<T>> CreateEnumerableAsync(IDataReader reader)
        {
            var readProps = ReadProperties(reader);
            var items = new List<T>();
            if (readProps != null)
            {
                while (await reader.ReadAsync())
                {
                    var entity = new T();
                    readProps(entity, reader);
                    items.Add(entity);
                }
            }

            return items;
        }

        object IMap.Create(IDataReader reader)
        {
            return Create(reader);
        }

        IEnumerable<object> IMap.CreateEnumerable(IDataReader reader)
        {
            return CreateEnumerable(reader);
        }

        async Task<IEnumerable<object>> IMap.CreateEnumerableAsync(IDataReader reader)
        {
            return await CreateEnumerableAsync(reader);
        }

        #endregion

        #region Parameters

        /// <summary>
        /// Adds parameters from the object to the command which are mapped
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">Entity to get data from</param>
        /// <param name="includeKeys">Whether to include keys in the list</param>
        public void AddParameters(IDbCommand command, T entity, bool includeKeys = false)
        {
            foreach (var field in _fields.Where(f => f.FieldType == FieldType.Map || includeKeys && f.FieldType == FieldType.Key))
                field.AddParameter(command, entity);
        }

        /// <summary>
        /// Adds parameters from the object to the command which are specified
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the data from</param>
        /// <param name="properties">The properties to add</param>
        public void AddParameters(IDbCommand command, T entity, params Expression<Func<T, object>>[] properties)
        {
            AddParameters(command, entity, properties.Select(PropertyName).ToArray());
        }

        /// <summary>
        /// Adds parameters from the object to the command which are specified
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the data from</param>
        /// <param name="properties">The names of the properties to add</param>
        public void AddParameters(IDbCommand command, T entity, params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
                field.AddParameter(command, entity);
        }

        /// <summary>
        /// Adds the key parameters to a command
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the values form</param>
        public void AddKeyParameters(IDbCommand command, T entity)
        {
            foreach (var field in _fields.Where(f => f.FieldType == FieldType.Key))
                field.AddParameter(command, entity);
        }

        /// <inheritdoc />
        public void AddParameters(IDbCommand command, object entity, bool includeKeys = false)
        {
            if(entity is T cast)
                AddParameters(command, cast, includeKeys);
            else
                throw new ArgumentException("entity is of the incorrect type for this map", nameof(entity));
        }

        /// <inheritdoc />
        public void AddParameters(IDbCommand command, object entity, params string[] properties)
        {
            if (entity is T cast)
                AddParameters(command, cast, properties);
            else
                throw new ArgumentException("entity is of the incorrect type for this map", nameof(entity));
        }

        /// <inheritdoc />
        public void AddKeyParameters(IDbCommand command, object entity)
        {
            if (entity is T cast)
                AddKeyParameters(command, cast);
            else
                throw new ArgumentException("entity is of the incorrect type for this map", nameof(entity));
        }

        #endregion

        /// <summary>
        /// Gets the key value for an entity
        /// </summary>
        /// <param name="entity">The entity to get the key value for</param>
        public object KeyValue(T entity)
        {
            var field = _fields.FirstOrDefault(f => f.FieldType == FieldType.Key);
            return field?.GetPropertyValue(entity);
        }

        /// <summary>
        /// Gets the key value for an entity
        /// </summary>
        /// <typeparam name="TKey">The type the key value should be returned in</typeparam>
        /// <param name="entity">The entity the key value is for</param>
        public TKey KeyValue<TKey>(T entity)
        {
            var field = _fields.FirstOrDefault(f => f.FieldType == FieldType.Key);
            if (field?.GetPropertyValue(entity) is TKey key)
                return key;

            return default(TKey);
        }

        private void MapProperties(Type type, MapperConfiguration configuration)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var fieldType = typeof(Field<>);
            foreach (var property in properties)
            {
                var colAttrib = property.GetCustomAttribute<ColumnAttribute>();
                var field = (IField)Activator.CreateInstance(fieldType.MakeGenericType(property.PropertyType), property);
                field.ColumnName = colAttrib != null ? colAttrib.ColumnName : configuration.ColumnName(property.Name);

                if (property.GetCustomAttribute<KeyAttribute>() != null || configuration.KeyMatch != null && configuration.KeyMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Key;
                else if (property.GetCustomAttribute<CalculatedAttribute>() != null || configuration.CalculatedMatch != null && configuration.CalculatedMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Calculated;
                else if (property.GetCustomAttribute<IgnoredAttribute>() != null || configuration.IgnoreMatch != null && configuration.IgnoreMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Ignore;

                _fields.Add(field);
            }
        }

        private static string PropertyName(Expression<Func<T, object>> property)
        {
            string name = null;
            if (property.Body is MemberExpression memberBody)
                name = memberBody.Member.Name;
            if (property.Body is UnaryExpression unaryBody)
                name = ((MemberExpression)unaryBody.Operand).Member.Name;

            return name;
        }

        private Action<T, IDataReader> ReadProperties(IDataReader reader)
        {
            Action<T, IDataReader> readProps = null;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var field = _fields.FirstOrDefault(f => f.ColumnName == name);
                if (field != null)
                    readProps += field.SetPropertyValue;
            }

            return readProps;
        }

        private void ClearStrings()
        {
            _selectColumnList = null;
            _insertColumnList = null;
            _insertParameterList = null;
            _updateSetString = null;
        }
    }
}

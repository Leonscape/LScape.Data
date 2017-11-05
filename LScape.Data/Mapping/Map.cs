﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Gemeric map class automatically 
    /// </summary>
    public class Map<T> : IMap where T : class, new()
    {
        private readonly List<Field> _fields = new List<Field>();
        private string _selectColumnList;
        private string _insertColumnList;
        private string _insertParameterList;
        private string _updateSetString;

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

        /// <inheritdoc />
        public Type ClrType => typeof(T);

        /// <inheritdoc/>
        public string TableName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Field> Fields => _fields;

        /// <summary>
        /// Select column list for use in a sql string
        /// </summary>
        public string SelectColumnList => _selectColumnList ?? (_selectColumnList = string.Join(", ", _fields.Where(f => f.FieldType != FieldType.Ignore).Select(f => $"[{f.ColumnName}]")));

        /// <summary>
        /// Selects column list with an alias for use in a sql string
        /// </summary>
        /// <param name="alias">THe alias to use</param>
        public string SelectColumnWithAlias(string alias)
        {
            return string.Join(", ", _fields.Where(f => f.FieldType != FieldType.Ignore).Select(f => $"{alias}.[{f.ColumnName}]"));
        }

        /// <summary>
        /// Insert column list for use in an insert sql string
        /// </summary>
        public string InsertColumnList => _insertColumnList ?? (_insertColumnList = string.Join(", ", _fields.Where(p => p.FieldType == FieldType.Map).Select(p => $"[{p.ColumnName}]")));

        /// <summary>
        /// Insert parameter list for use in a sql string
        /// </summary>
        public string InsertParameterList => _insertParameterList ?? (_insertParameterList = string.Join(", ", _fields.Where(f => f.FieldType == FieldType.Map).Select(f => $"@{f.ColumnName}")));

        /// <summary>
        /// Update set string for use in a sql string
        /// </summary>
        public string UpdateSetString => _updateSetString ?? (_updateSetString = string.Join(", ", _fields.Where(f => f.FieldType == FieldType.Map).Select(f => $"[{f.ColumnName}] = @{f.ColumnName}")));

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
        /// Identity Keys, Datestamps, etc...
        /// </remarks>
        public Map<T> Calculated(params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
                field.FieldType = FieldType.Calculated;

            return this;
        }

        /// <summary>
        /// Manually set a mapping for a property
        /// </summary>
        /// <param name="fields">The field to use</param>
        /// <remarks>
        /// Can be called multiple times, any properties not given
        /// a manual mapping will be automatically mapped
        /// </remarks>
        public Map<T> Mapping(params Field[] fields)
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
                var field = _fields.FirstOrDefault(f => f.ColumnName == name)?.PropertyInfo;
                field?.SetValue(result, reader.GetValue(i, field.PropertyType));
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
            var props = new List<Action<T, IDataReader>>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var field = _fields.FirstOrDefault(f => f.ColumnName == name);
                if (field != null)
                    props.Add((e, r) => { field.PropertyInfo.SetValue(e, r.GetValue(field.ColumnName, field.PropertyInfo.PropertyType)); });
            }

            var items = new List<T>();
            while (reader.Read())
            {
                var entity = new T();
                props.ForEach(a => a(entity, reader));
                items.Add(entity);
            }

            return items;
        }

        /// <summary>
        /// Adds parameters from the object to the command which are mapped
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">Entity to get data from</param>
        public void Parameters(IDbCommand command, T entity)
        {
            foreach (var field in _fields.Where(f => f.FieldType == FieldType.Map || f.FieldType == FieldType.Key))
            {
                command.AddParameter(field.ColumnName, field.DbType, field.PropertyInfo.GetValue(entity));
            }
        }

        /// <summary>
        /// Adds parameters from the object to the command which are specified
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the data from</param>
        /// <param name="properties">The properties to add</param>
        public void Parameters(IDbCommand command, T entity, params Expression<Func<T, object>>[] properties)
        {
            Parameters(command, entity, properties.Select(PropertyName).ToArray());
        }

        /// <summary>
        /// Adds parameters from the object to the command which are specified
        /// </summary>
        /// <param name="command">The command to add parameters to</param>
        /// <param name="entity">The entity to get the data from</param>
        /// <param name="properties">The names of the properties to add</param>
        public void Parameters(IDbCommand command, T entity, params string[] properties)
        {
            foreach (var field in _fields.Where(f => properties.Contains(f.PropertyName)))
            {
                command.AddParameter(field.ColumnName, field.DbType, field.PropertyInfo.GetValue(entity));
            }
        }


        /// <summary>
        /// Checks to ses if a type is mappable
        /// </summary>
        /// <param name="type">The type to check</param>
        protected virtual bool IsMappableType(Type type)
        {
            if (type.GetTypeInfo().IsValueType || type == typeof(string))
                return true;

            if (type.IsArray && type.GetElementType().GetTypeInfo().IsValueType)
                return true;

            return false;
        }

        private void MapProperties(Type type, MapperConfiguration configuration)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var colAttrib = property.GetCustomAttribute<ColumnAttribute>();
                var field = new Field
                {
                    PropertyInfo = property,
                    DbType = TypeMapping.GetDbType(property.PropertyType),
                    PropertyName = property.Name,
                    ColumnName = colAttrib != null ? colAttrib.ColumnName : configuration.ColumnName(property.Name),
                    FieldType = IsMappableType(property.PropertyType) ? FieldType.Map : FieldType.Ignore
                };

                if (property.GetCustomAttribute<KeyAttribute>() != null || configuration.KeyMatch != null && configuration.KeyMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Key;
                else if (property.GetCustomAttribute<CalculatedAttribute>() != null || configuration.CalculatedMatch != null && configuration.CalculatedMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Calculated;
                else if (property.GetCustomAttribute<IgnoredAttribute>() != null || configuration.IgnoreMatch != null && configuration.IgnoreMatch(property.Name, property.PropertyType))
                    field.FieldType = FieldType.Ignore;

                _fields.Add(field);
            }
        }

        private string PropertyName(Expression<Func<T, object>> property)
        {
            string name = null;
            if (property.Body is MemberExpression memberBody)
                name = memberBody.Member.Name;
            if (property.Body is UnaryExpression unaryBody)
                name = ((MemberExpression)unaryBody.Operand).Member.Name;

            return name;
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

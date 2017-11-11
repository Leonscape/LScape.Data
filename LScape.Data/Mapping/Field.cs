using LScape.Data.Extensions;
using System;
using System.Data;
using System.Reflection;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Holds the mapping details of a field
    /// </summary>
    public class Field<T> : IField
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly DbType _dbType;

        /// <summary>
        /// Creates a new field
        /// </summary>
        /// <param name="propertyInfo">THe property info</param>
        public Field(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            _dbType = TypeMapping.GetDbType(_propertyInfo.PropertyType);
            FieldType = TypeMapping.IsMappableType(_propertyInfo.PropertyType) ? FieldType.Map : FieldType.Ignore;
        }

        /// <inheritdoc />
        public FieldType FieldType { get; set; }

        /// <inheritdoc />
        public string PropertyName => _propertyInfo.Name;

        /// <inheritdoc />
        public string ColumnName { get; set; }

        /// <summary>
        /// The value of the property from an entity
        /// </summary>
        public T GetPropertyValue(object entity)
        {
            return (T)_propertyInfo.GetValue(entity);
        }

        /// <inheritdoc />
        object IField.GetPropertyValue(object entity)
        {
            return GetPropertyValue(entity);
        }

        /// <inheritdoc />
        public void SetPropertyValue(object entity, IDataReader reader)
        {
           _propertyInfo.SetValue(entity, reader.GetValue(ColumnName, _propertyInfo.PropertyType));
        }

        /// <inheritdoc />
        public void AddParameter(IDbCommand command, object entity)
        {
            command.AddParameter(ColumnName, _dbType, GetPropertyValue(entity));
        }
    }
}

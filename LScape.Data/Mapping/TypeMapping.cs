using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace LScape.Data.Mapping
{
    internal static class TypeMapping
    {
        public static DbType GetDbType(Type type)
        {
            if (IsEnum(ref type))
            {
                type = Enum.GetUnderlyingType(type);
            }

            return _typeMappings.TryGetValue(type, out var result) ? result : DbType.Object;
        }

        public static bool IsEnum(ref Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GenericTypeArguments[0].GetTypeInfo().IsEnum)
                type = type.GenericTypeArguments[0];

            return type.GetTypeInfo().IsEnum;
        }

        private static readonly Dictionary<Type, DbType> _typeMappings = new Dictionary<Type, DbType> {
            {typeof(bool), DbType.Boolean},
            {typeof(bool?), DbType.Boolean},
            {typeof(byte), DbType.Byte},
            {typeof(byte?), DbType.Byte},
            {typeof(string), DbType.String},
            {typeof(DateTime), DbType.DateTime2},
            {typeof(DateTime?), DbType.DateTime2},
            {typeof(short), DbType.Int16},
            {typeof(short?), DbType.Int16},
            {typeof(int), DbType.Int32},
            {typeof(int?), DbType.Int32},
            {typeof(long), DbType.Int64},
            {typeof(long?), DbType.Int64},
            {typeof(decimal), DbType.Decimal},
            {typeof(decimal?), DbType.Decimal},
            {typeof(double), DbType.Double},
            {typeof(double?), DbType.Double},
            {typeof(float), DbType.Single},
            {typeof(float?), DbType.Single},
            {typeof(TimeSpan), DbType.Time},
            {typeof(TimeSpan?), DbType.Time},
            {typeof(Guid), DbType.Guid},
            {typeof(Guid?), DbType.Guid},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?[]), DbType.Binary},
            {typeof(char[]), DbType.String},
            {typeof(char?[]), DbType.String}
        };
    }
}

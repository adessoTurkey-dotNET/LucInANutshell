using System.Collections;
using System.Reflection;

namespace Ads.LuceneIndexer.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsPrimitiveType(this PropertyInfo property)
        {
            var type = property.PropertyType;
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        public static bool IsPrimitiveType(this Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        public static Type? GetPropertyType(this PropertyInfo propertyInfo)
        {
            var nullable = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;
            var propertyType = nullable
                ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                : propertyInfo.PropertyType;

            return Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        }

        public static bool IsPropertyCollection(this PropertyInfo property)
        {
            return (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string));
        }

        public static bool IsTypeCollection(this Type typeInfo)
        {
            return (typeInfo.Name != nameof(String)
                && typeInfo.GetInterface(nameof(IEnumerable)) != null);
        }
    }
}

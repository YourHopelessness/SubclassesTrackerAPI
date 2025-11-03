using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace SubclassesTracker.Caching.Services.ObjectSerilization
{
    public interface IObjectFlattener
    {
        /// <summary>
        /// Flattens an object into a dictionary of key-value pairs, where keys represent the property paths.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        Dictionary<string, object?> Flatten(object? obj, string prefix = "");
    }

    /// <summary>
    /// Flattens complex objects into a dictionary of key-value pairs.
    /// </summary>
    public class ObjectFlattenerService : IObjectFlattener
    {
        public Dictionary<string, object?> Flatten(object? obj, string prefix = "")
        {
            var result = new Dictionary<string, object?>();

            if (obj == null) return result;

            var type = obj.GetType();

            if (IsSimpleType(type))
            {
                result[prefix] = obj;

                return result;
            }

            // --- Handle properties ---
            foreach (var prop in ReflectionCache.GetProperties(type))
            {
                if (!prop.CanRead || typeof(JToken).IsAssignableFrom(prop.PropertyType)) continue;

                var value = prop.GetValue(obj);
                var propName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";

                if (value == null)
                {
                    result[propName] = null; // Include null values
                }
                else if (IsSimpleType(prop.PropertyType))
                {
                    result[propName] = value; // Directly add simple types
                }
                else if (IsEnumerableButNotString(prop.PropertyType))
                {
                    // Handle collections
                    var list = ((IEnumerable)value).Cast<object?>().ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        var itemPrefix = $"{propName}[{i}]";
                        var nested = Flatten(item, itemPrefix);
                        foreach (var kvp in nested)
                            result[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Handle collections that does not implementing IEnumerable as Dictionary
                    var nested = Flatten(value, propName);
                    foreach (var kvp in nested)
                        result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a type is a simple type (primitive, string, enum, DateTime, etc.)
        /// </summary>
        private static bool IsSimpleType(Type type) =>
            type.IsPrimitive || type.IsEnum || type == typeof(string) ||
            type == typeof(decimal) || type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) || type == typeof(TimeSpan) ||
            type == typeof(Guid) || type == typeof(byte[]) ||
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                IsSimpleType(Nullable.GetUnderlyingType(type)!);

        /// <summary>
        /// Checks if a type is an enumerable type but not a string.
        /// </summary>
        private static bool IsEnumerableButNotString(Type type) =>
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }
}

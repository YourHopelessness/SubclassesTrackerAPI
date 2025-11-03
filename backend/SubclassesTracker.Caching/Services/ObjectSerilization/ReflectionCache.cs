using System.Collections.Concurrent;
using System.Collections;
using System.Reflection;

namespace SubclassesTracker.Caching.Services.ObjectSerilization
{
    /// <summary>
    /// Provides utility methods for caching and retrieving reflection-based information about types.
    /// </summary>
    /// <remarks>This static class offers methods to efficiently access property information and determine
    /// element types of collections using reflection. It caches property information to improve performance on repeated
    /// access.</remarks>
    public static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _properties = new();

        public static PropertyInfo[] GetProperties(Type type) =>
            _properties.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        public static PropertyInfo? FindProperty(Type type, string name) =>
            GetProperties(type).FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static Type? GetListElementType(Type listType) =>
            listType.IsArray
            ? listType.GetElementType()
            : listType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(listType)
            ? listType.GetGenericArguments()[0]
            : null;
    }
}

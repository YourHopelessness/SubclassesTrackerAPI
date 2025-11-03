using Mapster;
using Microsoft.Extensions.Logging;
using Parquet.Schema;
using System.Collections;
using System.Reflection;

namespace SubclassesTracker.Caching.Services.ObjectSerilization
{
    public interface IObjectUnflattener
    {
        /// <summary>
        /// Deserializes a flattened parquet data into a nested object of type T.
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="columnData">flatten dict</param>
        /// <returns>Object of T type</returns>
        T DeserializeFromColumns<T>(
            ParquetSchema schema,
            Dictionary<string, Array> columnData,
            int rowIndex)
            where T : class, new();
    }

    public class ObjectUnflattenerService(ILogger<ObjectUnflattenerService> logger) : IObjectUnflattener
    {
        public T DeserializeFromColumns<T>(
            ParquetSchema schema,
            Dictionary<string, Array> columnData,
            int rowIndex) where T : class, new()
        {
            var root = new T();

            foreach (var field in schema.GetDataFields())
            {
                try
                {
                    var path = ParsePath(field.Name);
                    var value = columnData[field.Name].GetValue(rowIndex);
                    SetNestedValue(root, path, value);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to map field {Field}", field.Name);
                }
            }

            return root;
        }

        private static List<PathSegment> ParsePath(string path)
        {
            // parse Fights[0]_Healers[1]_Name → ["Fights", 0, "Healers", 1, "Name"]
            var result = new List<PathSegment>();
            var tokens = path.Split(new[] { '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var baseName = token;
                int? index = null;

                var bracketIndex = token.IndexOf('[');
                if (bracketIndex >= 0)
                {
                    baseName = token[..bracketIndex];
                    var indexStr = token[(bracketIndex + 1)..token.IndexOf(']')];
                    if (int.TryParse(indexStr, out var idx))
                        index = idx;
                }

                result.Add(new PathSegment(baseName, index));
            }
            return result;
        }

        private void SetNestedValue(object target, List<PathSegment> path, object? value)
        {
            object current = target;

            for (int i = 0; i < path.Count; i++)
            {
                var (name, index) = path[i];
                var type = current.GetType();
                var prop = ReflectionCache.FindProperty(type, name);
                if (prop == null) return;

                // Last segment - set the value
                if (i == path.Count - 1)
                {
                    SetValue(prop, current, value);
                    return;
                }

                // Get or create the nested object
                var currentValue = prop.GetValue(current);
                if (currentValue == null)
                {
                    currentValue = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(current, currentValue);
                }

                // For lists, ensure the element exists
                if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                {
                    var list = (IList)currentValue;
                    var elemType = ReflectionCache.GetListElementType(prop.PropertyType) ?? typeof(object);

                    // Ensure the list has enough elements
                    var idx = index ?? 0;
                    while (list.Count <= idx)
                        list.Add(Activator.CreateInstance(elemType));

                    current = list[idx]!;
                }
                else
                {
                    current = currentValue!;
                }
            }
        }

        private void SetValue(PropertyInfo prop, object target, object? value)
        {
            if (value == null)
            {
                prop.SetValue(target, null);
                return;
            }

            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            try
            {
                if (!targetType.IsAssignableFrom(value.GetType()))
                    value = Convert.ChangeType(value, targetType);
                prop.SetValue(target, value);
            }
            catch
            {
                logger.LogWarning("Failed to convert value {Value} to type {Type} for property {Property}",
                    value, targetType, prop.Name);
            }
        }

        private readonly record struct PathSegment(string Name, int? Index);
    }
}

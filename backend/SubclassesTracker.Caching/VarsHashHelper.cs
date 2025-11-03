using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SubclassesTracker.Caching
{
    public static class VarsHashHelper
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Computes a canonical hash (SHA256) for GraphQL variables or any JSON-like object.
        /// </summary>
        /// <param name="variables">Any serializable object.</param>
        /// <returns>Lowercase hexadecimal SHA256 hash.</returns>
        public static string ComputeHash(object variables)
        {
            var canonical = Canonicalize(variables);
            var bytes = Encoding.UTF8.GetBytes(canonical);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Converts object to canonical JSON with sorted properties.
        /// </summary>
        private static string Canonicalize(object obj)
        {
            var json = JsonSerializer.Serialize(obj, jsonOptions);
            var doc = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                WriteCanonicalJson(doc.RootElement, writer);
            }
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Recursively writes JSON with sorted keys for deterministic hashing.
        /// </summary>
        private static void WriteCanonicalJson(JsonElement element, Utf8JsonWriter writer)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var prop in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(prop.Name);
                        WriteCanonicalJson(prop.Value, writer);
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                        WriteCanonicalJson(item, writer);
                    writer.WriteEndArray();
                    break;
                default:
                    element.WriteTo(writer);
                    break;
            }
        }
    }
}

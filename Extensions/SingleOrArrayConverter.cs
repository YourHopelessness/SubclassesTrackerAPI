using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SubclassesTrackerExtension.Extensions
{
    public sealed class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(List<T>);

        public override object ReadJson(
            JsonReader reader, Type objectType,
            object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
                return token.ToObject<List<T>>(serializer) ?? new List<T>();

            var list = new List<T>
        {
            token.ToObject<T>(serializer)!
        };
            return list;
        }

        public override void WriteJson(
            JsonWriter writer, object? value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);
    }
}

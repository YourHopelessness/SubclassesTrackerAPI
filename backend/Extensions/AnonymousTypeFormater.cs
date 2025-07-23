using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SubclassesTracker.Api.Extensions
{
    public static class AnonymousTypeFormater
    {
        public static string FlatAnonToHashString(this object anon, string prefix,
            string dateFormat = "yyyyMMddHHmmssfff",
            bool includeDate = true)
        {
            ArgumentNullException.ThrowIfNull(anon);

            var values = anon.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(p => FormatValue(p.GetValue(anon)));

            var core = string.Join("", values);
            var ts = includeDate 
                ? DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture)
                : "";

            return prefix + ComputeHashHex(core + ts, 255);
        }

        private static string ComputeHashHex(string input, int take)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var hex = Convert.ToHexStringLower(bytes);
            var takeHex = Math.Min(take, hex.Length);

            return hex[..takeHex];
        }

        static string FormatValue(object? v)
        {
            if (v == null) return "";
            if (v is string s) return s;
            if (v is Enum) return v.ToString()!;
            if (v is bool b) return b ? "true" : "false";
            if (v is DateTime dt) return dt.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            if (v is DateTimeOffset dto) return dto.ToString("yyyyMMddTHHmmsszzz", CultureInfo.InvariantCulture);
            if (v is IEnumerable en && v is not string)
                return string.Join(",", en.Cast<object?>().Select(x => x?.ToString() ?? ""));
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
        }

    }
}

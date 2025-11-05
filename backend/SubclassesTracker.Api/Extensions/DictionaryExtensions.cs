namespace SubclassesTracker.Api.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merge dictionary into one
        /// </summary>
        /// <param name="dicts">List of dicts</param>
        /// <returns>Dicts</returns>
        public static Dictionary<string, int> MergeDictionaries(this IEnumerable<Dictionary<string, int>> dicts)
        {
            var result = new Dictionary<string, int>();

            foreach (var dict in dicts)
            {
                foreach (var kvp in dict)
                {
                    if (result.ContainsKey(kvp.Key))
                        result[kvp.Key] += kvp.Value;
                    else
                        result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
    }
}

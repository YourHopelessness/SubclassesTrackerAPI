using System.Reflection;

namespace SubclassesTracker.GraphQL.Services
{
    public interface IQueryLoader
    {
        /// <summary>
        /// Loads the GraphQL query by name.
        /// </summary>
        /// <param name="queryName">Query name</param>
        /// <returns></returns>
        Task<string> LoadAsync(string queryName, CancellationToken token = default);
    }

    /// <summary>
    /// Loader for embedded GraphQL queries.
    /// </summary>
    public class EmbeddedQueryLoader : IQueryLoader
    {
        private readonly Assembly _asm = typeof(EmbeddedQueryLoader).Assembly;

        public async Task<string> LoadAsync(string queryName, CancellationToken token = default)
        {
            var name = _asm.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith($"{queryName}.graphql", StringComparison.Ordinal))
                ?? throw new FileNotFoundException($"GraphQL query '{queryName}' not found in resources.");

            await using var s = _asm.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Resource '{name}' resolved but stream is null.");

            using var r = new StreamReader(s);
            return await r.ReadToEndAsync(token);
        }
    }
}

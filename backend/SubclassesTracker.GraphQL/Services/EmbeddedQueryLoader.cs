using System.Text;

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
    /// Loader for GraphQL queries.
    /// </summary>
    public class ResxQueryLoader : IQueryLoader
    {
        public Task<string> LoadAsync(string queryName, CancellationToken token = default)
        {
            var obj = GraphQLResourcers.ResourceManager.GetObject(queryName)
                ?? throw new FileNotFoundException($"GraphQL query '{queryName}' not found in .resx.");

            if (obj is not byte[] bytes)
                throw new InvalidOperationException($"Resource '{queryName}' is not byte[].");

            var text = Encoding.UTF8.GetString(bytes);
            return Task.FromResult(text);
        }
    }
}

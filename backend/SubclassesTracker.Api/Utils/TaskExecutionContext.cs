namespace SubclassesTracker.Api.Utils
{
    /// <summary>
    /// Execution context for store access token
    /// </summary>
    public static class TaskExecutionContext
    {
        private static readonly AsyncLocal<string?> _accessToken = new();

        public static string? AccessToken
        {
            get => _accessToken.Value;
            set => _accessToken.Value = value;
        }
    }
}

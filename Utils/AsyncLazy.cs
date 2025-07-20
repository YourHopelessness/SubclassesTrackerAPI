using System.Runtime.CompilerServices;

namespace SubclassesTracker.Api.Utils
{
    public sealed class AsyncLazy<T>(Func<Task<T>> factory,
        LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication)
    {
        private readonly Lazy<Task<T>> _lazy = new(factory, mode);

        public Task<T> AsTask() => _lazy.Value;
        public TaskAwaiter<T> GetAwaiter() => _lazy.Value.GetAwaiter();
    }

}

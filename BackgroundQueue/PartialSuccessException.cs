namespace SubclassesTracker.Api.BackgroundQueue
{
    public class PartialSuccessException<TResult>(TResult currentResult, string errors) : Exception(errors)
    {
        public TResult? PartialResult { get; set; } = currentResult;
    }
}

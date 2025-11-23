namespace SubclassesTracker.Caching.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field)]
    public class CacheDirectoryOrderAttribute(int order) : Attribute
    {
        public int Order { get; } = order;
    }
}

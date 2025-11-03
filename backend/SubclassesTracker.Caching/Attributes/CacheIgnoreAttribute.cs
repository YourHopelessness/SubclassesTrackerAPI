namespace SubclassesTracker.Caching.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field)]
    public class CacheIgnoreAttribute : Attribute
    {
    }
}

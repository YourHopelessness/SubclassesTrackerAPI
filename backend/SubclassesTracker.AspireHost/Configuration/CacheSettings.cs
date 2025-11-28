namespace SubclassesTracker.AspireHost.Config
{
    public class CacheSettings
    {
        public static readonly string SectionName = "CachingSettings";
        public string CachePath { get; set; } = null!;
    }
}

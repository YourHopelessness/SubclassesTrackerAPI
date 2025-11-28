namespace SubclassesTracker.AspireHost.Config
{
    public class PgConfig
    {
        public static readonly string SectionName = "Postgres";
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string User { get; set; } = null!;
        public string UserPassword { get; set; } = null!;
        public string Database { get; set; } = null!;
        public string InitDatabase { get; set; } = null!;
        public string InitUserPassword { get; set; } = null!;
        public string InitUser { get; set; } = null!;
    }
}

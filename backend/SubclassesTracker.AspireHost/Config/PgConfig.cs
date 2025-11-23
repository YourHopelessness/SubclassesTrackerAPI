namespace SubclassesTracker.AspireHost.Config
{
    public class PgConfig
    {
        public static readonly string SectionName = "PostgresSettings";
        public string Host { get; set; } = "pg";
        public int Port { get; set; } = 5432;
        public string User { get; set; } = "subclasses_user";
        public string UserPassword { get; set; } = null!;
        public string Database { get; set; } = "subclasses_db";
        public string InitDatabase { get; set; } = "postgres";
        public string InitUserPassword { get; set; } = null!;
        public string InitUser { get; set; } = "postgres";
        public bool Persist { get; set; } = true;
    }
}

using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Entity;

namespace SubclassesTracker.Database.Context
{
    public class EsoContext(DbContextOptions<EsoContext> options) : DbContext(options)
    {
        public DbSet<EncounterType> EncounterTypes { get; set; }
        public DbSet<LineType> LineTypes { get; set; }
        public DbSet<SkillLine> SkillLines { get; set; }
        public DbSet<SkillTreeEntry> SkillTreeEntries { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<ZoneDifficulty> ZoneDifficulties { get; set; }
        public DbSet<Encounter> Encounters { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<ZoneDifficulty>()
                .HasKey(zd => new { zd.ZoneId, zd.DifficultyId });

            b.Entity<ZoneDifficulty>()
                .HasOne(zd => zd.Zone)
                .WithMany(z => z.ZoneDifficulties)
                .HasForeignKey(zd => zd.ZoneId);

            b.Entity<ZoneDifficulty>()
                .HasOne(zd => zd.Difficulty)
                .WithMany(d => d.ZoneDifficulties)
                .HasForeignKey(zd => zd.DifficultyId);

            b.Entity<Encounter>()
                .HasOne(e => e.Type)
                .WithMany(t => t.Encounters)
                .HasForeignKey(e => e.TypeId);

            b.Entity<Encounter>()
                .HasOne(e => e.Zone)
                .WithMany(z => z.Encounters)
                .HasForeignKey(e => e.ZoneId);

            b.Entity<Zone>()
                .HasOne(z => z.Type)
                .WithMany(t => t.Zones)
                .HasForeignKey(z => z.TypeId);

            b.Entity<SkillLine>()
                .HasOne(sl => sl.LineType)
                .WithMany(lt => lt.SkillLines)
                .HasForeignKey(sl => sl.LineTypeId);

            b.Entity<SkillTreeEntry>()
                .HasOne(st => st.SkillLine)
                .WithMany(sl => sl.SkillTreeEntries)
                .HasForeignKey(st => st.SkillLineId);

            b.Entity<Icon>()
                .HasMany(x => x.SkillLines)
                .WithOne(y => y.Icon)
                .HasForeignKey(x => x.IconId);
        }
    }
}

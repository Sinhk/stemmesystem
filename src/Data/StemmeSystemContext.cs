using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stemmesystem.Data
{
    public class StemmesystemContext : IdentityDbContext, IDataProtectionKeyContext
    {
        public DbSet<Delegat> Delegat => Set<Delegat>();
        public DbSet<Arrangement> Arrangement => Set<Arrangement>();
        public DbSet<Sak> Sak => Set<Sak>();
        public DbSet<Votering> Votering => Set<Votering>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        public StemmesystemContext(DbContextOptions<StemmesystemContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (!Database.IsSqlite())
                modelBuilder.HasDefaultSchema("stemme");

            modelBuilder.Entity<Arrangement>(e =>{});

            modelBuilder.Entity<Sak>(e =>{});
            modelBuilder.Entity<Votering>(e =>
            {
                e.HasMany(v => v.AvgitStemme).WithMany(d=> d.HarStemmtI);
            });

            modelBuilder.Entity<Stemme>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.RevoteKey);
            });

            modelBuilder.Entity<Delegat>(e =>
            {
                e.HasIndex(x => new { x.ArrangementId, x.Delegatnummer }).IsUnique();
                e.HasIndex(x => x.Delegatkode).IsUnique();
            });
        }
    }
    public class StemmesystemContextFactory : IDesignTimeDbContextFactory<StemmesystemContext>
    {
        public StemmesystemContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StemmesystemContext>();
            optionsBuilder.UseNpgsql("not important");

            return new StemmesystemContext(optionsBuilder.Options);
        }
    }
}

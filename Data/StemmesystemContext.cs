using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.MinSpeiding;

namespace Stemmesystem.Data
{
    public class StemmesystemContext : IdentityDbContext<ApplicationUser>, IPersistedGrantDbContext, IDataProtectionKeyContext
    {
        private readonly IOptions<OperationalStoreOptions>? _operationalStoreOptions;

        public DbSet<Delegat> Delegat => Set<Delegat>();
        public DbSet<Arrangement> Arrangement => Set<Arrangement>();
        public DbSet<Sak> Sak => Set<Sak>();
        public DbSet<Votering> Votering => Set<Votering>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        // IPersistedGrantDbContext
        public DbSet<PersistedGrant> PersistedGrants { get; set; } = null!;
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!;
        public DbSet<Key> Keys { get; set; } = null!;
        public DbSet<ServerSideSession> ServerSideSessions { get; set; } = null!;
        public DbSet<PushedAuthorizationRequest> PushedAuthorizationRequests { get; set; } = null!;

        public StemmesystemContext(DbContextOptions<StemmesystemContext> options, IOptions<OperationalStoreOptions>? operationalStoreOptions) : base(options)
        {
            _operationalStoreOptions = operationalStoreOptions;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("stemme");
            modelBuilder.ConfigurePersistedGrantContext(_operationalStoreOptions?.Value ?? new OperationalStoreOptions());

            modelBuilder.Entity<Arrangement>(e =>
            {
                e.OwnsOne(a => a.MinSpeidingOptions, ob =>
                {
                    ob.Property(o => o.Filter)
                        .HasMaxLength(200)
                        .HasConversion(
                            v => v!.RawFilter,
                            v => new ParticipantFilter(v)
                        );
                });
            });

            modelBuilder.Entity<Sak>(e =>{});
            modelBuilder.Entity<Votering>(e =>
            {
                e.HasMany(v => v.AvgitStemme).WithMany(d=> d.HarStemmtI);
            });

            modelBuilder.Entity<Stemme>(e =>
            {
                e.HasKey(s => s.Id);
            });

            modelBuilder.Entity<Delegat>(e =>
            {
                e.HasIndex(x => new { x.ArrangementId, x.Delegatnummer }).IsUnique();
                e.HasIndex(x => x.Delegatkode).IsUnique();
                e.Property(x => x.TilStede).HasDefaultValue(true);
                
                e
                    .HasMany(d=> d.HarStemmtI)
                    .WithMany(v => v.AvgitStemme)
                    .UsingEntity<Dictionary<string, object>>(
                        "DelegatVotering",
                        j => j.HasOne<Votering>().WithMany().OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Delegat>().WithMany().OnDelete(DeleteBehavior.ClientCascade));
            });
        }
    }
    
    public class StemmesystemContextFactory : IDesignTimeDbContextFactory<StemmesystemContext>
    {
        public StemmesystemContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StemmesystemContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=design_time;Username=design_time");
            return new StemmesystemContext(optionsBuilder.Options, null);
        }
    }
}

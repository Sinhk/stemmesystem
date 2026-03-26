using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.MinSpeiding;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;

namespace Stemmesystem.Data
{
    public class StemmesystemContext : ApiAuthorizationDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public DbSet<DelegateEntity> Delegates => Set<DelegateEntity>();
        public DbSet<Arrangement> Arrangements => Set<Arrangement>();
        public DbSet<Case> Cases => Set<Case>();
        public DbSet<Ballot> Ballots => Set<Ballot>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        public StemmesystemContext(DbContextOptions<StemmesystemContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options,operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("stemme");

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

            modelBuilder.Entity<Case>(e =>{});
            modelBuilder.Entity<Ballot>(e =>
            {
                e.HasMany(v => v.VotedDelegates).WithMany(d=> d.VotedIn);
            });

            modelBuilder.Entity<Vote>(e =>
            {
                e.HasKey(s => s.Id);
            });

            modelBuilder.Entity<DelegateEntity>(e =>
            {
                e.HasIndex(x => new { x.ArrangementId, x.DelegateNumber }).IsUnique();
                e.HasIndex(x => x.DelegateCode).IsUnique();
                e.Property(x => x.Present).HasDefaultValue(true);
                
                e
                    .HasMany(d=> d.VotedIn)
                    .WithMany(v => v.VotedDelegates)
                    .UsingEntity<Dictionary<string, object>>(
                        "DelegatVotering",
                        j => j.HasOne<Ballot>().WithMany().OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<DelegateEntity>().WithMany().OnDelete(DeleteBehavior.ClientCascade));
            });
        }
    }
}

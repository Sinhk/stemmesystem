using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Server.Data.Entities;

namespace Stemmesystem.Data;

public class StemmesystemContext(DbContextOptions<StemmesystemContext> options)
    : IdentityDbContext<StemmeUser>(options), IDataProtectionKeyContext
{
    public DbSet<Delegat> Delegat => Set<Delegat>();
    public DbSet<Arrangement> Arrangement => Set<Arrangement>();
    public DbSet<Sak> Sak => Set<Sak>();
    public DbSet<Votering> Votering => Set<Votering>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("stemme");

        builder.Entity<Arrangement>();

        builder.Entity<Sak>();
        builder.Entity<Votering>(e =>
        {
            e.HasMany(v => v.AvgitStemme).WithMany(d=> d.HarStemmtI);
        });

        builder.Entity<Stemme>(e =>
        {
            e.HasKey(s => s.Id);
        });

        builder.Entity<Delegat>(e =>
        {
            e.HasIndex(x => new { x.ArrangementId, x.Delegatnummer }).IsUnique();
            e.HasIndex(x => x.Delegatkode).IsUnique();
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
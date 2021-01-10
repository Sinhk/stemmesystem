using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stemmesystem.Data
{
    public class StemmesystemContext : DbContext
    {
        public DbSet<Delegat> Delegat => Set<Delegat>();
        public DbSet<Arrangement> Arrangement => Set<Arrangement>();
        public DbSet<Votering> Votering => Set<Votering>();

        public StemmesystemContext(DbContextOptions<StemmesystemContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
            });
        }

    }
}

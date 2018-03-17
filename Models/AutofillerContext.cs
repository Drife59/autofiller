using Microsoft.EntityFrameworkCore;

namespace Autofiller.Models
{
    public class AutofillerContext : DbContext
    {
        public AutofillerContext(DbContextOptions<AutofillerContext> options)
            : base(options)
        {
        }
        public DbSet<Website> Websites { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<Pivot> Pivots { get; set; }
        public DbSet<UserValue> UserValues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Merge> Merges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pivot>()
                .HasIndex(p => p.name)
                .IsUnique(true);
        }
    }
}
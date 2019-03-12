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
        public DbSet<Profil> Profils { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pivot>()
                .HasIndex(p => p.name)
                .IsUnique(true);

            //Set precision for decimal. Don't work. Don't know why
            //modelBuilder.Entity<UserValue>().Property(o => o.weight).HasPrecision(8, 4);
            //base.OnModelCreating(modelBuilder);
        }
    }
}
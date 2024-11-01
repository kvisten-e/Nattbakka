using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;

namespace nattbakka_server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Cex> Cex { get; set; }
        public DbSet<Transaction> Transaction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(entity =>
            {
                // Configure primary key
                entity.HasKey(t => t.Id);

                // Configure CexId as foreign key
                entity.Property(t => t.CexId)
                    .IsRequired();

                // Configure GroupId as regular property (not a foreign key)
                entity.Property(t => t.GroupId)
                    .IsRequired(false);  // Make it nullable if needed

                // Ignore the navigation property
                entity.Ignore(t => t.Group);

                // Configure other properties
                entity.Property(t => t.Tx).IsRequired();
                entity.Property(t => t.Address).IsRequired();
                entity.Property(t => t.Sol).IsRequired();
                entity.Property(t => t.SolChanged).IsRequired();
                entity.Property(t => t.Timestamp).IsRequired();
            });

            // Configure Cex relationship
            modelBuilder.Entity<Cex>(entity =>
            {
                entity.HasMany<Transaction>()
                    .WithOne()
                    .HasForeignKey(t => t.CexId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
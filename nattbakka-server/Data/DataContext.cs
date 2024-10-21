using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;

namespace nattbakka_server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Cex> cex { get; set; }
        public DbSet<Transaction> transaction { get; set; }

        public DbSet<Group> cex_group { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.cex_group)
                .WithMany(g => g.transaction)
                .HasForeignKey(t => t.group_id);

            base.OnModelCreating(modelBuilder);
        }

    }
}

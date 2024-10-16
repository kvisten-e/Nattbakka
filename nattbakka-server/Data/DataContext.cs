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
        public DbSet<Transaction> transactions { get; set; }

        public DbSet<Group> dex_groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.dex_groups)
                .WithMany(g => g.transactions)
                .HasForeignKey(t => t.group_id);

            base.OnModelCreating(modelBuilder);
        }

    }
}

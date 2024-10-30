using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;

namespace nattbakka_server.Data
{
    public class InMemoryDataContext : DbContext
    {
        public InMemoryDataContext(DbContextOptions<InMemoryDataContext> options) : base(options) { }

        public DbSet<Transaction> transaction { get; set; }

        public DbSet<Group> cex_group { get; set; }
        public DbSet<TransactionWithGroup> TransactionWithGroup { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;

namespace nattbakka_server.Data
{
    public class InMemoryDataContext : DbContext
    {
        public InMemoryDataContext(DbContextOptions<InMemoryDataContext> options) : base(options) { }

        public DbSet<Transaction> Transaction { get; set; }

        public DbSet<Group> Group { get; set; }
        public DbSet<TransactionGroup> TransactionGroup { get; set; }

    }
    
}

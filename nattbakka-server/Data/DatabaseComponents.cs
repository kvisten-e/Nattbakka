using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace nattbakka_server.Data
{
    public class DatabaseComponents
    {
        //private readonly DataContext _context;
        private readonly IDbContextFactory<DataContext> _contextFactory;

        public DatabaseComponents(IDbContextFactory<DataContext> contextFactory) {
            _contextFactory = contextFactory;
        }

        public async Task PostTransaction(ParsedTransaction pt, int dexId)
        {
            Console.WriteLine("Converting transaction");

            using var context = _contextFactory.CreateDbContext();

            var transaction = new Transaction()
            {
                tx = pt.tx,
                address = pt.receivingAddress,
                sol = pt.sol,
                dex_id = dexId,
            };

            Console.WriteLine("Saving to database: " + transaction.address);

            context.transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<List<Dex>> GetDexesAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var data = await context.dex.Where(d => d.active == true).ToListAsync();
            return data;
        }

    }
}

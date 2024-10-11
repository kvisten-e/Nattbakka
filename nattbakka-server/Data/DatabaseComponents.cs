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

        public async Task<List<Transaction>> GetTransactions(int dexId)
        {
            DateTime time_history = DateTime.Now.AddDays(-1);

            using var context = _contextFactory.CreateDbContext();

            var data = await context.transactions.Where(d =>
                d.dex_id == dexId
            ).ToListAsync();
            return data;
        }

        public async Task<List<Transaction>> GetTransactions(int dexId, int minSol = 0, int maxSol = 100, int group_id = 0, bool sol_changed = false)
        {
            DateTime time_history = DateTime.Now.AddDays(-1);
            
            using var context = _contextFactory.CreateDbContext();

            var data = await context.transactions.Where(d => 
                d.dex_id == dexId &&
                d.sol > minSol &&
                d.sol < maxSol &&
                d.group_id == group_id &&
                d.sol_changed == sol_changed &&
                d.timestamp > time_history
            ).ToListAsync();
            return data;
        }

    }
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }

}

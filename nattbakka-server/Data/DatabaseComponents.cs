using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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

        public async Task<List<Transaction>> GetTransactions(int dexId, int minSol = 1, int maxSol = 6, int group_id = 0, bool sol_changed = false)
        {
            DateTime time_history = DateTime.Now.AddDays(-5);
            
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


        public async Task<List<TransactionWithGroup>> GetTransactionsWithGroups()
        {
            using var context = _contextFactory.CreateDbContext();
            Console.WriteLine("HEJ");
            var data = await context.transactions
                .Include(t => t.dex_groups)  // Join with DexGroup
                .Select(t => new TransactionWithGroup
                {
                    id = t.id,
                    tx = t.tx,
                    address = t.address,
                    sol = t.sol,
                    sol_changed = t.sol_changed,
                    dex_id = t.dex_id,
                    group_id = t.group_id,
                    timestamp = t.timestamp,

                    total_wallets = t.dex_groups.total_wallets,
                    inactive_wallets = t.dex_groups.inactive_wallets,
                    time_different_unix = t.dex_groups.time_different_unix,
                    created = t.dex_groups.created
                }).ToListAsync();
            Console.WriteLine("RETURNING");

            return data;
        }


    }
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }

}

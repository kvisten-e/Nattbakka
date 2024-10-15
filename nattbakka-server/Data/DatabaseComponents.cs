using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            using var context = _contextFactory.CreateDbContext();

            var transaction = new Transaction()
            {
                tx = pt.tx,
                address = pt.receivingAddress,
                sol = pt.sol,
                dex_id = dexId,
                timestamp = DateTime.Now
            };

            context.transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<bool> AddGroupIdToTransaction(int transactionId, int groupId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.transactions
                    .Where(u => u.id == transactionId)
                    .ExecuteUpdateAsync(u =>
                        u.SetProperty(u => u.group_id, groupId)
                    );
                await context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Something went wrong file adding group id {groupId} to tx {transactionId} - error: {ex}");
                return false;
            }

        }

        public async Task UpdateTotalWalletsInGroup(int groupId)
        {
            int amount = await GetGroupAmount(groupId);
            using var context = _contextFactory.CreateDbContext();
           
            await context.dex_groups
                .Where(u => u.id == groupId)
                .ExecuteUpdateAsync(u =>
                    u.SetProperty(u => u.total_wallets, amount)
                );
            await context.SaveChangesAsync();


        }

        public async Task<int> CreateDexGroup(int totalWallets, int timeDifferentUnix)
        {
            using var context = _contextFactory.CreateDbContext();

            var group = new Group()
            {
                total_wallets = totalWallets,
                time_different_unix = timeDifferentUnix,
                created = DateTime.Now
            };

            context.dex_groups.Add(group);
            await context.SaveChangesAsync();
            int id = group.id;

            return id;
        }

        public async Task<int> GetGroupAmount(int groupId)
        {
            using var context = _contextFactory.CreateDbContext();
            int amount = await context.transactions
                .Where(d => d.group_id == groupId)
                .CountAsync();
            return amount;
        }

        public async Task<List<Dex>> GetDexesAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var data = await context.dex.Where(d => d.active == true).ToListAsync();
            return data;
        }

        public async Task<List<Transaction>> GetTransactions(int dexId, bool asNoTracking = false)
        {
            using var context = _contextFactory.CreateDbContext();
            DateTime time_history = DateTime.Now.AddDays(-1);

            var query = context.transactions.Where(t => 
                t.dex_id == dexId && 
                t.group_id == 0 && 
                t.timestamp > time_history
                );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactions(int dexId, int minSol = 1, int maxSol = 6, int group_id = 0, bool sol_changed = false, bool asNoTracking = false)
        {
            DateTime time_history = DateTime.Now.AddDays(-1);
            
            using var context = _contextFactory.CreateDbContext();

            var query = context.transactions.Where(d =>
                d.dex_id == dexId &&
                d.sol > minSol &&
                d.sol < maxSol &&
                d.group_id == group_id &&
                d.sol_changed == sol_changed &&
                d.timestamp > time_history
            );          
            
                if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }


        public async Task<List<TransactionWithGroup>> GetTransactionsWithGroups()
        {
            using var context = _contextFactory.CreateDbContext();
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

            return data;
        }


    }
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }

}

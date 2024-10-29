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
        private readonly IDbContextFactory<InMemoryDataContext> _inMemoryDataContext;
        private readonly IDbContextFactory<DataContext> _contextFactory;

        public DatabaseComponents(IDbContextFactory<DataContext> contextFactory, IDbContextFactory<InMemoryDataContext> inMemoryDataContext)
        {
            _contextFactory = contextFactory;
            _inMemoryDataContext = inMemoryDataContext;
        }

        private async Task<InMemoryDataContext> GetInMemoryDbContext()
        {
            return await Task.FromResult(_inMemoryDataContext.CreateDbContext());
        }

        private async Task<DataContext> GetDbContext()
        {
            return await Task.FromResult(_contextFactory.CreateDbContext());
        }

        public async Task PostTransaction(ParsedTransaction pt)
        {
            using var context = await GetInMemoryDbContext();
            var transaction = new Transaction()
            {
                tx = pt.signature,
                address = pt.receivingAddress,
                sol = pt.sol,
                cex_id = pt.cex_id,
                timestamp = DateTime.Now
            };

            context.transaction.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<bool> AddGroupIdToTransaction(int transactionId, int groupId)
        {
            try
            {
                using var context = await GetInMemoryDbContext();

                var transaction = await context.transaction.FirstOrDefaultAsync(u => u.id == transactionId);

                if (transaction == null)
                {
                    Console.WriteLine($"Transaction with ID {transactionId} not found.");
                    return false;
                }

                transaction.group_id = groupId;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong while adding group id {groupId} to tx {transactionId} - error: {ex}");
                return false;
            }
        }

        public async Task UpdateTotalWalletsInGroup(int groupId)
        {
            int amount = await GetGroupAmount(groupId);
            using var context = await GetInMemoryDbContext();

            var group = await context.cex_group.FirstOrDefaultAsync(u => u.id == groupId);

            if (group == null)
            {
                Console.WriteLine($"Group with ID {groupId} not found.");
                return;
            }

            group.total_wallets = amount;

            await context.SaveChangesAsync();
        }


        public async Task<int> CreateDexGroup(int totalWallets, int timeDifferentUnix)
        {
            using var context = await GetInMemoryDbContext();
            var group = new Group()
            {
                total_wallets = totalWallets,
                time_different_unix = timeDifferentUnix,
                created = DateTime.Now
            };

            context.cex_group.Add(group);
            await context.SaveChangesAsync();
            return group.id;
        }

        public async Task<int> GetGroupAmount(int groupId)
        {
            using var context = await GetInMemoryDbContext();
            int amount = await context.transaction
                .Where(d => d.group_id == groupId)
                .CountAsync();
            return amount;
        }

        public async Task<List<Cex>> GetCexesAsync()
        {
            using var context = await GetDbContext();
            var data = await context.cex.Where(d => d.active == true).ToListAsync();
            return data;
        }

        public async Task<List<Transaction>> GetTransactions(bool asNoTracking = false)
        {
            using var context = await GetInMemoryDbContext();
            DateTime time_history = DateTime.Now.AddDays(-1);

            var query = context.transaction.Where(t =>
                t.group_id == 0 &&
                t.timestamp > time_history
            );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactions(int minSol, bool asNoTracking = false)
        {
            using var context = await GetInMemoryDbContext();
            DateTime time_history = DateTime.Now.AddDays(-1);

            var query = context.transaction.Where(t =>
                t.sol >= minSol &&
                t.group_id == 0 &&
                t.timestamp > time_history
            );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactions(int minSol, int cex, bool asNoTracking = false)
        {
            using var context = await GetInMemoryDbContext();
            DateTime time_history = DateTime.Now.AddDays(-1);

            var query = context.transaction.Where(t =>
                t.sol >= minSol &&
                t.cex_id == cex &&
                t.group_id == 0 &&
                t.timestamp > time_history
            );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<TransactionWithGroup>> GetTransactionsWithGroups()
        {
            using var context = await GetInMemoryDbContext();

            var data = await context.transaction
                .Select(t => new TransactionWithGroup
                {
                    id = t.id,
                    tx = t.tx,
                    address = t.address,
                    sol = t.sol,
                    sol_changed = t.sol_changed,
                    cex_id = t.cex_id,
                    group_id = t.group_id,
                    timestamp = t.timestamp,
                    total_wallets = t.cex_group != null ? t.cex_group.total_wallets : 0,
                    inactive_wallets = t.cex_group != null ? t.cex_group.inactive_wallets : 0,
                    time_different_unix = t.cex_group != null ? t.cex_group.time_different_unix : 0,
                    created = t.cex_group != null ? t.cex_group.created : DateTime.MinValue
                }).ToListAsync();

            return data;
        }
    }
}

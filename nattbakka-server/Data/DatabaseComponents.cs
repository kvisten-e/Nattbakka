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

        public async Task PostTransactionInMemory(ParsedTransaction pt)
        {
            using var context = await GetInMemoryDbContext();
            var transaction = new Transaction()
            {
                Tx = pt.signature,
                Address = pt.receivingAddress,
                Sol = pt.sol,
                CexId = pt.cex_id,
                Timestamp = DateTime.Now
            };

            context.transaction.Add(transaction);
            await context.SaveChangesAsync();
        }
        public async Task PostTransactionDatabase(TransactionGroup group)
        {
            using var context = await GetDbContext();
            foreach (var transaction in group.Transactions)
            {   
                context.transaction.Add(transaction);
            }
            await context.SaveChangesAsync();
        }
        public async Task PostTransactionDatabase(Transaction transaction)
        {
            using var context = await GetDbContext();
            context.transaction.Add(transaction);
            await context.SaveChangesAsync();
        }
        
        
        public async Task<List<Transaction>> AddGroupIdToTransactions(List<Transaction> transactions, int groupId)
        {
            try
            {
                using var inMemoryContext = await GetInMemoryDbContext();

                foreach (var transaction in transactions)
                {
                    inMemoryContext.Attach(transaction);
                    transaction.GroupId = groupId;
                    inMemoryContext.Entry(transaction).Property(t => t.GroupId).IsModified = true;
                }
                await inMemoryContext.SaveChangesAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong while adding group id {groupId} to transactions - error: {ex}");
                return null;
            }
        }

        public async Task<Transaction> AddGroupIdToTransactions(Transaction transaction, int groupId)
        {
            try
            {
                using var inMemoryContext = await GetInMemoryDbContext();

                inMemoryContext.Attach(transaction);
                transaction.GroupId = groupId;
                inMemoryContext.Entry(transaction).Property(t => t.GroupId).IsModified = true;
                await inMemoryContext.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong while adding group id {groupId} to transaction - error: {ex}");
                return null;
            }
        }



        public async Task<Group> CreateDexGroup(int timeDifferentUnix)
        {
            using var inMemoryContext = await GetInMemoryDbContext();
            using var dbContext = await GetDbContext();

            var group = new Group()
            {
                TimeDifferentUnix = timeDifferentUnix,
                Created = DateTime.Now
            };

            inMemoryContext.cex_group.Add(group);
            await inMemoryContext.SaveChangesAsync();

            return group;
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
                t.GroupId == 0 &&
                t.Timestamp > time_history
            );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactions(double minSol, bool asNoTracking = false)
        {
            using var context = await GetInMemoryDbContext();
            DateTime time_history = DateTime.Now.AddDays(-1);

            var query = context.transaction.Where(t =>
                t.Sol >= minSol &&
                t.GroupId == 0 &&
                t.Timestamp > time_history
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
                t.Sol >= minSol &&
                t.CexId == cex &&
                t.GroupId == 0 &&
                t.Timestamp > time_history
            );

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public async Task<List<TransactionGroup>> GetTransactionsWithGroups()
        {
            using var context = await GetInMemoryDbContext();

            var transactions = await context.transaction
                .Where(t => t.GroupId != 0)
                .ToListAsync();

            var groups = await context.cex_group
                .Where(g => g.Id != 0)
                .ToListAsync();

            var transactionGroups = groups.Select(g => new TransactionGroup
            {
                Id = g.Id,
                Created = g.Created,
                TimeDifferentUnix = g.TimeDifferentUnix,
                Transactions = transactions
                    .Where(t => t.GroupId == g.Id)
                    .ToList()
            }).Where(g => g.Transactions.Any()).ToList();

            return transactionGroups;
        }

    }
}

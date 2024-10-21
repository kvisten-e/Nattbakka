﻿using Microsoft.EntityFrameworkCore;
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
                tx = pt.signature,
                address = pt.receivingAddress,
                sol = pt.sol,
                cex_id = dexId,
                timestamp = DateTime.Now
            };

            context.transaction.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<bool> AddGroupIdToTransaction(int transactionId, int groupId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.transaction
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
           
            await context.cex_group
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

            context.cex_group.Add(group);
            await context.SaveChangesAsync();
            int id = group.id;

            return id;
        }

        public async Task<int> GetGroupAmount(int groupId)
        {
            using var context = _contextFactory.CreateDbContext();
            int amount = await context.transaction
                .Where(d => d.group_id == groupId)
                .CountAsync();
            return amount;
        }

        public async Task<List<Cex>> GetCexesAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var data = await context.cex.Where(d => d.active == true).ToListAsync();
            return data;
        }

        public async Task<List<Transaction>> GetTransactions(bool asNoTracking = false)
        {
            using var context = _contextFactory.CreateDbContext();
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
            using var context = _contextFactory.CreateDbContext();
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
            using var context = _contextFactory.CreateDbContext();
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
            using var context = _contextFactory.CreateDbContext();
            var data = await context.transaction
                .Include(t => t.cex_group)  // Join with DexGroup
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

                    total_wallets = t.cex_group.total_wallets,
                    inactive_wallets = t.cex_group.inactive_wallets,
                    time_different_unix = t.cex_group.time_different_unix,
                    created = t.cex_group.created
                }).ToListAsync();

            return data;
        }


    }
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }

}

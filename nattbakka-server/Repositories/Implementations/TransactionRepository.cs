using Microsoft.Extensions.Caching.Distributed;
using nattbakka_server.Repositories.Interfaces;
using System;
using System.Text.Json;
using nattbakka_server.Models;
using Microsoft.EntityFrameworkCore;
using nattbakka_server.Data;


namespace nattbakka_server.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        // private readonly IDistributedCache _cache;
        private readonly DatabaseComponents _databaseComponents;

        public TransactionRepository(DatabaseComponents databaseComponents)
        {
            // _cache = cache;
            _databaseComponents = databaseComponents;
        }

        public async Task<TransactionGroup> AddTransactionGroupAsync(TransactionGroup group)
        {
            // Save to MySQL database
            await _databaseComponents.PostTransactionDatabase(group);
            
            // var cacheEntryOptions = new DistributedCacheEntryOptions
            // {
            //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            //     SlidingExpiration = TimeSpan.FromMinutes(5)
            // };
            //
            // var cacheKey = $"Group_{group.Id}";
            // var groupData = JsonSerializer.Serialize(group);

            // await _cache.SetStringAsync(cacheKey, groupData, cacheEntryOptions);

            return group;
        }
        
        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {

            await _databaseComponents.PostTransactionDatabase(transaction);
            
            // var cacheEntryOptions = new DistributedCacheEntryOptions
            // {
            //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            //     SlidingExpiration = TimeSpan.FromMinutes(5)
            // };
            //
            // var cacheKey = $"Group_{group.Id}";
            // var groupData = JsonSerializer.Serialize(group);

            // await _cache.SetStringAsync(cacheKey, groupData, cacheEntryOptions);

            return transaction;
        }
        
        
        

        // public async Task<TransactionGroup> GetTransactionGroupAsync(int groupId)
        // {
        //     //     var cacheKey = $"Group_{groupId}";
        //     //
        //     //     // Try fetching from cache first
        //     //     var cachedGroup = await _cache.GetStringAsync(cacheKey);
        //     //     if (!string.IsNullOrEmpty(cachedGroup))
        //     //     {
        //     //         return JsonSerializer.Deserialize<TransactionGroup>(cachedGroup);
        //     //     }
        //     //     return new TransactionGroup();
        //     // }
        // }
    }
}

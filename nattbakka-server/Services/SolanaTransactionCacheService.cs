using Enyim.Caching;
using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nattbakka_server.Models;
using System;
using System.Threading.Tasks;

namespace nattbakka_server.Services
{
    public class SolanaTransactionCacheService
    {
        private readonly IMemcachedClient _memcachedClient;
        private readonly ILogger<SolanaTransactionCacheService> _logger;
        private readonly TimeSpan _expirationTime = TimeSpan.FromDays(1);

        public SolanaTransactionCacheService(IMemcachedClient memcachedClient, ILogger<SolanaTransactionCacheService> logger)
        {
            _memcachedClient = memcachedClient;
            _logger = logger;
        }

        // Store a transaction in the cache with 1 day expiration using the transaction's ID as the key
        public async Task<bool> StoreTransactionAsync(TransactionMemCache transaction)
        {
            // Use the transaction's ID as the key
            string transactionKey = transaction.signature;
            _logger.LogInformation($"Storing transaction with key: {transactionKey}");

            return await _memcachedClient.SetAsync(transactionKey, transaction, _expirationTime);
        }

        // Retrieve transactions from the cache for the last day
        //public async Task<IActionResult> GetTransactionsForOneDayBack()
        //{
        //    _logger.LogDebug("Executing _memcachedClient.GetValueOrCreateAsync...");

        //    var cacheSeconds = 600;
        //    var posts = await _memcachedClient.GetValueOrCreateAsync(
        //        CacheKey,
        //        cacheSeconds,
        //        async () => await _blogPostService.GetRecent(10));

        //    _logger.LogDebug("Done _memcachedClient.GetValueOrCreateAsync");

        //    return Ok(posts);
        }


        // Remove a transaction from the cache using the transaction ID as the key
        //public async Task<bool> RemoveTransactionAsync(string transactionId)
        //{
        //    _logger.LogInformation($"Removing transaction with key: {transactionId}");
        //    return await _memcachedClient.RemoveAsync(transactionId);
        //}

        // Update a transaction in the cache using the transaction ID as the key

}

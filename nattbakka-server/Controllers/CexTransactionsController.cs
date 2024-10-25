using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using nattbakka_server.Models;
using nattbakka_server.Services;
using System.Collections.Generic;

namespace nattbakka_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CexTransactionsController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IEnumerable<string> _transactionCacheKeys;

        public CexTransactionsController(IMemoryCache cache, CexTransactionsService cexTransactionsService)
        {
            _cache = cache;
            _transactionCacheKeys = cexTransactionsService.GetTransactionCacheKeys(); // New method to access cache keys
        }

        [HttpGet("GetCachedTransactions")]
        public IActionResult GetCachedTransactions()
        {
            var cachedTransactions = new List<ParsedTransaction>();

            foreach (var cacheKey in _transactionCacheKeys)
            {
                if (_cache.TryGetValue<ParsedTransaction>(cacheKey, out var transaction) && transaction != null)
                {
                    cachedTransactions.Add(transaction);
                }
            }

            return Ok(cachedTransactions);
        }
    }
}

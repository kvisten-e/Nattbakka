using Microsoft.Extensions.Logging;
using nattbakka_server.Data;
using System.Linq;
using Transaction = nattbakka_server.Models.Transaction;
using TransactionWithGroup = nattbakka_server.Models.TransactionWithGroup;
using nattbakka_server.Models;
using System.Collections.Concurrent;
using nattbakka_server.Helpers;

namespace nattbakka_server.Services
{
    public class UpdateGroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly DatabaseComponents _databaseComponents;
        private readonly GetTransactionSolDecimals _solDecimals = new GetTransactionSolDecimals();
        public List<TransactionWithGroup> _cexGroups = new List<TransactionWithGroup>();
        private ConcurrentDictionary<int, bool> _changedTransactions = new ConcurrentDictionary<int, bool>();
        

        public UpdateGroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UpdateGroupService running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetCurrentCexGroups();




                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            _logger.LogInformation("UpdateGroupService is stopping.");
        }

        private async Task GetCurrentCexGroups()
        {
            _cexGroups = await _databaseComponents.GetTransactionsWithGroups();
        }

        //private async Task<ConcurrentDictionary<int, bool>> GetChangedSolBalabce(List<TransactionWithGroup> group)
        //{
        //    foreach(var transaction in _cexGroups)
        //    {
        //        if (transaction == null) continue;

        //        bool balanceChanged = _databaseComponents.GetWalletBalance(transaction.id);


        //    }
        //}


    }
}
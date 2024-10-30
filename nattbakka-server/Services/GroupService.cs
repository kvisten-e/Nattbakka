using Microsoft.Extensions.Logging;
using nattbakka_server.Data;
using System.Linq;
using Transaction = nattbakka_server.Models.Transaction;
using TransactionWithGroup = nattbakka_server.Models.TransactionWithGroup;
using nattbakka_server.Models;
using nattbakka_server.Helpers;


namespace nattbakka_server.Services
{
    public class GroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly DatabaseComponents _databaseComponents;
        public List<List<Transaction>> _createdGroupsList = new List<List<Transaction>>();
        public List<TransactionWithGroup> _cexGroups = new List<TransactionWithGroup>();
        public List<Transaction> _transactions = new List<Transaction>();
        public List<Cex> _cexes = new List<Cex>();
        private readonly GetTransactionSolDecimals _getDecimals = new GetTransactionSolDecimals();


        public GroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GroupService running.");
            _cexes = await _databaseComponents.GetCexesAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetCurrentCexGroups();

                int minSol = 1;
                _transactions = await _databaseComponents.GetTransactions(minSol, asNoTracking: true);

                foreach (var transaction in _transactions)
                {
                    if (await AddTxToActiveGroups(transaction)) continue;
                    if (CheckTxInCurrentGroupList(transaction)) continue;
                    CreateGroup(transaction);
                }


                foreach (var group in _createdGroupsList)
                {
                    if (group.Count < 3) continue;

                    int timeDifferent = CalculateUnixDifferent(group[0].timestamp, group[group.Count - 1].timestamp);
                    int idCreatedGroup = await _databaseComponents.CreateDexGroup(group.Count, timeDifferent);

                    if (idCreatedGroup <= 0) continue;

                    foreach (var tx in group)
                    {
                        await _databaseComponents.AddGroupIdToTransaction(tx.id, idCreatedGroup);
                    }
                }

                _createdGroupsList.Clear();
                _transactions.Clear();
                _cexGroups.Clear();


                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            }

            _logger.LogInformation("GroupService is stopping.");
        }

        private async Task GetCurrentCexGroups()
        {
            _cexGroups = await _databaseComponents.GetTransactionsWithGroups();
            
            //if(_cexGroups.Count == 0) return;
            //foreach(var group in _cexGroups)
            //{
            //    _logger.LogInformation($"GroupId: {group.group_id} - Addres: {group.address} - Sol: {group.sol} - Cex: {group.cex_id} - Timestamp: {group.timestamp}");
            //}
        }

        private async Task<bool> AddTxToActiveGroups(Transaction transaction)
        {

            int timeLimitUnix = 180;

            int? groupIdFound = _cexGroups
                .FirstOrDefault(t =>
                    t.cex_id == transaction.cex_id &&
                    ConvertDatetimeToUnix(transaction.timestamp) - ConvertDatetimeToUnix(t.timestamp) <= timeLimitUnix &&
                    ConvertDatetimeToUnix(transaction.timestamp) - ConvertDatetimeToUnix(t.timestamp) >= 0 &&
                    _getDecimals.GetTransactionSolDecimal(t.sol) == _getDecimals.GetTransactionSolDecimal(transaction.sol)
                )?.group_id;

            if (groupIdFound.HasValue && groupIdFound.Value > 0)
            {
                bool status = await _databaseComponents.AddGroupIdToTransaction(transaction.id, groupIdFound.Value);
                if (status)
                {
                    await _databaseComponents.UpdateTotalWalletsInGroup(groupIdFound.Value);
                }
                return true;
            }

            return false;
        }

        private bool CheckTxInCurrentGroupList(Transaction transaction) {


            foreach(var createdGroup in _createdGroupsList)
            {
                bool exist = createdGroup.Any(tr => tr.id == transaction.id);
                if (exist)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateGroup(Transaction transaction)
        {
            var leaderData = transaction;
            List<Transaction> createdGroup = new List<Transaction> { leaderData };

            while (true)
            {

                var newLeader = _transactions.FirstOrDefault(d =>
                    d.cex_id == leaderData.cex_id &&
                    d.timestamp > leaderData.timestamp &&
                    _getDecimals.GetTransactionSolDecimal(d.sol).Equals(_getDecimals.GetTransactionSolDecimal(leaderData.sol)) &&
                    (ConvertDatetimeToUnix(d.timestamp) - ConvertDatetimeToUnix(leaderData.timestamp)) <= 180 &&
                    _cexes.Any(a => a.address != transaction.address)
                    );

                if (newLeader == null)
                {
                    break;
                };

                createdGroup.Add(newLeader);
                leaderData = newLeader;
            }


            if (createdGroup.Count >= 3)
            {
                _createdGroupsList.Add(createdGroup);
            }

        }


        private long ConvertDatetimeToUnix(DateTime date)
        {
            return (long)date.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }
        
        private int CalculateUnixDifferent(DateTime first, DateTime last)
        {
            int unixSeconds = (int)(ConvertDatetimeToUnix(last) - ConvertDatetimeToUnix(first));
            return unixSeconds;
        }
    
    }
}
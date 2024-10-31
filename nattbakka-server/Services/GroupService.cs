using Microsoft.Extensions.Logging;
using nattbakka_server.Data;
using System.Linq;
using Transaction = nattbakka_server.Models.Transaction;
using nattbakka_server.Models;
using nattbakka_server.Helpers;
using nattbakka_server.Repositories.Interfaces;



namespace nattbakka_server.Services
{
    public class GroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly DatabaseComponents _databaseComponents;
        public List<List<Transaction>> _createdGroupsList = new List<List<Transaction>>();
        public List<TransactionGroup> _cexGroups = new List<TransactionGroup>();
        public List<Transaction> _transactions = new List<Transaction>();
        public List<Cex> _cexes = new List<Cex>();
        private readonly GetTransactionSolDecimals _getDecimals = new GetTransactionSolDecimals();
        private readonly ITransactionRepository _transactionRepository;


        public GroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents, ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
            _transactionRepository = transactionRepository;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GroupService running.");
            _cexes = await _databaseComponents.GetCexesAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetCurrentCexGroups();

                double minSol = 0.1;
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

                    int timeDifferent = CalculateUnixDifferent(group[0].Timestamp, group[group.Count - 1].Timestamp);
                    var createdGroup = await _databaseComponents.CreateDexGroup(timeDifferent);

                    if (createdGroup.Id <= 0) continue;

                    var transactionsWithId = await _databaseComponents.AddGroupIdToTransactions(group, createdGroup.Id);
                    if (transactionsWithId is not null)
                    {
                        await _transactionRepository.AddTransactionGroupAsync(new TransactionGroup
                        {
                            Id = createdGroup.Id,
                            Created = createdGroup.Created,
                            TimeDifferentUnix = timeDifferent,
                            Transactions = group
                        });
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

            if (_cexGroups.Count == 0) return;
            foreach (var group in _cexGroups)
            {
                foreach(var transaction in group.Transactions)
                {
                    _logger.LogInformation($"GroupId: {group.Id} - Addres: {transaction.Address} - Sol: {transaction.Sol} - Cex: {transaction.CexId} - Timestamp: {transaction.Timestamp}");
                }

            }
        }

        private async Task<bool> AddTxToActiveGroups(Transaction transaction)
        {
            int timeLimitUnix = 180;

            int? groupIdFound = _cexGroups
                .FirstOrDefault(group => group.Transactions.Any(t =>
                    t.CexId == transaction.CexId &&
                    ConvertDatetimeToUnix(transaction.Timestamp) - ConvertDatetimeToUnix(t.Timestamp) <= timeLimitUnix &&
                    ConvertDatetimeToUnix(transaction.Timestamp) - ConvertDatetimeToUnix(t.Timestamp) >= 0 &&
                    _getDecimals.GetTransactionSolDecimal(t.Sol) == _getDecimals.GetTransactionSolDecimal(transaction.Sol)
                ))?.Id;

            if (groupIdFound.HasValue && groupIdFound.Value > 0)
            {
                var transactionWithId = await _databaseComponents.AddGroupIdToTransactions(transaction, groupIdFound.Value);
                if (transactionWithId is not null)
                {
                    await _transactionRepository.AddTransactionAsync(transactionWithId);
                }
            }

            return false;
        }


        private bool CheckTxInCurrentGroupList(Transaction transaction) {


            foreach(var createdGroup in _createdGroupsList)
            {
                bool exist = createdGroup.Any(tr => tr.Id == transaction.Id);
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
                    d.CexId == leaderData.CexId &&
                    d.Timestamp > leaderData.Timestamp &&
                    _getDecimals.GetTransactionSolDecimal(d.Sol).Equals(_getDecimals.GetTransactionSolDecimal(leaderData.Sol)) &&
                    (ConvertDatetimeToUnix(d.Timestamp) - ConvertDatetimeToUnix(leaderData.Timestamp)) <= 180 &&
                    _cexes.Any(a => a.address != transaction.Address)
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
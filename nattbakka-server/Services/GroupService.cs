using Microsoft.Extensions.Logging;
using nattbakka_server.Data;
using System.Linq;
using System.Diagnostics;
using Transaction = nattbakka_server.Models.Transaction;
using TransactionWithGroup = nattbakka_server.Models.TransactionWithGroup;
using nattbakka_server.Models;
using Newtonsoft.Json;

namespace nattbakka_server.Services
{
    public class GroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly DatabaseComponents _databaseComponents;
        public List<List<Transaction>> _createdGroupsList = new List<List<Transaction>>();
        public List<TransactionWithGroup> _dexGroups = new List<TransactionWithGroup>();
        public List<Transaction> _transactions = new List<Transaction>();
        public List<Dex> _dexes = new List<Dex>();


        public GroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service running.");
            _dexes = await _databaseComponents.GetDexesAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetCurrentDexGroups();

                foreach (var group in _dexGroups)
                {
                    string json = JsonConvert.SerializeObject(group, Formatting.Indented);
                    Console.WriteLine(json);
                }

                //int binanceId = _dexes.Single(b => b.name == "Binance2").id;
                //_transactions = await _databaseComponents.GetTransactions(binanceId);

                //foreach (var transaction in _transactions)
                //{
                //    // 1. Kolla om transaktionen kan läggas till en aktiv grupp som ligger live
                //    if (AddTxToActiveGroups(transaction)) continue;
                //    // 2. Kolla om transaktionen redan finns med i en GroupList
                //    if (CheckTxInCurrentGroupList(transaction)) continue;
                //    // 3. Försök skapa en ny group med transaktionen
                //    CreateGroup(transaction);
                //}

                //int counter = 1;
                //foreach (var group in _createdGroupsList)
                //{
                //    Console.WriteLine($"Group {counter++} - Length: {group.Count}");

                //    foreach (var tx in group)
                //    {
                //        Console.WriteLine($"Time: {tx.timestamp} - Sol: {tx.sol}");
                //    }
                //}



                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            }

            _logger.LogInformation("Timed Background Service is stopping.");
        }

        private async Task GetCurrentDexGroups()
        {
            _dexGroups = await _databaseComponents.GetTransactionsWithGroups();
        }

        private bool AddTxToActiveGroups(Transaction transaction) {
            int timeLimitUnix = 3600;
            var groupIdFound = _dexGroups.First(t =>
                t.dex_id == transaction.dex_id &&
                ConvertDatetimeToUnix(transaction.timestamp) - ConvertDatetimeToUnix(t.created) <= timeLimitUnix &&
                GetTransactionSolDecimals(t.sol) == GetTransactionSolDecimals(transaction.sol)
            ).group_id;



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

            Console.WriteLine("Transaction id: " + leaderData.id);

            while (true)
            {

                var newLeader = _transactions.FirstOrDefault(d =>
                    d.timestamp > leaderData.timestamp &&
                    GetTransactionSolDecimals(d.sol) == GetTransactionSolDecimals(leaderData.sol) &&
                    (ConvertDatetimeToUnix(d.timestamp) - ConvertDatetimeToUnix(leaderData.timestamp)) <= 180
                    );

                if (newLeader == null)
                {
                    Console.WriteLine("newLeader is null");
                    break;
                };

                Console.WriteLine("New leader: " + newLeader.address);
                createdGroup.Add(newLeader);
                leaderData = newLeader;
            }


            if (createdGroup.Count >= 3)
            {
                _createdGroupsList.Add(createdGroup);
            }

        }

        private int GetTransactionSolDecimals(double sol)
        {
            int decimalsMax = 3;

            Console.WriteLine("Inside sol: " + sol);
            int firstDecimalIndex = (int)sol.ToString().IndexOf(",");
            
            if(firstDecimalIndex < 0)
            {
                Console.WriteLine("Värdet har inga decimaler");
                return (int)sol;
            }
            string solToString = sol.ToString();

            int acuallyDeciamlsOfSolValue = solToString.Length - firstDecimalIndex - 1;


            if (acuallyDeciamlsOfSolValue >= decimalsMax)
            {
                string decimalValue = sol.ToString().Substring(firstDecimalIndex + 1, decimalsMax);
                int decimalValueInt = Convert.ToInt32(decimalValue);
                return decimalValueInt;
            }
            else
            {
                string decimalValue = sol.ToString().Substring(firstDecimalIndex + 1, acuallyDeciamlsOfSolValue);
                int decimalValueInt = Convert.ToInt32(decimalValue);
                return decimalValueInt;
            }



        }
    
        private long ConvertDatetimeToUnix(DateTime date)
        {
            return (long)date.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }
    }
}
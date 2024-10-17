﻿using Microsoft.Extensions.Logging;
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
        public List<TransactionWithGroup> _cexGroups = new List<TransactionWithGroup>();
        public List<Transaction> _transactions = new List<Transaction>();
        public List<Cex> _cexes = new List<Cex>();


        public GroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service running.");
            _cexes = await _databaseComponents.GetCexesAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await GetCurrentDexGroups();

                int minSol = 1;
                _transactions = await _databaseComponents.GetTransactions(minSol, asNoTracking: true);



                foreach (var transaction in _transactions)
                {
                    // 1. Kolla om transaktionen kan läggas till en aktiv grupp som ligger live
                    if (await AddTxToActiveGroups(transaction)) continue;
                    // 2. Kolla om transaktionen redan finns med i en GroupList
                    if (CheckTxInCurrentGroupList(transaction)) continue;
                    // 3. Försök skapa en ny group med transaktionen
                    CreateGroup(transaction);
                }


                foreach (var group in _createdGroupsList)
                {
                    // 4. Skapa row i dex_groups - retunera id som skapas
                    int timeDifferent = CalculateUnixDifferent(group[0].timestamp, group[group.Count - 1].timestamp);
                    int idCreatedGroup = await _databaseComponents.CreateDexGroup(group.Count, timeDifferent);

                    if (idCreatedGroup <= 0) continue;

                    foreach (var tx in group)
                    {
                        // 5. Uppdatera varje transaktion i grupp med det id:et
                        await _databaseComponents.AddGroupIdToTransaction(tx.id, idCreatedGroup);

                    }
                }

                _createdGroupsList.Clear();
                _transactions.Clear();
                _cexGroups.Clear();


                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            }

            _logger.LogInformation("Timed Background Service is stopping.");
        }

        private async Task GetCurrentDexGroups()
        {
            _cexGroups = await _databaseComponents.GetTransactionsWithGroups();
        }

        private async Task<bool> AddTxToActiveGroups(Transaction transaction)
        {

            int timeLimitUnix = 180;

            int? groupIdFound = _cexGroups
                .FirstOrDefault(t =>
                    t.dex_id == transaction.dex_id &&
                    ConvertDatetimeToUnix(transaction.timestamp) - ConvertDatetimeToUnix(t.timestamp) <= timeLimitUnix &&
                    ConvertDatetimeToUnix(transaction.timestamp) - ConvertDatetimeToUnix(t.timestamp) >= 0 &&
                    GetTransactionSolDecimals(t.sol) == GetTransactionSolDecimals(transaction.sol)
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
                    d.dex_id == leaderData.dex_id &&
                    d.timestamp > leaderData.timestamp &&
                    GetTransactionSolDecimals(d.sol) == GetTransactionSolDecimals(leaderData.sol) &&
                    (ConvertDatetimeToUnix(d.timestamp) - ConvertDatetimeToUnix(leaderData.timestamp)) <= 180
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

        private int GetTransactionSolDecimals(double sol)
        {
            int decimalsMax = 3;
            string decimalValue;
            int firstDecimalIndex = (int)sol.ToString().IndexOf(",");
            
            if(firstDecimalIndex < 0)
            {
                return (int)sol;
            }
            string solToString = sol.ToString();

            int acuallyDeciamlsOfSolValue = solToString.Length - firstDecimalIndex - 1;

            if (acuallyDeciamlsOfSolValue >= decimalsMax)
            {
                decimalValue = sol.ToString().Substring(firstDecimalIndex + 1, decimalsMax);
            }
            else
            {
                decimalValue = sol.ToString().Substring(firstDecimalIndex + 1, acuallyDeciamlsOfSolValue);
            }

            bool correctFormat = decimalValue.IndexOf("-") == -1;
            if(correctFormat)
            {
                int decimalValueInt = Convert.ToInt32(decimalValue);
                return decimalValueInt;
            }
            
            return 0;

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
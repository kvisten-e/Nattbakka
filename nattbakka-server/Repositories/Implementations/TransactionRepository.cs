using Microsoft.Extensions.Caching.Distributed;
using nattbakka_server.Repositories.Interfaces;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using nattbakka_server.Models;
using Microsoft.EntityFrameworkCore;
using nattbakka_server.Data;
using nattbakka_server.Services;


namespace nattbakka_server.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        // private readonly IDistributedCache _cache;
        private readonly DatabaseComponents _databaseComponents;
        private readonly IHubContext<GroupHub> _hubContext;

        public TransactionRepository(DatabaseComponents databaseComponents, IHubContext<GroupHub> hubContext)
        {
            _databaseComponents = databaseComponents;
            _hubContext = hubContext;
        }

        public async Task<TransactionGroup> AddTransactionGroupAsync(TransactionGroup group)
        {
            // Save to MySQL database
            await _databaseComponents.PostTransactionDatabase(group);
            
            // Push update to client
            await _hubContext.Clients.All.SendAsync("ReceiveTransactionGroupUpdate", group);
            
            return group;
        }
        
        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            // Save to MySQL database
            await _databaseComponents.PostTransactionDatabase(transaction);
            
            // Push transaction to client
            await _hubContext.Clients.All.SendAsync("ReceiveNewTransactionToGroup", transaction);

            return transaction;
        }
        
        
    }
}

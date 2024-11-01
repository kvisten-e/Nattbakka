using Microsoft.AspNetCore.SignalR;
using nattbakka_server.Models;

namespace nattbakka_server.Services;

public class GroupHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("CexGroups", $"{Context.ConnectionId} has joined");
    }
    
    public async Task UpdateGroups(List<TransactionGroup> transactionGroups)
    {
        await Clients.All.SendAsync("CexGroups", "**Grupper uppdateras här**");
    }
}
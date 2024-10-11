using Microsoft.Extensions.Logging;
using nattbakka_server.Data;

namespace nattbakka_server.Services
{
    public class GroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly DatabaseComponents _databaseComponents;

        public GroupService(ILogger<GroupService> logger, DatabaseComponents databaseComponents)
        {
            _logger = logger;
            _databaseComponents = databaseComponents;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service running.");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Fetching data from database: ");
                int DexId = 1;
                int MinSol = 1;
                int MaxSol = 6;
                var transactions = await _databaseComponents.GetTransactions(DexId, MinSol, MaxSol);
                foreach (var trans in transactions)
                {
                    Console.WriteLine($"Address: {trans.address}\nSol: {trans.sol}\nDatetime: {trans.timestamp}");
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            _logger.LogInformation("Timed Background Service is stopping.");
        }
    }
}
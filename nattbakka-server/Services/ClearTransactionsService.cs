using nattbakka_server.Data;
using Microsoft.Extensions.Logging;

namespace nattbakka_server.Services;

public class ClearTransactionsService
{
    private readonly InMemoryDataContext _inMemoryDataContext;
    private readonly ILogger<ClearTransactionsService> _logger;
    public ClearTransactionsService(InMemoryDataContext inMemoryDataContext, ILogger<ClearTransactionsService> logger)
    {
        _inMemoryDataContext = inMemoryDataContext;
        _logger = logger;
    }
    
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClearTransactionsService started execution.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting cleanup cycle at {Time}", DateTime.UtcNow);

                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                _logger.LogInformation("Cutoff time for cleanup: {Cutoff}", cutoff);

                var oldGroups = _inMemoryDataContext.Group.Where(g => g.Created < cutoff).ToList();
                _logger.LogInformation("Found {Count} groups to remove.", oldGroups.Count);
                _inMemoryDataContext.Group.RemoveRange(oldGroups);

                var oldTransactionGroups = _inMemoryDataContext.TransactionGroup.Where(tg => tg.Created < cutoff).ToList();
                _logger.LogInformation("Found {Count} transaction groups to remove.", oldTransactionGroups.Count);
                _inMemoryDataContext.TransactionGroup.RemoveRange(oldTransactionGroups);

                var oldTransactions = _inMemoryDataContext.Transaction.Where(t => t.Created < cutoff).ToList();
                _logger.LogInformation("Found {Count} transactions to remove.", oldTransactions.Count);
                _inMemoryDataContext.Transaction.RemoveRange(oldTransactions);

                _inMemoryDataContext.SaveChanges();
                _logger.LogInformation("Cleanup cycle completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the cleanup cycle.");
            }

            _logger.LogInformation("Waiting for the next cycle.");
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

        _logger.LogInformation("ClearTransactionsService execution stopped.");
    }
}
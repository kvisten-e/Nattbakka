using nattbakka_server.Data;

namespace nattbakka_server.Services;

public class ClearTransactionsService
{
    private readonly InMemoryDataContext _inMemoryDataContext;
    
    public ClearTransactionsService(InMemoryDataContext inMemoryDataContext)
    {
        _inMemoryDataContext = inMemoryDataContext;
    }
    
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime breakpoint = DateTime.Now.AddHours(-1);

            var entitiesToRemove = _inMemoryDataContext.Transaction.Where(e => e.Timestamp < breakpoint);

            _inMemoryDataContext.Transaction.RemoveRange(entitiesToRemove);

            _inMemoryDataContext.SaveChanges();
            
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
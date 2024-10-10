using MySqlConnector;
using Solnet.Programs.Models.Stake;

namespace nattbakka_server.Services
{
    public sealed class GroupService : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<GroupService> _logger;
        private readonly Task _completedTask = Task.CompletedTask;
        private Timer? _timer;

        public GroupService(ILogger<GroupService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("{Service} is running.", nameof(GroupService));
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return _completedTask;
        }

        private void DoWork(object? state)
        {
            Console.WriteLine("Test heh");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{Service} is stopping.", nameof(GroupService));

            _timer?.Change(Timeout.Infinite, 0);

            return _completedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer is IAsyncDisposable timer)
            {
                await timer.DisposeAsync();
            }
            _timer = null;
        }


    }
}

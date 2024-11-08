namespace nattbakka_server.Services
{
    public class ServiceRunner : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var groupService = scope.ServiceProvider.GetRequiredService<GroupService>();
            var updateGroupService = scope.ServiceProvider.GetRequiredService<UpdateGroupService>();
            var clearTransactionsService = scope.ServiceProvider.GetRequiredService<ClearTransactionsService>();

            await groupService.ExecuteAsync(stoppingToken);
            await updateGroupService.ExecuteAsync(stoppingToken);
            await clearTransactionsService.ExecuteAsync(stoppingToken);
            
        }
    }
}

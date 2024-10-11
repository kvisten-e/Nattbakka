namespace nattbakka_server.Services
{
    public class GroupServiceRunner : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public GroupServiceRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var groupService = scope.ServiceProvider.GetRequiredService<GroupService>();
            await groupService.ExecuteAsync(stoppingToken);
        }
    }
}

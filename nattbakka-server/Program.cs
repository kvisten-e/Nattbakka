using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nattbakka_server.Data;
using nattbakka_server.Models;
using nattbakka_server.Services;
using nattbakka_server.Options;
using nattbakka_server.Repositories.Implementations;
using nattbakka_server.Repositories.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<DataContext>(options => 
{
    string? connectionString = builder.Configuration.GetValue<string>("GetConnectionString:DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

});

builder.Services.AddDbContextFactory<InMemoryDataContext>(options =>
{
    options.UseInMemoryDatabase("TransactionsDb");
});


var cexGroupLog = new LoggerConfiguration()
    .WriteTo.File("./Logs/CexGroups.log", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();




builder.Logging.ClearProviders();
builder.Logging.AddSerilog(cexGroupLog);

builder.Services.AddSignalR();


builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<UpdateGroupService>();
builder.Services.AddScoped<ClearTransactionsService>();
builder.Services.AddHostedService<ServiceRunner>();

// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = "localhost:6379,connectTimeout=5000,syncTimeout=10000"; 
//     options.InstanceName = "TransactionCache";
// });



builder.Services.Configure<RpcApiKeysOptions>(options =>
{
    options.ShyftApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysShyft).Get<List<string>>();
    options.HeliusApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysHelius).Get<List<string>>();
});


builder.Services.AddScoped<DatabaseComponents>();
builder.Services.AddScoped<CexTransactionsService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GroupHub>("group-hub");


using (var scope = app.Services.CreateScope())
{
    var cexService = scope.ServiceProvider.GetRequiredService<CexTransactionsService>();
    await cexService.SolanaTransactionsWebSocket();
}

await app.RunAsync();





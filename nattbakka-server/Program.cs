using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nattbakka_server.Data;
using nattbakka_server.Models;
using nattbakka_server.Services;
using nattbakka_server.Options;
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


var logger = new LoggerConfiguration()
    .WriteTo.File("./Logs/CexGroups.log", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<UpdateGroupService>();
builder.Services.AddHostedService<GroupServiceRunner>();


builder.Services.Configure<RpcApiKeysOptions>(options =>
{
    options.ShyftApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysShyft).Get<List<string>>();
    options.HeliusApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysHelius).Get<List<string>>();
});


builder.Services.AddScoped<DatabaseComponents>();
builder.Services.AddScoped<CexTransactionsService>();

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

using (var scope = app.Services.CreateScope())
{
    // Starta bevakning av dexes, spara transaktioner till databasen
    var cexService = scope.ServiceProvider.GetRequiredService<CexTransactionsService>();
    await cexService.SolanaTransactionsWebSocket();

    // Leta/skapa grupper
    /// -> Startas automatisk med GroupService

    // Uppdatera grupper och markera förändrade wallets


    // Sätt upp APIer till frontend



    // wss://mainnet.helius-rpc.com/?api-key=ab19f7c7-c836-4bbc-ae73-74ea4eb2c9f8

}

await app.RunAsync();








// Fungerar
/*
 app.MapGet("/dexes", async (DataContext context) => await context.dex.ToListAsync());
 */



// Fungerar
/*
 * 
 * 
 * 
 * 
app.MapPost("/save-transaction", async (DatabaseComponents databaseComponents) =>
{
    var pt = new ParsedTransaction() {
        tx = "1212121212",
        receivingAddress = "3434343434",
        sendingAddress = "5656565656",
        sol = 1,
    };
    Console.WriteLine("Sendning..");
    await databaseComponents.PostTransaction(pt, 1);
});
*/


// Pausad, fungerar ej
/*app.MapGet("/active-websockets", (DexService dexService) =>
{
    // Get the active DEX names
    var activeDexNames = dexService.GetActiveDexNames();

    // Return the list as a JSON response
    return Results.Ok(activeDexNames);
});*/





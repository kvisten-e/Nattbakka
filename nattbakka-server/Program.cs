using Microsoft.EntityFrameworkCore;
using nattbakka_server.Data;
using nattbakka_server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<DataContext>(options => 
{
    string? connectionString = builder.Configuration.GetValue<string>("GetConnectionString:DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<GroupService>();
builder.Services.AddHostedService<GroupServiceRunner>();

var apiKeys = builder.Configuration.GetSection("ApiKeys").Get<List<string>>();

builder.Services.AddScoped<DatabaseComponents>();
builder.Services.AddScoped<DexService>();

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
    var dexService = scope.ServiceProvider.GetRequiredService<DexService>();
    await dexService.MonitorDexesAsync(apiKeys);

    // Leta/skapa grupper
    /// -> Startas automatisk med GroupService

    // Uppdatera grupper och markera förändrade wallets


    // Sätt upp APIer till frontend





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





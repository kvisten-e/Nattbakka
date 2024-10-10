using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nattbakka_server;
using nattbakka_server.Data;
using nattbakka_server.Models;
using nattbakka_server.Services;
using Solnet.Programs.Models.Stake;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<DataContext>(options => 
{
    string? connectionString = builder.Configuration.GetValue<string>("GetConnectionString:DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

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


app.MapGet("/dexes", async (DataContext context) => await context.dex.ToListAsync());

// Starta websockets
using (var scope = app.Services.CreateScope())
{
    var dexService = scope.ServiceProvider.GetRequiredService<DexService>();
    await dexService.MonitorDexesAsync(apiKeys);

}


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

app.Run();







// Pausad, fungerar ej
/*app.MapGet("/active-websockets", (DexService dexService) =>
{
    // Get the active DEX names
    var activeDexNames = dexService.GetActiveDexNames();

    // Return the list as a JSON response
    return Results.Ok(activeDexNames);
});*/





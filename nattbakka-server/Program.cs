using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nattbakka_server;
using nattbakka_server.Data;
using nattbakka_server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(options => 
{
    string connectionString = builder.Configuration.GetValue<string>("GetConnectionString:DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<DexRepository>();
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

app.MapGet("/balance", () =>
{
    return 10;
});

app.MapGet("/active-websockets", (DexService dexService) =>
{
    // Get the active DEX names
    var activeDexNames = dexService.GetActiveDexNames();

    // Return the list as a JSON response
    return Results.Ok(activeDexNames);
});


app.MapGet("/dexes", async (DataContext context) => await context.dex.ToListAsync());

// Starta websockets
using (var scope = app.Services.CreateScope())
{
    var dexService = scope.ServiceProvider.GetRequiredService<DexService>();
    await dexService.MonitorDexesAsync(); 
}

app.Run();



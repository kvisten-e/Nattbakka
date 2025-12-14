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
    
    DotNetEnv.Env.Load(); 
    var connectionString = Environment.GetEnvironmentVariable("MyDb_ConnectionString")
                           ?? throw new InvalidOperationException("MyDb_ConnectionString not configured");

    builder.Services.AddDbContextFactory<DataContext>(options => options.UseSqlServer(connectionString));

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
    builder.Services.AddScoped<DatabaseComponents>();
    builder.Services.AddScoped<CexTransactionsService>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    
    builder.Services.AddHostedService<ServiceRunner>();

    builder.Services.Configure<RpcApiKeysOptions>(options =>
    {
        options.ShyftApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysShyft).Get<List<string>>();
        options.HeliusApiKeys = builder.Configuration.GetSection(RpcApiKeysOptions.ApiKeysHelius).Get<List<string>>();
    });





    var app = builder.Build();


    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors(x => x.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials());

    app.UseWebSockets();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<GroupHub>("cexGroupHub");


    using (var scope = app.Services.CreateScope())
    {
        var cexService = scope.ServiceProvider.GetRequiredService<CexTransactionsService>();
        await cexService.SolanaTransactionsWebSocket();
    }

    await app.RunAsync();





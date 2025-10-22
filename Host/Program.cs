using ContinentExpress.TT.Api.Extensions;
using ContinentExpress.TT.DataAccess;
using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Calculate;
using ContinetExpress.TT.Logic.Calculate.Decorators.Caching;
using ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage;
using ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage.Repositories;
using ContinetExpress.TT.Logic.Models;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.ScriptProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddSingleton<IDistanceCalculator, DistanceCalculator>()
    .AddSingleton<IHandler<DistanceRequest, double?>, DistanceHandler>()
    .Decorate<IHandler<DistanceRequest, double?>, LocalStorageDecorator>()
    .Decorate<IHandler<DistanceRequest, double?>, DistanceCacheDecorator>()
    .AddSingleton<IRedisDbFactory, RedisDbFactory>()
    .AddSingleton<IAirportsRepository, AirportsRepository>()
    .AddSingleton<NpgsqlDataSource>(sp => NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("Db")!))
    .Configure<RedisSettings>(o=>
    {
        o.ConnectionString = builder.Configuration.GetConnectionString(RedisSettings.ConnectionStringName) 
            ?? throw new InvalidOperationException("Не задана строка подключения к Redis");
    })
    .Configure<PlacesApiSettings>(o => builder.Configuration.GetSection(PlacesApiSettings.SectionName).Bind(o))
    .AddRefitClient<IPlacesApi>()
    .ConfigureHttpClient((sp, httpClient) =>
    {
        var placesApiSettings = sp.GetRequiredService<IOptions<PlacesApiSettings>>().Value;
        httpClient.BaseAddress = placesApiSettings.BaseUri;
        httpClient.Timeout = placesApiSettings.Timeout;
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

var dbConnString = app.Configuration.GetConnectionString("Db") ?? throw new InvalidOperationException("Не задана строка подключения к БД");
var logger = new MicrosoftUpgradeLog(app.Services.GetRequiredService<ILogger<Program>>());
EnsureDatabase.For.PostgresqlDatabase(dbConnString, logger);
EnsureDatabase.For.PostgresqlSchema(dbConnString, "public", logger);

var upgrader =
    DeployChanges.To
        .PostgresqlDatabase(dbConnString)
        .LogTo(logger)
        .WithScriptsFromFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Migrations"), new FileSystemScriptOptions
        {
            Extensions = ["*.sql", "*.psql"]
        })
        .WithTransactionPerScript();

UpgradeEngine upgradeEngine = upgrader
    .LogScriptOutput()
    .JournalToPostgresqlTable("public", "migrations")
    .Build();

if (upgradeEngine.IsUpgradeRequired())
{
    upgradeEngine.PerformUpgrade();
}
else
{
    logger.LogInformation("Обновление схемы БД не требуется");
}

app.MapGet("/distance/{src}/{dst}", (HttpContext ctx, string src, string dst, [FromServices] IHandler<DistanceRequest, double?> distanceHandler) => 
    distanceHandler.HandleAsync(new(src, dst), ctx.RequestAborted)
)
.WithName("distance")
.Produces<float>(StatusCodes.Status200OK)
.WithOpenApi();

app.Run();

using ContinetExpress.TT.Logic;
using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using Refit;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddScoped<IDistanceCalculator, DistanceCalculator>()
    .AddScoped<IHandler<DistanceRequest, double>, DistanceHandler>()
    .Decorate<IHandler<DistanceRequest, double>, DistanceCacheDecorator>()
    .Configure<RedisSettings>(o => o.ConnectionString = builder.Configuration.GetConnectionString(RedisSettings.ConnectionStringName)!)
    .AddResiliencePipeline(Consts.PollyRetryPipeline, builder =>
    {
        builder.AddRetry(new RetryStrategyOptions()
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<RedisConnectionException>()
                .Handle<RedisTimeoutException>(),
            MaxRetryAttempts = 3
        });
    })
    .AddRefitClient<IPlacesApi>()
    .ConfigureHttpClient((sp, httpClient) =>
    {
        var section = builder.Configuration.GetSection("PlacesApiSettings");
        httpClient.BaseAddress = section.GetValue<Uri>("BaseUri");
        httpClient.Timeout = section.GetValue<TimeSpan>("Timeout");
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.MapGet("/distance/{src}/{dst}", (HttpContext ctx, string src, string dst, [FromServices] IHandler<DistanceRequest, double> distanceHandler) => 
    distanceHandler.HandleAsync(new(src, dst), ctx.RequestAborted)
)
.WithName("distance")
.Produces<float>(StatusCodes.Status200OK)
.WithOpenApi();

app.Run();

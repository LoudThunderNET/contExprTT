using ContinentExpress.TT.Api.Extensions;
using ContinetExpress.TT.Logic;
using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Models;
using ContinetExpress.TT.Logic.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Refit;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddScoped<IDistanceCalculator, DistanceCalculator>()
    .AddScoped<IHandler<DistanceRequest, double>, DistanceHandler>()
    .Decorate<IHandler<DistanceRequest, double>, DistanceCacheDecorator>()
    .AddSingleton<IRedisDbFactory, RedisDbFactory>()
    .AddSingleton<RedisConfiguration>(builder.Configuration.GetSection("Redis").Get<RedisConfiguration>()!)
    .AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(sp => [sp.GetRequiredService<RedisConfiguration>()])
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

using ContinetExpress.TT.Logic;
using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddScoped<IHandler<DistanceRequest, float>, DistanceHandler>()
    .AddRefitClient<IPlacesApi>()
    .ConfigureHttpClient((sp, httpClient) => 
    {
        var section= builder.Configuration.GetSection("PlacesApiSettings");
        httpClient.BaseAddress = section.GetValue<Uri>("BaseUri");
        httpClient.Timeout = section.GetValue<TimeSpan>("Timeout");
    })
    .AddStandardHedgingHandler().Configure(o => o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10))
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o=> o.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.MapGet("/distance/{src}/{dst}", (HttpContext ctx, string src, string dst, [FromServices] IHandler<DistanceRequest, float> distanceHandler) => 
    distanceHandler.HandleAsync(new(src, dst), ctx.RequestAborted)
)
.WithName("distance")
.Produces<float>(StatusCodes.Status200OK)
.WithOpenApi();

app.Run();

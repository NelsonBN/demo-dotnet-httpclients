using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient("DemoClient", (serviceProvider, client)
    => client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"));


var app = builder.Build();

app.MapGet("/posts", async (
    ILogger<Program> logger,
    IHttpClientFactory factory,
    CancellationToken cancellationToken) =>
{
    var tasks = Enumerable.Range(0, 10000).Select(async _ =>
    {
        var client = factory.CreateClient("DemoClient");
        var response = await client.GetAsync("posts", cancellationToken);

        logger.LogInformation("Status code: {StatusCode}", response.StatusCode);

        return response.StatusCode;
    });

    await Task.WhenAll(tasks);

    logger.LogInformation("All requests completed successfully.");

    return Results.Ok("Requests completed");
});

await app.RunAsync();

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

app.MapGet("/posts", async (
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var tasks = Enumerable.Range(0, 10000).Select(async _ =>
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts");

        logger.LogInformation("Status code: {StatusCode}", response.StatusCode);

        return response.StatusCode;
    });

    await Task.WhenAll(tasks);

    logger.LogInformation("All requests completed successfully.");

    return Results.Ok("Requests completed");
});

await app.RunAsync();

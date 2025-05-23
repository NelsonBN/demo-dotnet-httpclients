using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient<DemoApiClient>((serviceProvider, client) =>
{
    var endpoint = serviceProvider.GetRequiredService<IConfiguration>()["DemoApiEndpoint"] ?? throw new AggregateException();
    client.BaseAddress = new Uri(endpoint);
});

builder.Services.AddHttpClient("DemoApi", (serviceProvider, client) =>
{
    var endpoint = serviceProvider.GetRequiredService<IConfiguration>()["DemoApiEndpoint"] ?? throw new AggregateException();
    client.BaseAddress = new Uri(endpoint);
});


var app = builder.Build();

app.MapGet("/typed/posts", async (DemoApiClient client, CancellationToken cancellationToken) =>
    await client.GetPostsAsync(cancellationToken) ?? Enumerable.Empty<Post>());

app.MapGet("/named/posts", async (IHttpClientFactory factory, CancellationToken cancellationToken) =>
{
    var client = factory.CreateClient("DemoApi");
    return await client.GetFromJsonAsync<IEnumerable<Post>>("posts", cancellationToken);
});

app.MapGet("/factory/posts", async (IConfiguration configuration, IHttpClientFactory factory, CancellationToken cancellationToken) =>
{
    var endpoint = configuration["DemoApiEndpoint"] ?? throw new AggregateException();
    var client = factory.CreateClient();
    return await client.GetFromJsonAsync<IEnumerable<Post>>($"{endpoint}/posts", cancellationToken);
});

await app.RunAsync();



public class DemoApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public Task<IEnumerable<Post>?> GetPostsAsync(CancellationToken cancellationToken = default)
        => _httpClient.GetFromJsonAsync<IEnumerable<Post>>("posts", cancellationToken);

}

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
}

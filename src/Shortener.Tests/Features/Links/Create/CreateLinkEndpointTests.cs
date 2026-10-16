using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shortener.Api.Features.Links.Create;
using Shortener.Api.Infrastructure.Persistence;
using Shortener.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Shortener.Tests.Features.Links.Create;

public sealed class CreateLinkEndpointTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"shortener-{Guid.NewGuid():N}.db");
    private ShortenerApiFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new ShortenerApiFactory(_databasePath);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://short.example")
        });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    [Fact]
    public async Task PostCreatesLinkAndReturnsRequestDerivedShortUrl()
    {
        const string targetUrl = "https://example.com/article";

        var response = await _client.PostAsJsonAsync("/api/links", new { targetUrl });
        var result = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Matches("^[A-Za-z1-9]{7}$", result.Code);
        Assert.Equal($"https://short.example/{result.Code}", result.ShortUrl);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Links.SingleAsync(link => link.Code == result.Code);
        Assert.Equal(targetUrl, stored.TargetUrl);
    }

    [Fact]
    public async Task PostReturnsProblemDetailsForInvalidUrl()
    {
        var response = await _client.PostAsJsonAsync("/api/links", new { targetUrl = "ftp://example.com/file" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(body);
        Assert.Contains("URL de destino", problem.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task PostAcceptsCustomCodeAndTreatsCasingAsDistinct()
    {
        var upperResponse = await _client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com/upper",
            code = "ABC1"
        });
        var upper = await upperResponse.Content.ReadFromJsonAsync<CreateLinkResponse>();
        var lowerResponse = await _client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com/lower",
            code = "abc1"
        });
        var lower = await lowerResponse.Content.ReadFromJsonAsync<CreateLinkResponse>();

        Assert.Equal(HttpStatusCode.Created, upperResponse.StatusCode);
        Assert.Equal("ABC1", upper?.Code);
        Assert.Equal(HttpStatusCode.Created, lowerResponse.StatusCode);
        Assert.Equal("abc1", lower?.Code);
    }

    [Fact]
    public async Task PostRejectsDuplicateCustomCodeWithProblemDetails()
    {
        const string code = "Duplicate1";
        await _client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com/one", code });

        var response = await _client.PostAsJsonAsync("/api/links", new { targetUrl = "https://example.com/two", code });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(body);
        Assert.Contains("já está", problem.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task PostRejectsCustomCodeOutsideAllowedAlphabet()
    {
        var response = await _client.PostAsJsonAsync("/api/links", new
        {
            targetUrl = "https://example.com/invalid",
            code = "Code0"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}

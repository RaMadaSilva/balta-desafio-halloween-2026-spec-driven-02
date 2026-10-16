using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shortener.Api.Domain.Links;
using Shortener.Api.Infrastructure.Persistence;
using Shortener.Tests.Infrastructure;

namespace Shortener.Tests.Features.Links.Redirect;

public sealed class RedirectEndpointTests : IAsyncLifetime
{
    private static readonly DateTimeOffset NowUtc = new(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"shortener-{Guid.NewGuid():N}.db");
    private ShortenerApiFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new ShortenerApiFactory(_databasePath, new FrozenTimeProvider(NowUtc));
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://short.example"),
            AllowAutoRedirect = false
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
    public async Task GetRedirectsActiveLinkWith302()
    {
        await SeedLink("Active1", null);

        var response = await _client.GetAsync("/Active1");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("https://example.com/target", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task GetReturns404ForUnknownCode()
    {
        var response = await _client.GetAsync("/Missing1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReturns410AtExactExpiryInstant()
    {
        await SeedLink("Expired", NowUtc);

        var response = await _client.GetAsync("/Expired");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact]
    public async Task GetRedirectsOneTickBeforeExpiry()
    {
        await SeedLink("Before1", NowUtc.AddTicks(1));

        var response = await _client.GetAsync("/Before1");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private async Task SeedLink(string code, DateTimeOffset? expiresAtUtc)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Links.Add(new Link(code, "https://example.com/target", NowUtc.AddMinutes(-1), expiresAtUtc));
        await db.SaveChangesAsync();
    }

    private sealed class FrozenTimeProvider(DateTimeOffset nowUtc) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => nowUtc;
    }
}

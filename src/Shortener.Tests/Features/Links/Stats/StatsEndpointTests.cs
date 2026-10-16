using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shortener.Api.Domain.Links;
using Shortener.Api.Features.Links.Stats;
using Shortener.Api.Infrastructure.Persistence;
using Shortener.Tests.Infrastructure;

namespace Shortener.Tests.Features.Links.Stats;

public sealed class StatsEndpointTests : IAsyncLifetime
{
    private static readonly DateTimeOffset NowUtc = new(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);
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
    public async Task GetStatsReturnsCountWithoutAuthenticationForActiveLink()
    {
        await SeedLink("Stats01", 3, null);

        var response = await _client.GetAsync("/api/links/Stats01/stats");
        var result = await response.Content.ReadFromJsonAsync<LinkStatsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, result?.AccessCount);
    }

    [Fact]
    public async Task GetStatsRemainsAvailableForExpiredLink()
    {
        await SeedLink("Stats02", 5, NowUtc.AddMinutes(-1));

        var response = await _client.GetAsync("/api/links/Stats02/stats");
        var result = await response.Content.ReadFromJsonAsync<LinkStatsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, result?.AccessCount);
    }

    [Fact]
    public async Task GetStatsReturns404ForUnknownCode()
    {
        var response = await _client.GetAsync("/api/links/Unknown1/stats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task SeedLink(string code, int accessCount, DateTimeOffset? expiresAtUtc)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var link = new Link(code, "https://example.com/target", NowUtc.AddMinutes(-5), expiresAtUtc);
        db.Links.Add(link);
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Links SET AccessCount = {accessCount} WHERE Code = {code}");
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Shortener.Api.Features.Links.Create;
using Shortener.Api.Features.Links.Stats;
using Shortener.Tests.Infrastructure;

namespace Shortener.Tests.Infrastructure.Persistence;

public sealed class RestartPersistenceTests
{
    [Fact]
    public async Task CreatedLinkAndDataRemainAvailableAfterApplicationRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"shortener-{Guid.NewGuid():N}.db");
        ShortenerApiFactory? firstFactory = null;
        ShortenerApiFactory? secondFactory = null;

        try
        {
            firstFactory = new ShortenerApiFactory(databasePath);
            using (var firstClient = CreateClient(firstFactory))
            {
                var created = await firstClient.PostAsJsonAsync("/api/links", new
                {
                    targetUrl = "https://example.com/persisted",
                    code = "Persist1"
                });

                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            }

            await firstFactory.DisposeAsync();
            firstFactory = null;
            SqliteConnection.ClearAllPools();

            secondFactory = new ShortenerApiFactory(databasePath);
            using var secondClient = CreateClient(secondFactory);

            var statsBeforeRedirect = await secondClient.GetFromJsonAsync<LinkStatsResponse>("/api/links/Persist1/stats");
            var redirect = await secondClient.GetAsync("/Persist1");
            var statsAfterRedirect = await secondClient.GetFromJsonAsync<LinkStatsResponse>("/api/links/Persist1/stats");

            Assert.Equal(0, statsBeforeRedirect?.AccessCount);
            Assert.Equal(HttpStatusCode.Found, redirect.StatusCode);
            Assert.Equal("https://example.com/persisted", redirect.Headers.Location?.ToString());
            Assert.Equal(1, statsAfterRedirect?.AccessCount);
        }
        finally
        {
            if (firstFactory is not null)
            {
                await firstFactory.DisposeAsync();
            }

            if (secondFactory is not null)
            {
                await secondFactory.DisposeAsync();
            }

            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private static HttpClient CreateClient(ShortenerApiFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://short.example"),
            AllowAutoRedirect = false
        });
}

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Shortener.Tests.Infrastructure;

namespace Shortener.Tests.Infrastructure.ApiDocumentation;

public sealed class ApiDocumentationTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"shortener-{Guid.NewGuid():N}.db");
    private ShortenerApiFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new ShortenerApiFactory(_databasePath, environment: Environments.Production);
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
    public async Task OpenApiDocumentIsAvailableInProductionAndContainsApiRoutes()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(body);
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/links").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/{code}").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/links/{code}/stats").TryGetProperty("get", out _));
    }

    [Fact]
    public async Task ScalarReferenceIsAvailableInProduction()
    {
        var response = await _client.GetAsync("/scalar");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("scalar", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LaunchProfilesOpenScalarAutomatically()
    {
        var projectDirectory = FindApiProjectDirectory();
        var launchSettingsPath = Path.Combine(projectDirectory, "Properties", "launchSettings.json");
        using var settings = JsonDocument.Parse(File.ReadAllText(launchSettingsPath));
        var profiles = settings.RootElement.GetProperty("profiles");

        foreach (var profileName in new[] { "http", "https" })
        {
            var profile = profiles.GetProperty(profileName);
            Assert.True(profile.GetProperty("launchBrowser").GetBoolean());
            Assert.Equal("scalar", profile.GetProperty("launchUrl").GetString());
        }
    }

    private static string FindApiProjectDirectory()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDirectory is not null)
        {
            var projectPath = Path.Combine(currentDirectory.FullName, "Shortener.Api", "Shortener.Api.csproj");
            if (File.Exists(projectPath))
            {
                return Path.GetDirectoryName(projectPath)!;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Shortener.Api.csproj.");
    }
}

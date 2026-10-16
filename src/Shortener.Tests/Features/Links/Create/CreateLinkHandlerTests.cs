using Shortener.Api.Infrastructure.Persistence;
using Shortener.Api.Features.Links.Create;

namespace Shortener.Tests.Features.Links.Create;

public sealed class CreateLinkHandlerTests
{
    [Fact]
    public async Task HandleRetriesGeneratedCodeWhenRepositoryReportsCollision()
    {
        var repository = new ScriptedLinkRepository(false, true);
        var generator = new SequenceCodeGenerator("Taken12", "Free123");
        var handler = new CreateLinkHandler(repository, generator, TimeProvider.System);

        var result = await handler.HandleAsync(new CreateLinkRequest("https://example.com"), "https", "short.example");

        Assert.True(result.IsSuccess);
        Assert.Equal("Free123", result.Code);
        Assert.Equal("https://short.example/Free123", result.ShortUrl);
        Assert.Equal(new[] { "Taken12", "Free123" }, generator.GeneratedCodes);
        Assert.Equal(new[] { "Taken12", "Free123" }, repository.AttemptedCodes);
    }

    [Fact]
    public async Task HandleRejectsCustomCodeWhenRepositoryReportsDuplicate()
    {
        var repository = new ScriptedLinkRepository(false);
        var generator = new SequenceCodeGenerator();
        var handler = new CreateLinkHandler(repository, generator, TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateLinkRequest("https://example.com", "Taken1"),
            "https",
            "short.example");

        Assert.False(result.IsSuccess);
        Assert.Contains("já está", result.Error);
        Assert.Empty(generator.GeneratedCodes);
    }

    [Fact]
    public async Task HandlePersistsValidatedExpiryAsUtc()
    {
        var repository = new ScriptedLinkRepository(true);
        var generator = new SequenceCodeGenerator("Free123");
        var handler = new CreateLinkHandler(repository, generator, TimeProvider.System);
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

        var result = await handler.HandleAsync(
            new CreateLinkRequest("https://example.com", ExpiresAtUtc: expiresAtUtc),
            "https",
            "short.example");

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse(expiresAtUtc).ToUniversalTime(), repository.AddedLinks.Single().ExpiresAtUtc);
    }

    private sealed class SequenceCodeGenerator(params string[] codes) : IShortCodeGenerator
    {
        private readonly Queue<string> _codes = new(codes);
        public List<string> GeneratedCodes { get; } = [];

        public string Generate()
        {
            var code = _codes.Dequeue();
            GeneratedCodes.Add(code);
            return code;
        }
    }

    private sealed class ScriptedLinkRepository(params bool[] insertResults) : ILinkRepository
    {
        private readonly Queue<bool> _insertResults = new(insertResults);
        public List<string> AttemptedCodes { get; } = [];
        public List<LinkRecord> AddedLinks { get; } = [];

        public Task<bool> TryAddAsync(LinkRecord link, CancellationToken cancellationToken = default)
        {
            AttemptedCodes.Add(link.Code);
            AddedLinks.Add(link);
            return Task.FromResult(_insertResults.Dequeue());
        }

        public Task<LinkRecord?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult<LinkRecord?>(null);

        public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> TryIncrementAccessCountAsync(
            string code,
            DateTimeOffset nowUtc,
            CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}

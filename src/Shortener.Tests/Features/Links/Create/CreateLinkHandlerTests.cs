using Shortener.Api.Domain.Links;
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

        var result = await handler.HandleAsync("https://example.com", "https", "short.example");

        Assert.True(result.IsSuccess);
        Assert.Equal("Free123", result.Code);
        Assert.Equal("https://short.example/Free123", result.ShortUrl);
        Assert.Equal(new[] { "Taken12", "Free123" }, generator.GeneratedCodes);
        Assert.Equal(new[] { "Taken12", "Free123" }, repository.AttemptedCodes);
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

        public Task<bool> TryAddAsync(Link link, CancellationToken cancellationToken = default)
        {
            AttemptedCodes.Add(link.Code);
            return Task.FromResult(_insertResults.Dequeue());
        }

        public Task<Link?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult<Link?>(null);

        public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

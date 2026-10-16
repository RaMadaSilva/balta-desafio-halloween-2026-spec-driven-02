using Shortener.Api.Domain.Links;

namespace Shortener.Tests.Domain.Links;

public sealed class LinkTests
{
    [Fact]
    public void RegisterAccessOnlyIncreasesAccessCount()
    {
        var link = new Link("Count01", "https://example.com", DateTimeOffset.UtcNow);

        Assert.Equal(0, link.AccessCount);
        link.RegisterAccess();
        link.RegisterAccess();

        Assert.Equal(2, link.AccessCount);
    }
}

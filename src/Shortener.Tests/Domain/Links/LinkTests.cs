using Shortener.Api.Domain.Links;

namespace Shortener.Tests.Domain.Links;

public sealed class LinkTests
{
    [Fact]
    public void NextAccessCountIncreasesCountByOne()
    {
        var link = new Link("Count01", "https://example.com", DateTimeOffset.UtcNow);

        var nextAccessCount = Link.NextAccessCount.Compile();

        Assert.Equal(1, nextAccessCount(link));
    }

    [Fact]
    public void IsActiveAtRequiresExpiryToBeStrictlyAfterNow()
    {
        var nowUtc = new DateTimeOffset(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);
        var isActiveAt = Link.IsActiveAt(nowUtc).Compile();

        Assert.True(isActiveAt(new Link("Active1", "https://example.com", nowUtc)));
        Assert.True(isActiveAt(new Link("Active2", "https://example.com", nowUtc, nowUtc.AddTicks(1))));
        Assert.False(isActiveAt(new Link("Expired", "https://example.com", nowUtc, nowUtc)));
        Assert.False(isActiveAt(new Link("Past001", "https://example.com", nowUtc, nowUtc.AddTicks(-1))));
    }
}

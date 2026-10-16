using Shortener.Api.Features.Links.Redirect;

namespace Shortener.Tests.Features.Links.Redirect;

public sealed class RedirectExpiryRuleTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsActiveAtReturnsTrueWhenLinkDoesNotExpire()
    {
        Assert.True(RedirectExpiryRule.IsActiveAt(null, NowUtc));
    }

    [Fact]
    public void IsActiveAtReturnsTrueBeforeExpiry()
    {
        Assert.True(RedirectExpiryRule.IsActiveAt(NowUtc.AddTicks(1), NowUtc));
    }

    [Fact]
    public void IsActiveAtReturnsFalseAtExactExpiry()
    {
        Assert.False(RedirectExpiryRule.IsActiveAt(NowUtc, NowUtc));
    }

    [Fact]
    public void IsActiveAtReturnsFalseAfterExpiry()
    {
        Assert.False(RedirectExpiryRule.IsActiveAt(NowUtc.AddTicks(-1), NowUtc));
    }
}

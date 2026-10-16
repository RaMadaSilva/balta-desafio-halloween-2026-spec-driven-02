using Shortener.Api.Features.Links.Create;

namespace Shortener.Tests.Features.Links.Create;

public sealed class ExpiryValidatorTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TryParseAcceptsFutureUtcInstant()
    {
        var valid = ExpiryValidator.TryParse("2026-10-16T15:00:01Z", NowUtc, out var expiresAtUtc, out var error);

        Assert.True(valid);
        Assert.Null(error);
        Assert.Equal(NowUtc.AddSeconds(1), expiresAtUtc);
    }

    [Theory]
    [InlineData("2026-10-16T15:00:00Z")]
    [InlineData("2026-10-16T14:59:59Z")]
    public void TryParseRejectsCurrentOrPastInstant(string value)
    {
        var valid = ExpiryValidator.TryParse(value, NowUtc, out var expiresAtUtc, out var error);

        Assert.False(valid);
        Assert.Null(expiresAtUtc);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("2026-10-16T15:00:01+00:00")]
    [InlineData("2026-10-16T15:00:01Z\n")]
    [InlineData("16/10/2026 15:00:01Z")]
    [InlineData("not-a-date")]
    public void TryParseRejectsValuesThatAreNotUtcIsoInstants(string value)
    {
        var valid = ExpiryValidator.TryParse(value, NowUtc, out _, out var error);

        Assert.False(valid);
        Assert.NotNull(error);
    }
}

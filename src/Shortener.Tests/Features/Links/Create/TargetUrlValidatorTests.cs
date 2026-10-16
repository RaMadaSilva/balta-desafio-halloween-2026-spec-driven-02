using Shortener.Api.Features.Links.Create;

namespace Shortener.Tests.Features.Links.Create;

public sealed class TargetUrlValidatorTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?q=one")]
    public void ValidateAcceptsAbsoluteHttpUrls(string targetUrl)
    {
        Assert.Null(TargetUrlValidator.Validate(targetUrl));
    }

    [Fact]
    public void ValidateAcceptsUrlAtMaximumLength()
    {
        var prefix = "https://example.com/";
        var targetUrl = prefix + new string('a', 2048 - prefix.Length);

        Assert.Null(TargetUrlValidator.Validate(targetUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/relative/path")]
    [InlineData("ftp://example.com/file")]
    [InlineData("http:example.com")]
    public void ValidateRejectsInvalidOrUnsupportedUrls(string? targetUrl)
    {
        Assert.NotNull(TargetUrlValidator.Validate(targetUrl));
    }

    [Fact]
    public void ValidateRejectsUrlOverMaximumLength()
    {
        var targetUrl = "https://example.com/" + new string('a', 2049);

        Assert.NotNull(TargetUrlValidator.Validate(targetUrl));
    }
}

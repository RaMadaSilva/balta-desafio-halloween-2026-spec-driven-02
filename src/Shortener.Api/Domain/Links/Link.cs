using System.Linq.Expressions;

namespace Shortener.Api.Domain.Links;

public sealed class Link
{
    public static Expression<Func<Link, bool>> IsActiveAt(DateTimeOffset nowUtc) =>
        link => link.ExpiresAtUtc == null || link.ExpiresAtUtc > nowUtc;

    public static Expression<Func<Link, long>> NextAccessCount =>
        link => link.AccessCount + 1;

    private Link()
    {
    }

    public Link(string code, string targetUrl, DateTimeOffset createdAtUtc, DateTimeOffset? expiresAtUtc = null)
    {
        Code = code;
        TargetUrl = targetUrl;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc?.ToUniversalTime();
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string TargetUrl { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public long AccessCount { get; private set; }

}

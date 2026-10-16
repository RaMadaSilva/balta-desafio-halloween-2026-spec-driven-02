namespace Shortener.Api.Infrastructure.Persistence;

public sealed class LinkRecord
{
    private LinkRecord()
    {
    }

    public LinkRecord(string code, string targetUrl, DateTimeOffset createdAtUtc, DateTimeOffset? expiresAtUtc = null)
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

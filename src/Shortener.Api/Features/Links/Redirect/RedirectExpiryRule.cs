namespace Shortener.Api.Features.Links.Redirect;

public static class RedirectExpiryRule
{
    public static bool IsActiveAt(DateTimeOffset? expiresAtUtc, DateTimeOffset nowUtc) =>
        expiresAtUtc is null || expiresAtUtc > nowUtc;
}

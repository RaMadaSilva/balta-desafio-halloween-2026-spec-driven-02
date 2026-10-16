namespace Shortener.Api.Features.Links.Create;

public sealed record CreateLinkRequest(string? TargetUrl, string? Code = null, string? ExpiresAtUtc = null);

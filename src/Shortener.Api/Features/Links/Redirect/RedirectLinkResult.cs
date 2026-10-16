namespace Shortener.Api.Features.Links.Redirect;

public sealed record RedirectLinkResult(RedirectStatus Status, string? TargetUrl = null);

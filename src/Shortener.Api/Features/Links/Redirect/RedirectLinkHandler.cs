using Shortener.Api.Domain.Links;

namespace Shortener.Api.Features.Links.Redirect;

public sealed class RedirectLinkHandler(ILinkRepository repository, TimeProvider timeProvider)
{
    public async Task<RedirectLinkResult> HandleAsync(string code, CancellationToken cancellationToken = default)
    {
        var link = await repository.GetByCodeAsync(code, cancellationToken);
        if (link is null)
        {
            return new RedirectLinkResult(RedirectStatus.NotFound);
        }

        if (link.ExpiresAtUtc is { } expiresAtUtc && timeProvider.GetUtcNow() >= expiresAtUtc)
        {
            return new RedirectLinkResult(RedirectStatus.Expired);
        }

        return new RedirectLinkResult(RedirectStatus.Found, link.TargetUrl);
    }
}

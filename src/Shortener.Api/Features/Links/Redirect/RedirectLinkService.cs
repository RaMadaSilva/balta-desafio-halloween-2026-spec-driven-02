using Shortener.Api.Infrastructure.Persistence;

namespace Shortener.Api.Features.Links.Redirect;

public sealed class RedirectLinkService(ILinkRepository repository, TimeProvider timeProvider)
{
    public async Task<RedirectLinkResult> RedirectAsync(string code, CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow();
        var link = await repository.GetByCodeAsync(code, cancellationToken);
        if (link is null)
        {
            return new RedirectLinkResult(RedirectStatus.NotFound);
        }

        if (!RedirectExpiryRule.IsActiveAt(link.ExpiresAtUtc, nowUtc))
        {
            return new RedirectLinkResult(RedirectStatus.Expired);
        }

        var incremented = await repository.TryIncrementAccessCountAsync(code, nowUtc, cancellationToken);
        return incremented
            ? new RedirectLinkResult(RedirectStatus.Found, link.TargetUrl)
            : new RedirectLinkResult(RedirectStatus.NotFound);
    }
}

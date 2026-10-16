using Shortener.Api.Domain.Links;

namespace Shortener.Api.Features.Links.Redirect;

public sealed class RedirectLinkHandler(ILinkRepository repository, TimeProvider timeProvider)
{
    public async Task<RedirectLinkResult> HandleAsync(string code, CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow();
        if (await repository.TryIncrementAccessCountAsync(code, nowUtc, cancellationToken))
        {
            var validLink = await repository.GetByCodeAsync(code, cancellationToken);
            return validLink is null
                ? new RedirectLinkResult(RedirectStatus.NotFound)
                : new RedirectLinkResult(RedirectStatus.Found, validLink.TargetUrl);
        }

        var link = await repository.GetByCodeAsync(code, cancellationToken);
        return link is null
            ? new RedirectLinkResult(RedirectStatus.NotFound)
            : new RedirectLinkResult(RedirectStatus.Expired);

    }
}

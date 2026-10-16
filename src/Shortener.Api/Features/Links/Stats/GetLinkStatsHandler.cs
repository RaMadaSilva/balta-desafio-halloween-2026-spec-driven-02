using Shortener.Api.Infrastructure.Persistence;

namespace Shortener.Api.Features.Links.Stats;

public sealed class GetLinkStatsHandler(ILinkRepository repository)
{
    public async Task<long?> HandleAsync(string code, CancellationToken cancellationToken = default)
    {
        var link = await repository.GetByCodeAsync(code, cancellationToken);
        return link?.AccessCount;
    }
}

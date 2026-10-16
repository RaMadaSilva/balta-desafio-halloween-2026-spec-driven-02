using Shortener.Api.Infrastructure.Persistence;

namespace Shortener.Api.Features.Links.Stats;

public sealed class GetLinkStatsService(ILinkRepository repository)
{
    public async Task<long?> GetStatsAsync(string code, CancellationToken cancellationToken = default)
    {
        var link = await repository.GetByCodeAsync(code, cancellationToken);
        return link?.AccessCount;
    }
}

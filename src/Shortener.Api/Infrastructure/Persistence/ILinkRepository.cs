namespace Shortener.Api.Infrastructure.Persistence;

public interface ILinkRepository
{
    Task<bool> TryAddAsync(LinkRecord link, CancellationToken cancellationToken = default);
    Task<LinkRecord?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> TryIncrementAccessCountAsync(string code, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}

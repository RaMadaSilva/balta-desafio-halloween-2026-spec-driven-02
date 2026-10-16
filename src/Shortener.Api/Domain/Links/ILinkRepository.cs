namespace Shortener.Api.Domain.Links;

public interface ILinkRepository
{
    Task AddAsync(Link link, CancellationToken cancellationToken = default);
    Task<Link?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}

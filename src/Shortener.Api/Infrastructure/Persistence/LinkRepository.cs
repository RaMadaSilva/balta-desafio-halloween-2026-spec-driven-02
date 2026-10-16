using Microsoft.EntityFrameworkCore;
using Shortener.Api.Domain.Links;

namespace Shortener.Api.Infrastructure.Persistence;

public sealed class LinkRepository(AppDbContext dbContext) : ILinkRepository
{
    public async Task AddAsync(Link link, CancellationToken cancellationToken = default)
    {
        dbContext.Links.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Link?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Links.SingleOrDefaultAsync(link => link.Code == code, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Links.AnyAsync(link => link.Code == code, cancellationToken);
}

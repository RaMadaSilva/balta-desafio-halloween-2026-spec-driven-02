using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Shortener.Api.Infrastructure.Persistence;

public sealed class LinkRepository(AppDbContext dbContext) : ILinkRepository
{
    private const int UniqueConstraintErrorCode = 2067;

    public async Task<bool> TryAddAsync(LinkRecord link, CancellationToken cancellationToken = default)
    {
        dbContext.Links.Add(link);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.GetBaseException() is SqliteException { SqliteExtendedErrorCode: UniqueConstraintErrorCode })
        {
            dbContext.Entry(link).State = EntityState.Detached;
            return false;
        }
    }

    public Task<LinkRecord?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Links.SingleOrDefaultAsync(link => link.Code == code, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Links.AnyAsync(link => link.Code == code, cancellationToken);

    public async Task<bool> TryIncrementAccessCountAsync(
        string code,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default) =>
        await dbContext.Links
            .Where(link => link.Code == code)
            .Where(link => link.ExpiresAtUtc == null || link.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(update => update.SetProperty(link => link.AccessCount, link => link.AccessCount + 1), cancellationToken) == 1;
}

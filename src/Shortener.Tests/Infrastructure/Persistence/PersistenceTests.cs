using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shortener.Api.Domain.Links;
using Shortener.Api.Infrastructure.Persistence;

namespace Shortener.Tests.Infrastructure.Persistence;

public sealed class PersistenceTests
{
    [Fact]
    public async Task MigrationPersistsLinksAndAllowsCodesThatDifferByCase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        db.Links.AddRange(
            new Link("ABC", "https://example.com/one", DateTimeOffset.UtcNow),
            new Link("abc", "https://example.com/two", DateTimeOffset.UtcNow));

        await db.SaveChangesAsync();

        Assert.Equal(2, await db.Links.CountAsync());
    }

    [Fact]
    public async Task MigrationRejectsDuplicateCodesWithMatchingCase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        db.Links.Add(new Link("Same", "https://example.com/one", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        db.Links.Add(new Link("Same", "https://example.com/two", DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task TryIncrementAccessCountCountsValidLinksButNotExpiredLinks()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        var nowUtc = new DateTimeOffset(2026, 10, 16, 15, 0, 0, TimeSpan.Zero);
        db.Links.AddRange(
            new Link("Valid01", "https://example.com/valid", nowUtc.AddMinutes(-1)),
            new Link("Expired", "https://example.com/expired", nowUtc.AddMinutes(-1), nowUtc));
        await db.SaveChangesAsync();
        var repository = new LinkRepository(db);

        Assert.True(await repository.TryIncrementAccessCountAsync("Valid01", nowUtc));
        Assert.False(await repository.TryIncrementAccessCountAsync("Expired", nowUtc));
        Assert.Equal(1, await db.Links.Where(link => link.Code == "Valid01").Select(link => link.AccessCount).SingleAsync());
        Assert.Equal(0, await db.Links.Where(link => link.Code == "Expired").Select(link => link.AccessCount).SingleAsync());
    }
}

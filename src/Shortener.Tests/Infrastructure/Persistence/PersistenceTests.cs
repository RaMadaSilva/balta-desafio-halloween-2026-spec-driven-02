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
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shortener.Api.Domain.Links;
using Shortener.Api.Infrastructure.Persistence;

namespace Shortener.Tests.Infrastructure.Persistence;

public sealed class AppDbContextModelTests
{
    [Fact]
    public void LinkCodeUsesCaseSensitiveCollation()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var db = new AppDbContext(options);
        var code = db.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(Link))!.FindProperty(nameof(Link.Code))!;

        Assert.Equal("BINARY", code.GetCollation());
    }
}

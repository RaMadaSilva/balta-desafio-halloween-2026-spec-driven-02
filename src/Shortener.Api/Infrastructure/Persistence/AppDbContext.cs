using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shortener.Api.Domain.Links;

namespace Shortener.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<DateTimeOffset, long> UtcTicksConverter =
        new(value => value.UtcDateTime.Ticks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var link = modelBuilder.Entity<Link>();

        link.ToTable("Links");
        link.HasKey(item => item.Id);
        link.Property(item => item.Code).IsRequired().UseCollation("BINARY");
        link.HasIndex(item => item.Code).IsUnique();
        link.Property(item => item.TargetUrl).IsRequired();
        link.Property(item => item.CreatedAtUtc).HasConversion(UtcTicksConverter);
        link.Property(item => item.ExpiresAtUtc).HasConversion(UtcTicksConverter);
        link.Property(item => item.AccessCount).IsRequired();
    }
}

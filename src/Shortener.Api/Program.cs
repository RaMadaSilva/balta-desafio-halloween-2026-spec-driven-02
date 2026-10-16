using Microsoft.EntityFrameworkCore;
using Shortener.Api.Domain.Links;
using Shortener.Api.Features.Links.Create;
using Shortener.Api.Features.Links.Redirect;
using Shortener.Api.Features.Links.Stats;
using Shortener.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Shortener")));
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CreateLinkHandler>();
builder.Services.AddScoped<RedirectLinkHandler>();
builder.Services.AddScoped<GetLinkStatsHandler>();

var app = builder.Build();
CreateLinkEndpoint.Map(app);
RedirectLinkEndpoint.Map(app);
GetLinkStatsEndpoint.Map(app);

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.Run();

public partial class Program { }

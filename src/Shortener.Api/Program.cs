using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<CreateLinkService>();
builder.Services.AddScoped<RedirectLinkService>();
builder.Services.AddScoped<GetLinkStatsService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.AllowedHosts = (builder.Configuration["AllowedHosts"] ?? string.Empty)
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    foreach (var proxy in builder.Configuration.GetSection("KnownProxies").GetChildren().Select(item => item.Value))
    {
        if (!IPAddress.TryParse(proxy, out var address))
        {
            throw new InvalidOperationException($"O endereço configurado em KnownProxies não é válido: {proxy}");
        }

        options.KnownProxies.Add(address);
    }
});

var app = builder.Build();
app.UseForwardedHeaders();
CreateLinkEndpoint.Map(app);
RedirectLinkEndpoint.Map(app);
GetLinkStatsEndpoint.Map(app);

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.Run();

public partial class Program { }

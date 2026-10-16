namespace Shortener.Api.Features.Links.Stats;

public static class GetLinkStatsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/links/{code}/stats", async (
            string code,
            GetLinkStatsService service,
            CancellationToken cancellationToken) =>
        {
            var accessCount = await service.GetStatsAsync(code, cancellationToken);
            return accessCount is null
                ? Results.NotFound()
                : Results.Ok(new LinkStatsResponse(accessCount.Value));
        });
    }
}

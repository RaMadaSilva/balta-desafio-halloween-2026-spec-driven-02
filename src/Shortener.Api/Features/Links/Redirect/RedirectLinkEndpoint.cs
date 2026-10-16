using Microsoft.AspNetCore.Http;

namespace Shortener.Api.Features.Links.Redirect;

public static class RedirectLinkEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/{code}", async (string code, RedirectLinkHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(code, cancellationToken);

            return result.Status switch
            {
                RedirectStatus.Found => Results.Redirect(result.TargetUrl!),
                RedirectStatus.Expired => Results.StatusCode(StatusCodes.Status410Gone),
                _ => Results.NotFound()
            };
        });
    }
}

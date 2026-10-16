using Microsoft.AspNetCore.Http;

namespace Shortener.Api.Features.Links.Redirect;

public static class RedirectLinkEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/{code}", async (string code, RedirectLinkService service, CancellationToken cancellationToken) =>
        {
            var result = await service.RedirectAsync(code, cancellationToken);

            return result.Status switch
            {
                RedirectStatus.Found => Results.Redirect(result.TargetUrl!),
                RedirectStatus.Expired => Results.StatusCode(StatusCodes.Status410Gone),
                _ => Results.NotFound()
            };
        });
    }
}

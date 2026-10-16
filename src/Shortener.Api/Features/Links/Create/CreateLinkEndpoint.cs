using Microsoft.AspNetCore.Http;
using Shortener.Api.Infrastructure.Errors;

namespace Shortener.Api.Features.Links.Create;

public static class CreateLinkEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/links", async (
            CreateLinkRequest request,
            HttpRequest httpRequest,
            CreateLinkService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CreateAsync(
                request,
                httpRequest.Scheme,
                httpRequest.Host.ToString(),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return ApiErrors.Validation(result.Error!);
            }

            var response = new CreateLinkResponse(result.ShortUrl!, result.Code!);
            return Results.Created(result.ShortUrl, response);
        });
    }
}

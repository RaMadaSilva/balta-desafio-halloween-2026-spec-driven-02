using Microsoft.AspNetCore.Http;

namespace Shortener.Api.Features.Links.Create;

public static class CreateLinkEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/links", async (
            CreateLinkRequest request,
            HttpRequest httpRequest,
            CreateLinkHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                request.TargetUrl,
                httpRequest.Scheme,
                httpRequest.Host.ToString(),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Pedido inválido",
                    detail: result.Error);
            }

            var response = new CreateLinkResponse(result.ShortUrl!, result.Code!);
            return Results.Created(result.ShortUrl, response);
        });
    }
}

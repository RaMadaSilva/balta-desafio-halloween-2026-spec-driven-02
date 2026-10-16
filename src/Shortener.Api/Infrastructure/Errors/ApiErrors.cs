using Microsoft.AspNetCore.Http;

namespace Shortener.Api.Infrastructure.Errors;

public static class ApiErrors
{
    public static IResult Validation(string detail) =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Pedido inválido",
            detail: detail);
}

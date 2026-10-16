using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shortener.Api.Infrastructure.Errors;

namespace Shortener.Tests.Infrastructure.Errors;

public sealed class ApiErrorsTests
{
    [Fact]
    public void ValidationReturnsPortugueseBadRequestProblemDetails()
    {
        var result = Assert.IsType<ProblemHttpResult>(ApiErrors.Validation("URL inválida."));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Pedido inválido", result.ProblemDetails.Title);
        Assert.Equal("URL inválida.", result.ProblemDetails.Detail);
    }
}

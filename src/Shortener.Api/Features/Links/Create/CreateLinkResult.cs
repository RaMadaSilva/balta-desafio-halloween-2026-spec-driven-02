namespace Shortener.Api.Features.Links.Create;

public sealed record CreateLinkResult(string? Code, string? ShortUrl, string? Error)
{
    public bool IsSuccess => Error is null;
}

using Shortener.Api.Domain.Links;

namespace Shortener.Api.Features.Links.Create;

public sealed class CreateLinkHandler(
    ILinkRepository repository,
    IShortCodeGenerator codeGenerator,
    TimeProvider timeProvider)
{
    public async Task<CreateLinkResult> HandleAsync(
        string? targetUrl,
        string scheme,
        string host,
        CancellationToken cancellationToken = default)
    {
        if (TargetUrlValidator.Validate(targetUrl) is { } error)
        {
            return new CreateLinkResult(null, null, error);
        }

        while (true)
        {
            var code = codeGenerator.Generate();
            var link = new Link(code, targetUrl!, timeProvider.GetUtcNow());

            if (await repository.TryAddAsync(link, cancellationToken))
            {
                return new CreateLinkResult(code, $"{scheme}://{host}/{code}", null);
            }
        }
    }
}

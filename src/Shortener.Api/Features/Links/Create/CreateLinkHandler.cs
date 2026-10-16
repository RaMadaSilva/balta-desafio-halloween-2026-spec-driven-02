using Shortener.Api.Domain.Links;

namespace Shortener.Api.Features.Links.Create;

public sealed class CreateLinkHandler(
    ILinkRepository repository,
    IShortCodeGenerator codeGenerator,
    TimeProvider timeProvider)
{
    public async Task<CreateLinkResult> HandleAsync(
        CreateLinkRequest request,
        string scheme,
        string host,
        CancellationToken cancellationToken = default)
    {
        if (TargetUrlValidator.Validate(request.TargetUrl) is { } error)
        {
            return new CreateLinkResult(null, null, error);
        }

        if (request.Code is { } customCode)
        {
            if (CustomShortCodeValidator.Validate(customCode) is { } codeError)
            {
                return new CreateLinkResult(null, null, codeError);
            }

            var customLink = new Link(customCode, request.TargetUrl!, timeProvider.GetUtcNow());
            if (!await repository.TryAddAsync(customLink, cancellationToken))
            {
                return new CreateLinkResult(null, null, "O código personalizado já está em uso.");
            }

            return new CreateLinkResult(customCode, $"{scheme}://{host}/{customCode}", null);
        }

        while (true)
        {
            var code = codeGenerator.Generate();
            var link = new Link(code, request.TargetUrl!, timeProvider.GetUtcNow());

            if (await repository.TryAddAsync(link, cancellationToken))
            {
                return new CreateLinkResult(code, $"{scheme}://{host}/{code}", null);
            }
        }
    }
}

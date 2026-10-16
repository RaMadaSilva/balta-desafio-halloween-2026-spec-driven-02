namespace Shortener.Api.Features.Links.Create;

public static class TargetUrlValidator
{
    public const string ErrorMessage = "A URL de destino tem de ser absoluta, usar HTTP ou HTTPS e ter no máximo 2048 caracteres.";

    public static string? Validate(string? targetUrl)
    {
        if (string.IsNullOrWhiteSpace(targetUrl) || targetUrl.Length > 2048 ||
            !Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrEmpty(uri.Host))
        {
            return ErrorMessage;
        }

        return null;
    }
}

namespace Shortener.Api.Features.Links.Create;

internal static class ShortCodeRules
{
    internal const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";

    internal static bool ContainsOnlyAllowedCharacters(string code) =>
        code.All(AllowedCharacters.Contains);
}

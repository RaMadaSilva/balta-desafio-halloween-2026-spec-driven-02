namespace Shortener.Api.Features.Links.Create;

public static class CustomShortCodeValidator
{
    public const string ErrorMessage = "O código personalizado tem de ter entre 4 e 20 caracteres e usar apenas letras ASCII e números de 1 a 9.";

    public static string? Validate(string? code)
    {
        if (code is null || code.Length is < 4 or > 20 || !ShortCodeRules.ContainsOnlyAllowedCharacters(code))
        {
            return ErrorMessage;
        }

        return null;
    }
}

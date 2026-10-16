using System.Security.Cryptography;

namespace Shortener.Api.Features.Links.Create;

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    public string Generate()
    {
        var code = new char[7];

        for (var index = 0; index < code.Length; index++)
        {
            code[index] = ShortCodeRules.AllowedCharacters[RandomNumberGenerator.GetInt32(ShortCodeRules.AllowedCharacters.Length)];
        }

        return new string(code);
    }
}

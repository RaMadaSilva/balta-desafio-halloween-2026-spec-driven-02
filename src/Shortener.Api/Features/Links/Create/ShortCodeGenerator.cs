using System.Security.Cryptography;

namespace Shortener.Api.Features.Links.Create;

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";

    public string Generate()
    {
        var code = new char[7];

        for (var index = 0; index < code.Length; index++)
        {
            code[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(code);
    }
}

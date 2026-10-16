using System.Text.RegularExpressions;
using Shortener.Api.Features.Links.Create;

namespace Shortener.Tests.Features.Links.Create;

public sealed class ShortCodeGeneratorTests
{
    [Fact]
    public void GenerateReturnsSevenCharactersFromAllowedAlphabet()
    {
        var generator = new ShortCodeGenerator();

        var codes = Enumerable.Range(0, 100).Select(_ => generator.Generate());

        Assert.All(codes, code => Assert.Matches(new Regex("^[A-Za-z1-9]{7}$"), code));
    }
}

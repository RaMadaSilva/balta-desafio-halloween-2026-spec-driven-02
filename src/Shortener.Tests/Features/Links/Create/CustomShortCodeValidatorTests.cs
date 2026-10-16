using Shortener.Api.Features.Links.Create;

namespace Shortener.Tests.Features.Links.Create;

public sealed class CustomShortCodeValidatorTests
{
    [Theory]
    [InlineData("Ab12")]
    [InlineData("ABC1")]
    [InlineData("abc1")]
    [InlineData("A1b2C3d4E5f6G7h8J9k1")]
    public void ValidateAcceptsCodesWithinLengthAndAlphabet(string code)
    {
        Assert.Null(CustomShortCodeValidator.Validate(code));
    }

    [Theory]
    [InlineData("Ab1")]
    [InlineData("A1b2C3d4E5f6G7h8J9k12")]
    [InlineData("Code0")]
    [InlineData("has space")]
    [InlineData("code!")]
    [InlineData("código")]
    [InlineData("")]
    public void ValidateRejectsCodesOutsideLengthOrAlphabet(string code)
    {
        Assert.NotNull(CustomShortCodeValidator.Validate(code));
    }
}

using System.Globalization;
using System.Text.RegularExpressions;

namespace Shortener.Api.Features.Links.Create;

public static class ExpiryValidator
{
    private static readonly Regex UtcIso8601Pattern =
        new("^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}(?:\\.\\d{1,7})?Z$", RegexOptions.CultureInvariant);

    public const string ErrorMessage = "A expiração tem de ser um instante UTC ISO 8601 terminado em Z e posterior ao momento actual.";

    public static bool TryParse(
        string? value,
        DateTimeOffset nowUtc,
        out DateTimeOffset? expiresAtUtc,
        out string? error)
    {
        expiresAtUtc = null;
        error = null;

        if (value is null)
        {
            return true;
        }

        if (!UtcIso8601Pattern.IsMatch(value) ||
            !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ||
            parsed <= nowUtc)
        {
            error = ErrorMessage;
            return false;
        }

        expiresAtUtc = parsed.ToUniversalTime();
        return true;
    }
}

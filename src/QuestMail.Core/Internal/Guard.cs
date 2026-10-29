using System.Text.RegularExpressions;

namespace QuestMail.Core.Internal;

internal static partial class Guard
{
    public static void Defined<T>(T value, string name) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name, value, "Unknown enum value.");
    }

    public static void NonNegative(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(name, "Value must be finite and non-negative.");
    }

    public static void Positive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(name, "Value must be finite and greater than zero.");
    }

    public static void Color(string value, string name)
    {
        ArgumentNullException.ThrowIfNull(value, name);
        if (!ColorPattern().IsMatch(value))
            throw new ArgumentException("Use a hexadecimal or named CSS color.", name);
    }

    public static void Url(string value, string name, bool image = false)
    {
        ArgumentNullException.ThrowIfNull(value, name);
        if (value != value.Trim() || value.Any(char.IsControl) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException("An absolute URL without control characters is required.", name);

        var allowed = uri.Scheme is "https" or "http" ||
            (image ? uri.Scheme == "cid" : uri.Scheme is "mailto" or "tel");
        var hasTarget = uri.Scheme is "https" or "http"
            ? !string.IsNullOrWhiteSpace(uri.Host)
            : value.Length > uri.Scheme.Length + 1;
        if (!allowed || !hasTarget)
            throw new ArgumentException(image ? "Images require an HTTP, HTTPS or CID URL." :
                "Links require an HTTP, HTTPS, mailto or tel URL.", name);
    }

    public static string NormalizeText(string text, string name = "text")
    {
        ArgumentNullException.ThrowIfNull(text, name);
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    [GeneratedRegex(@"\A(?:#[0-9a-fA-F]{3,4}|#[0-9a-fA-F]{6}|#[0-9a-fA-F]{8}|[a-zA-Z]+)\z")]
    private static partial Regex ColorPattern();
}

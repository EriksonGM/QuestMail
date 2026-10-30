using System.Collections;

namespace QuestMail.Core.Styles;

/// <summary>Immutable, ordered visual CSS declarations with a restricted, URL-free value syntax.</summary>
public sealed class CssDeclarations : IReadOnlyDictionary<string, string>, IEquatable<CssDeclarations>
{
    private static readonly HashSet<string> AllowedProperties = new(StringComparer.Ordinal)
    {
        "background-color", "color", "font-family", "font-size", "font-style", "font-weight", "font-variant",
        "line-height", "letter-spacing", "word-spacing", "text-align", "text-decoration", "text-decoration-color",
        "text-decoration-line", "text-decoration-style", "text-indent", "text-transform", "text-shadow",
        "white-space", "word-break", "word-wrap", "overflow-wrap", "opacity", "box-shadow", "border-radius",
        "border-color", "border-style", "border-width",
        "border-top-color", "border-right-color", "border-bottom-color", "border-left-color",
        "border-top-style", "border-right-style", "border-bottom-style", "border-left-style",
        "border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
        "outline-color", "outline-style", "outline-width"
    };

    private readonly KeyValuePair<string, string>[] _declarations;

    private CssDeclarations(KeyValuePair<string, string>[] declarations) => _declarations = declarations;

    public static CssDeclarations Empty { get; } = new([]);
    public int Count => _declarations.Length;
    public IEnumerable<string> Keys => _declarations.Select(pair => pair.Key);
    public IEnumerable<string> Values => _declarations.Select(pair => pair.Value);
    public string this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);

    public static CssDeclarations Create(params (string Property, string Value)[] declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        var result = Empty;
        foreach (var (property, value) in declarations)
            result = result.Set(property, value);
        return result;
    }

    public static CssDeclarations Create(IEnumerable<KeyValuePair<string, string>> declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        var result = Empty;
        foreach (var pair in declarations)
            result = result.Set(pair.Key, pair.Value);
        return result;
    }

    /// <summary>Returns a new collection. A replacement moves to the end, preserving CSS cascade order.</summary>
    public CssDeclarations Set(string property, string value)
    {
        var name = NormalizeProperty(property);
        ValidateValue(value);
        return new CssDeclarations(_declarations.Where(pair => pair.Key != name)
            .Append(new KeyValuePair<string, string>(name, value.Trim())).ToArray());
    }

    /// <summary>Returns a new collection without this property; does not reset inherited desktop CSS on mobile.</summary>
    public CssDeclarations Remove(string property)
    {
        var name = NormalizeProperty(property);
        return Without(key => key == name);
    }

    public bool ContainsKey(string key) => TryGetValue(key, out _);

    public bool TryGetValue(string key, out string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        foreach (var pair in _declarations)
            if (string.Equals(pair.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        value = "";
        return false;
    }

    internal CssDeclarations Without(Func<string, bool> predicate) =>
        new(_declarations.Where(pair => !predicate(pair.Key)).ToArray());

    internal CssDeclarations Overlay(CssDeclarations? other)
    {
        var result = this;
        if (other is not null)
            foreach (var pair in other)
                result = result.Set(pair.Key, pair.Value);
        return result;
    }

    public bool Equals(CssDeclarations? other) => other is not null && _declarations.SequenceEqual(other._declarations);
    public override bool Equals(object? obj) => obj is CssDeclarations other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var pair in _declarations)
        {
            hash.Add(pair.Key, StringComparer.Ordinal);
            hash.Add(pair.Value, StringComparer.Ordinal);
        }
        return hash.ToHashCode();
    }

    public override string ToString() => string.Concat(_declarations.Select(pair => $"{pair.Key}:{pair.Value};"));
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() =>
        ((IEnumerable<KeyValuePair<string, string>>)_declarations).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string NormalizeProperty(string property)
    {
        ArgumentNullException.ThrowIfNull(property);
        var name = property.Trim().ToLowerInvariant();
        if (property.Any(char.IsControl) || !AllowedProperties.Contains(name))
            throw new ArgumentException($"CSS property '{property}' is not supported. Use typed styles for layout and visibility.", nameof(property));
        return name;
    }

    private static void ValidateValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512)
            throw new ArgumentException("CSS values must contain between 1 and 512 characters.", nameof(value));

        // Deliberately not a general CSS parser: accepting only these tokens excludes URLs, functions,
        // escapes, comments, !important, extra declarations and HTML/style-tag injection.
        var quote = '\0';
        foreach (var character in value)
        {
            if (!char.IsLetterOrDigit(character) && !" #.,%+-'\"".Contains(character))
                throw new ArgumentException("CSS values support only simple keywords, numbers, units, colors and quoted font names; no CSS functions or delimiters.", nameof(value));
            if (character is '\'' or '"')
            {
                if (quote == '\0') quote = character;
                else if (quote == character) quote = '\0';
            }
        }
        if (quote != '\0')
            throw new ArgumentException("CSS values must have balanced quotes.", nameof(value));
    }
}

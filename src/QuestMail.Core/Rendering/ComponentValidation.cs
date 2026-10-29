using System.Text.RegularExpressions;
using QuestMail.Core.Components;
using QuestMail.Core.Internal;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Rendering;

internal static partial class ComponentValidation
{
    public static void Validate(EmailComponent component, int depth = 0)
    {
        if (depth > 64)
            throw new ArgumentException("Component trees cannot exceed 64 levels of nesting.", nameof(component));
        ArgumentNullException.ThrowIfNull(component.Style);
        ValidateStyle(component.Style);
        if (component.MobileStyle is { } mobile)
            ValidateStyle(mobile);

        switch (component)
        {
            case Body body:
                Guard.Color(body.CanvasColor, nameof(body.CanvasColor));
                ArgumentNullException.ThrowIfNull(body.Language);
                if (!LanguagePattern().IsMatch(body.Language))
                    throw new ArgumentException("Use a language tag such as en or es-MX.", nameof(body.Language));
                break;
            case Row row:
                if (row.MobileColumns < 1)
                    throw new ArgumentOutOfRangeException(nameof(row.MobileColumns));
                Guard.NonNegative(row.Gap, nameof(row.Gap));
                break;
            case TextComponent text:
                foreach (var run in text.Content)
                {
                    if (run.Url is not null)
                        Guard.Url(run.Url, nameof(run.Url));
                    if (run.Typography is not null)
                        ValidateTypography(run.Typography);
                }
                break;
            case Image image:
                if (image.Height is { } height)
                    Guard.Positive(height, nameof(image.Height));
                if (image.LinkUrl is not null)
                    Guard.Url(image.LinkUrl, nameof(image.LinkUrl));
                break;
            case Frame or Line or Button or Link or Divider or Spacer:
                break;
            default:
                throw new NotSupportedException($"Component type {component.GetType().Name} is not supported.");
        }

        if (component is ContainerComponent container)
            foreach (var child in container.Children)
                Validate(child, depth + 1);
    }

    private static void ValidateStyle(ComponentStyle style)
    {
        if (style.MaxWidth is { } maxWidth)
            Guard.Positive(maxWidth, nameof(style.MaxWidth));
        if (style.Height is { } height)
            Guard.NonNegative(height, nameof(style.Height));
        if (style.BorderRadius is { } radius)
            Guard.NonNegative(radius, nameof(style.BorderRadius));
        if (style.BackgroundColor is { } color)
            Guard.Color(color, nameof(style.BackgroundColor));
        if (style.HorizontalAlignment is { } horizontal)
            Guard.Defined(horizontal, nameof(style.HorizontalAlignment));
        if (style.VerticalAlignment is { } vertical)
            Guard.Defined(vertical, nameof(style.VerticalAlignment));
        if (style.Typography is { } typography)
            ValidateTypography(typography);
    }

    private static void ValidateTypography(TypographyStyle typography)
    {
        if (typography.FontFamily is { } family && !FontFamilyPattern().IsMatch(family))
            throw new ArgumentException("Font families may contain names, spaces, commas, quotes and hyphens.", nameof(typography.FontFamily));
        if (typography.FontSize is { } size)
            Guard.Positive(size, nameof(typography.FontSize));
        if (typography.FontWeight is { } weight && (weight < 1 || weight > 1000))
            throw new ArgumentOutOfRangeException(nameof(typography.FontWeight));
        if (typography.Color is { } color)
            Guard.Color(color, nameof(typography.Color));
        if (typography.LineHeight is { } lineHeight)
            Guard.Positive(lineHeight, nameof(typography.LineHeight));
        if (typography.LetterSpacing is { } spacing && !double.IsFinite(spacing))
            throw new ArgumentOutOfRangeException(nameof(typography.LetterSpacing));
        if (typography.TextAlignment is { } alignment)
            Guard.Defined(alignment, nameof(typography.TextAlignment));
        if (typography.Decoration is { } decoration)
            Guard.Defined(decoration, nameof(typography.Decoration));
        if (typography.WhiteSpace is { } whiteSpace)
            Guard.Defined(whiteSpace, nameof(typography.WhiteSpace));
    }

    [GeneratedRegex(@"\A[a-zA-Z]{2,8}(?:-[a-zA-Z0-9]{1,8})*\z")]
    private static partial Regex LanguagePattern();

    [GeneratedRegex("\\A[\\p{L}\\p{N} ,\"'\\-]+\\z")]
    private static partial Regex FontFamilyPattern();
}

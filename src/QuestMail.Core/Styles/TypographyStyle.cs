using QuestMail.Core.Enums;

namespace QuestMail.Core.Styles;

/// <summary>Unset properties inherit from the parent component.</summary>
public sealed record TypographyStyle
{
    public string? FontFamily { get; init; }
    public double? FontSize { get; init; }
    public int? FontWeight { get; init; }
    public string? Color { get; init; }
    public double? LineHeight { get; init; }
    public double? LetterSpacing { get; init; }
    public ElementAlignEnum? TextAlignment { get; init; }
    public bool? Italic { get; init; }
    public TextDecorationEnum? Decoration { get; init; }
    public WhiteSpaceEnum? WhiteSpace { get; init; }
    public bool? WordWrap { get; init; }
}

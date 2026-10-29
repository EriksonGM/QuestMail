using QuestMail.Core.Enums;

namespace QuestMail.Core.Styles;

/// <summary>Reusable desktop styles. MobileStyle overrides only the properties it sets.</summary>
public sealed record ComponentStyle
{
    public EmailWidth? Width { get; init; }
    public double? MaxWidth { get; init; }
    public double? Height { get; init; }
    public Spacing? Margin { get; init; }
    public Spacing? Padding { get; init; }
    public string? BackgroundColor { get; init; }
    public EmailBorder? Border { get; init; }
    public double? BorderRadius { get; init; }
    public ElementAlignEnum? HorizontalAlignment { get; init; }
    public VerticalAlignEnum? VerticalAlignment { get; init; }
    public TypographyStyle? Typography { get; init; }
    public bool? Visible { get; init; }
}

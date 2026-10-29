using QuestMail.Core.Internal;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Components;

/// <summary>An encoded text fragment, optionally styled and linked.</summary>
public sealed class TextRun
{
    public string Text { get; }
    public string? Url { get; init; }
    public TypographyStyle? Typography { get; init; }

    public TextRun(string text) => Text = Guard.NormalizeText(text);
}

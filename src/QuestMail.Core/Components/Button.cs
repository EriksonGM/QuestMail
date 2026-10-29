using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

/// <summary>An email CTA rendered as a styled link, never an HTML button.</summary>
public sealed class Button : EmailComponent
{
    public string Text { get; }
    public string Url { get; }
    public string? Title { get; init; }
    public bool OpenInNewWindow { get; init; }

    public Button(string text, string url)
    {
        Text = Guard.NormalizeText(text);
        Guard.Url(url, nameof(url));
        Url = url;
    }
}

using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

public sealed class Link : EmailComponent
{
    public string Text { get; }
    public string Url { get; }
    public string? Title { get; init; }
    public bool OpenInNewWindow { get; init; }

    public Link(string text, string url)
    {
        Text = Guard.NormalizeText(text);
        Guard.Url(url, nameof(url));
        Url = url;
    }
}

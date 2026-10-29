using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

public sealed class Image : EmailComponent
{
    public string SourceUrl { get; }
    public string AltText { get; }
    public double? Height { get; init; }
    public string? LinkUrl { get; init; }

    public Image(string sourceUrl, string altText)
    {
        Guard.Url(sourceUrl, nameof(sourceUrl), image: true);
        ArgumentNullException.ThrowIfNull(altText);
        SourceUrl = sourceUrl;
        AltText = altText;
    }
}

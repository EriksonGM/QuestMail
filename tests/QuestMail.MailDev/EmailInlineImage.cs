namespace QuestMail.MailDev;

/// <summary>A PNG embedded in the preview assembly, addressed from HTML through a stable Content-ID.</summary>
public sealed record EmailInlineImage(string Name)
{
    public string ContentId => $"{Name}@questmail.test";
    public string ResourceName => $"QuestMail.PreviewAssets.{Name}.png";
    public string FileName => $"{Name}.png";
    public string Source => $"cid:{ContentId}";
}

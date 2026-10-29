namespace QuestMail.Core;

/// <summary>An immutable snapshot of the HTML and plain-text email bodies.</summary>
public sealed record EmailContent(string Html, string Plain);

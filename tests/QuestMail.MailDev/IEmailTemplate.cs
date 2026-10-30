using QuestMail.Core.Components;

namespace QuestMail.MailDev;

public interface IEmailTemplate
{
    string Name { get; }
    string Subject { get; }
    Body CreateBody();
    IReadOnlyList<EmailInlineImage> InlineImages => Array.Empty<EmailInlineImage>();
}

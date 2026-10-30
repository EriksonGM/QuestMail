using MimeKit;
using MimeKit.Utils;
using QuestMail.Core;

namespace QuestMail.MailDev;

public static class EmailMessageFactory
{
    public static MimeMessage Create(IEmailTemplate template, MailDevSettings settings)
    {
        var content = new EmailBuilder(template.CreateBody()).Build();
        var message = new MimeMessage
        {
            MessageId = MimeUtils.GenerateMessageId(),
            Date = DateTimeOffset.UtcNow,
            Subject = $"[QuestMail preview] {template.Subject}",
            Body = new BodyBuilder { TextBody = content.Plain, HtmlBody = content.Html }.ToMessageBody()
        };
        message.From.Add(settings.From);
        message.To.Add(settings.To);
        message.Headers.Add("X-QuestMail-Template", template.Name);
        return message;
    }
}

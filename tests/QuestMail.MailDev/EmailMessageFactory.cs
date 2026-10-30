using MimeKit;
using MimeKit.Utils;
using QuestMail.Core;

namespace QuestMail.MailDev;

public static class EmailMessageFactory
{
    public static MimeMessage Create(IEmailTemplate template, MailDevSettings settings)
    {
        var content = new EmailBuilder(template.CreateBody()).Build();
        var builder = new BodyBuilder { TextBody = content.Plain, HtmlBody = content.Html };
        var contentIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var image in template.InlineImages)
        {
            if (!contentIds.Add(image.ContentId))
                throw new InvalidOperationException($"Duplicate inline image Content-ID: {image.ContentId}.");
            using var resource = typeof(IEmailTemplate).Assembly.GetManifestResourceStream(image.ResourceName)
                ?? throw new InvalidOperationException($"Embedded preview image '{image.ResourceName}' was not found.");
            using var buffer = new MemoryStream();
            resource.CopyTo(buffer);
            var part = builder.LinkedResources.Add(image.FileName, buffer.ToArray(), new ContentType("image", "png"));
            part.ContentId = image.ContentId;
            part.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        }
        var message = new MimeMessage
        {
            MessageId = MimeUtils.GenerateMessageId(),
            Date = DateTimeOffset.UtcNow,
            Subject = $"[QuestMail preview] {template.Subject}",
            Body = builder.ToMessageBody()
        };
        message.From.Add(settings.From);
        message.To.Add(settings.To);
        message.Headers.Add("X-QuestMail-Template", template.Name);
        return message;
    }
}

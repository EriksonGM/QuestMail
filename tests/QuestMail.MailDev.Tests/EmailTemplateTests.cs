using MimeKit;
using QuestMail.Core;

namespace QuestMail.MailDev.Tests;

public class EmailTemplateTests
{
    [Fact]
    public void CatalogDiscoversTemplatesInNameOrderWithUniqueNames()
    {
        var templates = EmailTemplateCatalog.Discover();

        Assert.True(templates.Count >= 18);
        Assert.Equal(templates.Select(template => template.Name).Order(StringComparer.Ordinal), templates.Select(template => template.Name));
        Assert.Equal(templates.Count, templates.Select(template => template.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        foreach (var name in new[] { "welcome", "password-reset", "order-confirmation", "newsletter", "security-alert", "custom-css" })
            Assert.Contains(templates, template => template.Name == name);
    }

    [Fact]
    public void EveryTemplateProducesRepeatableHtmlAndPlainTextAlternatives()
    {
        var settings = MailDevSettings.FromEnvironment(_ => null);
        foreach (var template in EmailTemplateCatalog.Discover())
        {
            var content = new EmailBuilder(template.CreateBody()).Build();
            Assert.False(string.IsNullOrWhiteSpace(content.Html));
            Assert.False(string.IsNullOrWhiteSpace(content.Plain));
            Assert.Contains("@media", content.Html);
            Assert.Equal(content, new EmailBuilder(template.CreateBody()).Build());

            using var message = EmailMessageFactory.Create(template, settings);
            var alternatives = Assert.IsType<MultipartAlternative>(message.Body);
            Assert.Equal(2, alternatives.Count);
            Assert.Equal("text/plain", alternatives[0].ContentType.MimeType);
            if (template.InlineImages.Count == 0)
                Assert.Equal("text/html", alternatives[1].ContentType.MimeType);
            else
            {
                var related = Assert.IsType<MultipartRelated>(alternatives[1]);
                Assert.Equal("text/html", related[0].ContentType.MimeType);
                Assert.Equal(template.InlineImages.Count + 1, related.Count);
            }
            Assert.Equal(content.Html.ReplaceLineEndings("\n"), Assert.IsType<string>(message.HtmlBody).ReplaceLineEndings("\n"));
            Assert.Equal(content.Plain.ReplaceLineEndings("\n"), Assert.IsType<string>(message.TextBody).ReplaceLineEndings("\n"));
            Assert.Equal(settings.From, Assert.Single(message.From));
            Assert.Equal(settings.To, Assert.Single(message.To));
            Assert.Equal($"[QuestMail preview] {template.Subject}", message.Subject);
            Assert.Equal(template.Name, message.Headers["X-QuestMail-Template"]);
            Assert.False(string.IsNullOrWhiteSpace(message.MessageId));
        }
    }

    [Fact]
    public void MimeSerializationPreservesBothBodiesAndUnicode()
    {
        var settings = MailDevSettings.FromEnvironment(_ => null);
        foreach (var template in EmailTemplateCatalog.Discover())
        {
            using var message = EmailMessageFactory.Create(template, settings);
            using var stream = new MemoryStream();
            message.WriteTo(stream, cancellationToken: TestContext.Current.CancellationToken);
            stream.Position = 0;
            using var restored = MimeMessage.Load(stream, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(message.Subject, restored.Subject);
            Assert.Equal(Assert.IsType<string>(message.HtmlBody).ReplaceLineEndings("\n"),
                Assert.IsType<string>(restored.HtmlBody).ReplaceLineEndings("\n"));
            Assert.Equal(Assert.IsType<string>(message.TextBody).ReplaceLineEndings("\n"),
                Assert.IsType<string>(restored.TextBody).ReplaceLineEndings("\n"));
        }
    }

    [Fact]
    public void WelcomePreservesUnicodeAndEscapesHtml()
    {
        var template = Assert.Single(EmailTemplateCatalog.Select("welcome"));
        var content = new EmailBuilder(template.CreateBody()).Build();

        Assert.Contains("Olá, Ada & equipo!", content.Plain);
        Assert.Contains("Ada &amp; equipo!", content.Html);
    }

    [Fact]
    public void SelectionIsCaseInsensitiveAndUnknownNamesFail()
    {
        Assert.Equal("welcome", Assert.Single(EmailTemplateCatalog.Select("WELCOME")).Name);
        Assert.Equal(EmailTemplateCatalog.Discover().Count, EmailTemplateCatalog.Select(null).Count);
        Assert.Throws<ArgumentException>(() => EmailTemplateCatalog.Select("missing-template"));
    }
}

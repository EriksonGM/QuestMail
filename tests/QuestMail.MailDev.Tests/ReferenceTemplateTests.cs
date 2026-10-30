using System.Xml.Linq;
using MimeKit;
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.MailDev.Templates;

namespace QuestMail.MailDev.Tests;

public class ReferenceTemplateTests
{
    [Theory]
    [InlineData("carepair-password-reset", "Hi Olivia,")]
    [InlineData("release-announcement", "Smart Distribute")]
    [InlineData("mintify-order-confirmation", "Thank you for order!")]
    [InlineData("aattio-order-confirmation", "Your Order is Confirmed!")]
    [InlineData("plugin-products-newsletter", "Our Products")]
    [InlineData("strac-shipping-update", "Your shipment is")]
    [InlineData("britflow-welcome", "Wahoo! You're In")]
    [InlineData("fashion-warehouse-sale", "80% off")]
    [InlineData("sara-summer-sale", "Summer Sale Started!")]
    [InlineData("mintify-app-welcome", "Hi Nizam,")]
    [InlineData("koota-app-welcome", "You're Welcome!")]
    [InlineData("mintify-cloud-registration", "You've registered")]
    public void ReferenceTemplatesHaveExpectedContentAndOnlyLocalImageSources(string name, string expected)
    {
        var template = Assert.Single(EmailTemplateCatalog.Select(name));
        var content = new EmailBuilder(template.CreateBody()).Build();
        var document = XDocument.Parse(content.Html);

        Assert.Contains(expected, content.Plain);
        Assert.Contains("@media", content.Html);
        foreach (var image in document.Descendants("img"))
        {
            Assert.StartsWith("cid:", (string?)image.Attribute("src"));
            Assert.False(string.IsNullOrWhiteSpace((string?)image.Attribute("alt")));
        }
        Assert.Contains(document.Descendants("a"), link => ((string?)link.Attribute("href"))?.StartsWith("https://example.com/", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void EveryInlineImageReferenceHasOneEmbeddedPngWithMatchingContentId()
    {
        var settings = MailDevSettings.FromEnvironment(_ => null);
        foreach (var template in EmailTemplateCatalog.Discover())
        {
            using var message = EmailMessageFactory.Create(template, settings);
            var document = XDocument.Parse(Assert.IsType<string>(message.HtmlBody));
            var sources = document.Descendants("img").Select(image => (string)image.Attribute("src")!)
                .Where(source => source.StartsWith("cid:", StringComparison.Ordinal)).Select(source => source[4..]).ToHashSet();
            var images = message.BodyParts.OfType<MimePart>().Where(part => part.ContentType.MediaType == "image").ToArray();

            Assert.Equal(template.InlineImages.Count, images.Length);
            Assert.Equal(sources.Count, images.Length);
            Assert.Empty(message.Attachments);
            foreach (var image in images)
            {
                Assert.Equal("image/png", image.ContentType.MimeType);
                Assert.Contains(Assert.IsType<string>(image.ContentId), sources);
                Assert.Equal(ContentDisposition.Inline, image.ContentDisposition?.Disposition);
                using var stream = new MemoryStream();
                var payload = image.Content;
                Assert.NotNull(payload);
                payload.DecodeTo(stream, TestContext.Current.CancellationToken);
                Assert.True(stream.Length > 8);
                Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, stream.ToArray()[..8]);
            }
        }
    }

    [Fact]
    public void CarePairMatchesReferenceStructureAndKeepsAnAccessiblePasswordResetLink()
    {
        var template = new CarePairPasswordResetTemplate();
        var content = new EmailBuilder(template.CreateBody()).Build();
        var document = XDocument.Parse(content.Html);
        var action = Assert.Single(document.Descendants("a"), link => (string?)link.Attribute("href") == CarePairPasswordResetTemplate.ResetUrl);

        Assert.Contains("background-color:#2460e9;", content.Html);
        Assert.Contains("border-radius:22px;", content.Html);
        Assert.Contains("border-radius:22px 22px 0 0;", content.Html);
        Assert.Contains("background-color:#f6f6f6;", content.Html);
        Assert.Equal("Reset password", (string?)Assert.Single(action.Descendants("img")).Attribute("alt"));
        Assert.Contains("Need to reset password? No problem.", content.Plain);
        Assert.Contains("support@cairpair.com", content.Plain);
        Assert.Contains("© 2024 CarePair", content.Plain);
        Assert.Contains(CarePairPasswordResetTemplate.ResetUrl, content.Plain);
        Assert.Equal(6, template.InlineImages.Count);
    }

    [Fact]
    public void MissingAndDuplicateEmbeddedAssetsFailClearly()
    {
        var settings = MailDevSettings.FromEnvironment(_ => null);
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() =>
            EmailMessageFactory.Create(new AssetTestTemplate([new EmailInlineImage("missing-asset")]), settings)).Message);
        Assert.Contains("Duplicate", Assert.Throws<InvalidOperationException>(() =>
            EmailMessageFactory.Create(new AssetTestTemplate([new EmailInlineImage("rocket"), new EmailInlineImage("rocket")]), settings)).Message);
    }

    private sealed class AssetTestTemplate(IReadOnlyList<EmailInlineImage> images) : IEmailTemplate
    {
        public string Name => "asset-test";
        public string Subject => "Asset test";
        public IReadOnlyList<EmailInlineImage> InlineImages => images;
        public Body CreateBody() => new(new Paragraph("Test"));
    }
}

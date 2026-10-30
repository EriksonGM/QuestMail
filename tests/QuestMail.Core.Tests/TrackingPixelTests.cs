using System.Xml.Linq;
using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class TrackingPixelTests
{
    private const string TrackingUrl = "https://tracking.example.com/open.gif?message=sample-123&campaign=welcome";

    [Fact]
    public void TrackingIsDisabledByDefaultAndNullDoesNotChangeTheOutput()
    {
        var body = new Body(new Paragraph("Hello"));
        var original = new EmailBuilder(body).Build();
        var disabled = new EmailBuilder(body.WithTrackingPixel(null)).Build();

        Assert.Null(body.TrackingPixelUrl);
        Assert.Equal(original, disabled);
        Assert.DoesNotContain("qm-tracking-pixel", original.Html);
        Assert.Empty(XDocument.Parse(original.Html).Descendants("img"));
    }

    [Fact]
    public void TrackingPixelIsOneByOneDecorativeOutsideTheContentAndUrlIsEncoded()
    {
        var body = new Body(new Paragraph("Hello")) { TrackingPixelUrl = TrackingUrl };
        var email = new EmailBuilder(body).Build();
        var document = XDocument.Parse(email.Html);
        var pixel = Assert.Single(document.Descendants("img"));

        Assert.Equal(TrackingUrl, (string?)pixel.Attribute("src"));
        Assert.Equal("1", (string?)pixel.Attribute("width"));
        Assert.Equal("1", (string?)pixel.Attribute("height"));
        Assert.Equal("", (string?)pixel.Attribute("alt"));
        Assert.Equal("presentation", (string?)pixel.Attribute("role"));
        Assert.Equal("true", (string?)pixel.Attribute("aria-hidden"));
        Assert.Equal("qm-tracking-pixel", (string?)pixel.Attribute("class"));
        Assert.Equal("body", pixel.Parent?.Name.LocalName);
        Assert.Same(pixel, document.Descendants("body").Single().Elements().Last());
        Assert.Contains("position:absolute;", (string?)pixel.Attribute("style"));
        Assert.Contains("opacity:0;", (string?)pixel.Attribute("style"));
        Assert.Contains("width:1px;height:1px;", (string?)pixel.Attribute("style"));
        Assert.DoesNotContain("display:none", (string?)pixel.Attribute("style"));
        Assert.Null(pixel.Attribute("loading"));
        Assert.Contains("message=sample-123&amp;campaign=welcome", email.Html);
        Assert.Equal($"Hello{Environment.NewLine}", email.Plain);
        Assert.DoesNotContain(TrackingUrl, email.Plain);
    }

    [Fact]
    public void TrackingDoesNotChangeContentMarkupClassIdsOrPlainText()
    {
        var body = new Body().AddRow(row => row.AddParagraph("First").AddParagraph("Second"))
            .WithMobileStyle(new ComponentStyle { Padding = new Spacing(12) });
        var baseline = new EmailBuilder(body).Build();
        var tracked = new EmailBuilder(body.WithTrackingPixel(TrackingUrl)).Build();
        var document = XDocument.Parse(tracked.Html);
        Assert.Single(document.Descendants("img"), IsPixel).Remove();

        Assert.Equal(XDocument.Parse(baseline.Html).ToString(SaveOptions.DisableFormatting),
            document.ToString(SaveOptions.DisableFormatting));
        Assert.Equal(baseline.Plain, tracked.Plain);
    }

    [Fact]
    public void TrackingPixelSurvivesEveryFluentBodyCopyOperation()
    {
        var body = new Body().WithTrackingPixel(TrackingUrl);
        Body[] copies =
        [
            body.AddParagraph("Text"),
            body.Add(new Paragraph("Text")),
            body.AddFrame(frame => frame.AddParagraph("Text")),
            body.AddRow(row => row.AddParagraph("Text")),
            body.WithStyle(new ComponentStyle { Padding = new Spacing(12) }),
            body.WithMobileStyle(new ComponentStyle { Margin = new Spacing(0) }),
            body.WithMobileStyle(null),
            body.WithLanguage("pt-BR"),
            body.WithCanvasColor("#eeeeee")
        ];

        foreach (var copy in copies)
        {
            Assert.Equal(TrackingUrl, copy.TrackingPixelUrl);
            Assert.Equal(TrackingUrl, (string?)Assert.Single(XDocument.Parse(new EmailBuilder(copy).Build().Html)
                .Descendants("img"), IsPixel).Attribute("src"));
        }
        Assert.Empty(body.Children);
    }

    [Fact]
    public void FluentConfigurationIsImmutableAndCanReplaceOrDisableThePixel()
    {
        var original = new Body(new Paragraph("Text"))
        {
            Language = "pt-BR", CanvasColor = "#eeeeee",
            Style = new ComponentStyle { Margin = new Spacing(12) },
            MobileStyle = new ComponentStyle { Margin = new Spacing(0) }
        };
        var tracked = original.WithTrackingPixel(TrackingUrl);
        var replaced = tracked.WithTrackingPixel("https://tracking.example.com/other.gif");
        var disabled = tracked.WithTrackingPixel(null);

        Assert.Null(original.TrackingPixelUrl);
        Assert.Equal(TrackingUrl, tracked.TrackingPixelUrl);
        Assert.Equal("https://tracking.example.com/other.gif", replaced.TrackingPixelUrl);
        Assert.Equal("pt-BR", tracked.Language);
        Assert.Equal("#eeeeee", tracked.CanvasColor);
        Assert.Same(original.Style, tracked.Style);
        Assert.Same(original.MobileStyle, tracked.MobileStyle);
        Assert.Same(original.Children[0], tracked.Children[0]);
        Assert.Equal(new EmailBuilder(original).Build(), new EmailBuilder(disabled).Build());
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).WithTrackingPixel(TrackingUrl));
    }

    [Theory]
    [InlineData("https://tracking.example.com/open.gif")]
    [InlineData("http://127.0.0.1:8080/open?message=test")]
    [InlineData("https://tracking.example.com/open?message=123&kind=welcome")]
    public void AbsoluteHttpAndHttpsUrlsAreAccepted(string url)
    {
        var email = new EmailBuilder(new Body { TrackingPixelUrl = url }).Build();

        Assert.Equal(url, (string?)Assert.Single(XDocument.Parse(email.Html).Descendants("img")).Attribute("src"));
        Assert.Empty(email.Plain);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/open.gif")]
    [InlineData("//tracking.example.com/open.gif")]
    [InlineData("open.gif")]
    [InlineData("https://")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP")]
    [InlineData("cid:pixel@questmail.test")]
    [InlineData("mailto:hello@example.com")]
    [InlineData("tel:+12025550142")]
    [InlineData("ftp://example.com/open.gif")]
    [InlineData("file:///C:/open.gif")]
    [InlineData(" https://tracking.example.com/open.gif")]
    [InlineData("https://tracking.example.com/open.gif ")]
    [InlineData("https://tracking.example.com/open.gif\r\n")]
    [InlineData("https://tracking.example.com/op\ten.gif")]
    public void InvalidTrackingUrlsAreRejectedByBothApis(string url)
    {
        Assert.Equal("TrackingPixelUrl", Assert.Throws<ArgumentException>(() =>
            new EmailBuilder(new Body { TrackingPixelUrl = url })).ParamName);
        Assert.Throws<ArgumentException>(() => new Body().WithTrackingPixel(url));
    }

    [Fact]
    public void QuotesAndQueryParametersCannotInjectHtmlAttributes()
    {
        const string url = "https://tracking.example.com/open?label=\"quoted\"&value=<sample>";
        var email = new EmailBuilder(new Body().WithTrackingPixel(url)).Build();
        var pixel = Assert.Single(XDocument.Parse(email.Html).Descendants("img"));

        Assert.Equal(url, (string?)pixel.Attribute("src"));
        Assert.Contains("&quot;quoted&quot;", email.Html);
        Assert.Contains("&lt;sample&gt;", email.Html);
        Assert.Null(pixel.Attribute("onload"));
        Assert.Null(pixel.Attribute("onerror"));
    }

    [Fact]
    public void RepeatedBuildsEmitExactlyOnePixelAndDoNotPerformNetworkRequests()
    {
        // Port 1 is not an image endpoint. Rendering must serialize the URL, never fetch it.
        var builder = new EmailBuilder(new Body { TrackingPixelUrl = "http://127.0.0.1:1/not-fetched.gif" })
            .AddParagraph("Text");
        var first = builder.Build();
        var second = builder.Build();

        Assert.Equal(first, second);
        Assert.Single(XDocument.Parse(first.Html).Descendants("img"), IsPixel);
    }

    private static bool IsPixel(XElement element) => (string?)element.Attribute("class") == "qm-tracking-pixel";
}

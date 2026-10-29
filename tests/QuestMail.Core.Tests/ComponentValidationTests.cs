using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class ComponentValidationTests
{
    [Theory]
    [InlineData("https://example.com/path?a=1&b=2")]
    [InlineData("http://example.com")]
    [InlineData("mailto:support@example.com")]
    [InlineData("tel:+123456789")]
    public void SafeLinkSchemesAreAccepted(string url)
    {
        var email = new EmailBuilder().Add(new Link("Link", url)).Add(new Button("Button", url)).Build();

        Assert.Contains(url, email.Plain);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("file:///C:/secret")]
    [InlineData("//example.com")]
    [InlineData("/relative")]
    [InlineData("")]
    [InlineData(" https://example.com")]
    [InlineData("https://example.com\r\nInjected")]
    public void UnsafeLinksAreRejected(string url)
    {
        Assert.Throws<ArgumentException>(() => new Link("Link", url));
        Assert.Throws<ArgumentException>(() => new Button("Button", url));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/svg+xml,<svg onload='alert(1)'/>")]
    [InlineData("file:///C:/image.png")]
    [InlineData("mailto:support@example.com")]
    [InlineData("/image.png")]
    public void UnsafeImageSourcesAreRejected(string source)
    {
        Assert.Throws<ArgumentException>(() => new Image(source, "Image"));
    }

    [Fact]
    public void OptionalUrlsAreValidatedBeforeChangingTheBuilder()
    {
        var builder = new EmailBuilder().AddParagraph("Existing");
        var before = builder.Build();

        Assert.Throws<ArgumentException>(() => builder.Add(new Image("cid:logo", "Logo") { LinkUrl = "javascript:alert(1)" }));
        Assert.Throws<ArgumentException>(() => builder.Add(new Paragraph(new[] { new TextRun("Unsafe") { Url = "javascript:alert(1)" } })));
        Assert.Equal(before, builder.Build());
    }

    [Fact]
    public void TextAndAttributeInjectionRemainLiteralContent()
    {
        const string payload = "\"><img src=x onerror=alert(1)>";
        var email = new EmailBuilder().Add(new Heading(payload))
            .Add(new Paragraph(payload)).Add(new Button(payload, "https://example.com") { Title = payload })
            .Add(new Image("https://example.com/logo.png", payload)).Build();

        Assert.DoesNotContain("<img src=x", email.Html);
        Assert.Contains("&lt;img src=x onerror=alert(1)&gt;", email.Html);
        Assert.Contains(payload, email.Plain);
    }

    [Theory]
    [InlineData("red;background-image:url(https://attacker.example)")]
    [InlineData("#fff\" onclick=\"alert(1)")]
    [InlineData("</style><script>alert(1)</script>")]
    [InlineData("")]
    public void CssInjectionIsRejectedInColors(string color)
    {
        Assert.Throws<ArgumentException>(() => new EmailBorder(1, color));
        Assert.Throws<ArgumentException>(() => new Divider(color));
        Assert.Throws<ArgumentException>(() => new EmailBuilder().Add(new Frame
        {
            Style = new ComponentStyle { BackgroundColor = color }
        }));
        Assert.Throws<ArgumentException>(() => new EmailBuilder().Add(new Paragraph("Text")
        {
            MobileStyle = new ComponentStyle { Typography = new TypographyStyle { Color = color } }
        }));
    }

    [Theory]
    [InlineData("Arial;display:none")]
    [InlineData("</style><script>alert(1)</script>")]
    [InlineData("url(https://example.com)")]
    public void CssInjectionIsRejectedInFontFamilies(string family)
    {
        Assert.Throws<ArgumentException>(() => new EmailBuilder().Add(new Paragraph("Text")
        {
            Style = new ComponentStyle { Typography = new TypographyStyle { FontFamily = family } }
        }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidDimensionsAndSpacingAreRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Spacing(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Fixed(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Max(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Percent(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Spacer(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Divider(thickness: value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBorder(value, "#ffffff"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBuilder().Add(new Image("cid:logo", "Logo") { Height = value }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBuilder().Add(new Row { Gap = value }));
    }

    [Fact]
    public void ZeroWidthsInvalidPercentagesAndHeadingLevelsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Fixed(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Max(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmailWidth.Percent(101));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Heading("Text", (HeadingLevelEnum)4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBuilder().Add(new Row { MobileColumns = 0 }));
    }

    [Fact]
    public void InvalidCommonAndTypographyStylesAreRejected()
    {
        ComponentStyle[] styles =
        [
            new() { MaxWidth = 0 }, new() { BorderRadius = -1 }, new() { Height = -1 },
            new() { HorizontalAlignment = (ElementAlignEnum)99 }, new() { VerticalAlignment = (VerticalAlignEnum)99 },
            new() { Typography = new TypographyStyle { FontSize = 0 } },
            new() { Typography = new TypographyStyle { FontWeight = 1001 } },
            new() { Typography = new TypographyStyle { LineHeight = -1 } },
            new() { Typography = new TypographyStyle { LetterSpacing = double.NaN } },
            new() { Typography = new TypographyStyle { TextAlignment = (ElementAlignEnum)99 } },
            new() { Typography = new TypographyStyle { Decoration = (TextDecorationEnum)99 } },
            new() { Typography = new TypographyStyle { WhiteSpace = (WhiteSpaceEnum)99 } }
        ];

        foreach (var style in styles)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBuilder().Add(new Paragraph("Text") { Style = style }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EmailBuilder().Add(new Paragraph("Text") { MobileStyle = style }));
        }
    }

    [Fact]
    public void InvalidDescendantsAreRejectedAtomically()
    {
        var builder = new EmailBuilder().AddParagraph("Existing");
        var before = builder.Build();

        Assert.Throws<ArgumentException>(() => builder.Add(new Frame(new Row(new Paragraph("Invalid")
        {
            Style = new ComponentStyle { BackgroundColor = "red;display:none" }
        }))));
        Assert.Equal(before, builder.Build());
    }

    [Fact]
    public void ComponentAndTextCollectionsAreDefensiveReadOnlySnapshots()
    {
        EmailComponent[] children = [new Paragraph("Original")];
        TextRun[] runs = [new TextRun("Original run")];
        var frame = new Frame(children);
        var paragraph = new Paragraph(runs);
        children[0] = new Paragraph("Replaced");
        runs[0] = new TextRun("Replaced run");

        Assert.Equal("Original", Assert.IsType<Paragraph>(frame.Children[0]).Text);
        Assert.Equal("Original run", paragraph.Text);
        Assert.Throws<NotSupportedException>(() => ((IList<EmailComponent>)frame.Children)[0] = new Paragraph("Mutation"));
        Assert.Throws<NotSupportedException>(() => ((IList<TextRun>)paragraph.Content)[0] = new TextRun("Mutation"));
    }

    [Fact]
    public void NullComponentsCollectionsAndTextAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new EmailBuilder((Body)null!));
        Assert.Throws<ArgumentNullException>(() => new EmailBuilder().Add((EmailComponent)null!));
        Assert.Throws<ArgumentNullException>(() => new Frame((IEnumerable<EmailComponent>)null!));
        Assert.Throws<ArgumentException>(() => new Row(new EmailComponent[] { null! }));
        Assert.Throws<ArgumentNullException>(() => new Heading((string)null!));
        Assert.Throws<ArgumentNullException>(() => new Paragraph((string)null!));
        Assert.Throws<ArgumentException>(() => new Paragraph(new TextRun[] { null! }));
        Assert.Throws<ArgumentNullException>(() => new Image("cid:logo", null!));
        Assert.Throws<ArgumentNullException>(() => new Link("Text", null!));
        Assert.Throws<ArgumentNullException>(() => new EmailBuilder().Add(new Frame { Style = null! }));
    }

    [Fact]
    public void BodyCanOnlyAppearAtTheRoot()
    {
        Assert.Throws<ArgumentException>(() => new Frame(new Body()));
        Assert.Throws<ArgumentException>(() => new Row(new Body()));
        Assert.Throws<ArgumentException>(() => new Body(new Body()));
        Assert.Throws<ArgumentException>(() => new EmailBuilder().Add(new Body()));
    }

    [Theory]
    [InlineData("\" onload=\"alert(1)")]
    [InlineData("en;display:none")]
    [InlineData("")]
    public void InvalidLanguageTagsAreRejected(string language)
    {
        Assert.Throws<ArgumentException>(() => new EmailBuilder(new Body { Language = language }));
    }

    [Fact]
    public void ExcessivelyDeepTreesAreRejectedBeforeRendering()
    {
        EmailComponent component = new Paragraph("Deep");
        for (var index = 0; index < 65; index++) component = new Frame(component);

        Assert.Throws<ArgumentException>(() => new EmailBuilder().Add(component));
    }

    [Fact]
    public void StylesAreReusableImmutableValues()
    {
        var style = new ComponentStyle { BackgroundColor = "#ffffff", Padding = new Spacing(24) };
        var variant = style with { BackgroundColor = "#eeeeee" };

        Assert.Equal("#ffffff", style.BackgroundColor);
        Assert.Equal(style.Padding, variant.Padding);
        var first = new EmailBuilder().Add(new Frame(new Paragraph("Same")) { Style = style }).Build();
        var second = new EmailBuilder().Add(new Frame(new Paragraph("Same")) { Style = style }).Build();
        Assert.Equal(first, second);
    }
}

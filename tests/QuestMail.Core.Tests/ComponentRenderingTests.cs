using System.Globalization;
using System.Xml.Linq;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class ComponentRenderingTests
{
    [Fact]
    public void AllTenComponentsComposeIntoACompleteDocument()
    {
        var body = new Body(new Frame(
            new Heading("Welcome"),
            new Row(
                new Image("https://example.com/product.png", "Product") { LinkUrl = "https://example.com/product" },
                new Frame(new Heading("Details", HeadingLevelEnum.H2),
                    new Heading("Features", HeadingLevelEnum.H3), new Paragraph("Description"))),
            new Button("Buy now", "https://example.com/buy"),
            new Link("Terms", "https://example.com/terms"),
            new Divider(), new Spacer(24))) { Language = "es-MX" };

        var email = new EmailBuilder(body).Build();
        var document = XDocument.Parse(email.Html);

        Assert.Equal("es-MX", document.Root!.Attribute("lang")!.Value);
        Assert.Single(document.Descendants("h1"));
        Assert.Single(document.Descendants("h2"));
        Assert.Single(document.Descendants("h3"));
        Assert.Single(document.Descendants("p"));
        Assert.Single(document.Descendants("img"));
        Assert.Equal(3, document.Descendants("a").Count());
        Assert.Empty(document.Descendants("button"));
        Assert.Contains(document.Descendants("td"), td => (string?)td.Attribute("role") == "separator");
        Assert.Contains("Product (https://example.com/product)", email.Plain);
        Assert.Contains("Buy now (https://example.com/buy)", email.Plain);
        Assert.Contains("Terms (https://example.com/terms)", email.Plain);
        Assert.Equal(string.Join(Environment.NewLine,
            "Welcome", "Product (https://example.com/product)", "Details", "Features", "Description",
            "Buy now (https://example.com/buy)", "Terms (https://example.com/terms)", "---", "", ""), email.Plain);
        Assert.All(document.Descendants("table"), table => Assert.Equal("presentation", (string?)table.Attribute("role")));
    }

    [Fact]
    public void BodyDefaultsToFifteenPixelOuterMarginsAcrossConstructionPaths()
    {
        var emails = new[]
        {
            new EmailBuilder().AddParagraph("Content").Build(),
            new EmailBuilder(new Body(new Paragraph("Content"))).Build(),
            new EmailBuilder(new Body(new List<EmailComponent> { new Paragraph("Content") })).Build(),
            new EmailBuilder(new Body().AddParagraph("Content")).Build()
        };

        foreach (var email in emails)
        {
            var document = XDocument.Parse(email.Html);
            var margin = Assert.Single(document.Descendants("td"), element => (string?)element.Attribute("class") == "qm-0-margin");

            Assert.Equal("padding:15px 15px 15px 15px;", (string?)margin.Attribute("style"));
            // Keep the HTML body reset: the component wrapper provides email-compatible outer spacing.
            Assert.Contains("margin:0;padding:0;", (string)Assert.Single(document.Descendants("body")).Attribute("style")!);
            Assert.Equal($"Content{Environment.NewLine}", email.Plain);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    public void BodyMarginCanBeOverriddenOrRemoved(double pixels)
    {
        var body = new Body(new Paragraph("Content"))
        {
            Style = new ComponentStyle { Margin = new Spacing(pixels) }
        };
        var email = new EmailBuilder(body).Build();
        var margin = Assert.Single(XDocument.Parse(email.Html).Descendants("td"),
            element => (string?)element.Attribute("class") == "qm-0-margin");

        Assert.Equal($"padding:{pixels}px {pixels}px {pixels}px {pixels}px;", (string?)margin.Attribute("style"));
        Assert.Equal($"Content{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void FluentBodyMarginSupportsIndependentSidesWithoutMutatingTheOriginal()
    {
        var original = new Body().AddParagraph("Content");
        var customized = original.WithStyle(new ComponentStyle { Margin = new Spacing(1, 2, 3, 4) });
        var originalHtml = XDocument.Parse(new EmailBuilder(original).Build().Html);
        var customizedHtml = XDocument.Parse(new EmailBuilder(customized).Build().Html);

        Assert.Equal("padding:15px 15px 15px 15px;", (string?)Assert.Single(originalHtml.Descendants("td"),
            element => (string?)element.Attribute("class") == "qm-0-margin").Attribute("style"));
        Assert.Equal("padding:1px 2px 3px 4px;", (string?)Assert.Single(customizedHtml.Descendants("td"),
            element => (string?)element.Attribute("class") == "qm-0-margin").Attribute("style"));
    }

    [Fact]
    public void BodyHasASeparateCanvasAndCenteredContentWithOutlookFallback()
    {
        var email = new EmailBuilder(new Body(new Paragraph("Content"))
        {
            CanvasColor = "#eeeeee",
            Style = new ComponentStyle { Width = EmailWidth.Max(640), BackgroundColor = "#ffffff", Padding = new Spacing(32) }
        }).Build();

        Assert.Contains("<body style=\"margin:0;padding:0;background-color:#eeeeee;\">", email.Html);
        Assert.Contains("max-width:640px;", email.Html);
        Assert.Contains("background-color:#ffffff;", email.Html);
        Assert.Contains("padding:32px 32px 32px 32px;", email.Html);
        Assert.Contains("class=\"qm-0-margin\" align=\"center\" style=\"padding:15px 15px 15px 15px;\"", email.Html);
        Assert.Contains("<!--[if mso]>", email.Html);
        Assert.Contains("width=\"640\" align=\"center\"", email.Html);
    }

    [Fact]
    public void BuilderAppendsToTheRootWithoutMutatingIt()
    {
        var root = new Body(new Paragraph("Original"));
        var builder = new EmailBuilder(root);
        var before = builder.Build();
        var after = builder.Add(new Paragraph("Appended")).Build();

        Assert.Single(root.Children);
        Assert.DoesNotContain("Appended", before.Html);
        Assert.Equal($"Original{Environment.NewLine}Appended{Environment.NewLine}", after.Plain);
        Assert.Equal(before, new EmailBuilder(root).Build());
    }

    [Fact]
    public void FrameSupportsIndependentSpacingBackgroundAndBorders()
    {
        var email = new EmailBuilder().Add(new Frame(new Paragraph("Card"))
        {
            Style = new ComponentStyle
            {
                Width = EmailWidth.Percent(80), MaxWidth = 480,
                Margin = new Spacing(1, 2, 3, 4), Padding = new Spacing(5, 6, 7, 8),
                BackgroundColor = "#fafafa", Border = new EmailBorder(2, "#cccccc", BorderStyleEnum.Dashed),
                BorderRadius = 12, HorizontalAlignment = ElementAlignEnum.Right, VerticalAlignment = VerticalAlignEnum.Middle
            }
        }).Build();

        Assert.Contains("padding:1px 2px 3px 4px;", email.Html);
        Assert.Contains("padding:5px 6px 7px 8px;", email.Html);
        Assert.Contains("background-color:#fafafa;border:2px dashed #cccccc;border-radius:12px;", email.Html);
        Assert.Contains("width:80%;max-width:480px;", email.Html);
        Assert.Contains("align=\"right\"", email.Html);
        Assert.Contains("valign=\"middle\"", email.Html);
    }

    [Theory]
    [InlineData(HeadingLevelEnum.H1, "h1", 32)]
    [InlineData(HeadingLevelEnum.H2, "h2", 24)]
    [InlineData(HeadingLevelEnum.H3, "h3", 20)]
    public void HeadingLevelsAreSemantic(HeadingLevelEnum level, string tag, int size)
    {
        var document = XDocument.Parse(new EmailBuilder().Add(new Heading("Title", level)).Build().Html);
        var heading = Assert.Single(document.Descendants(tag));

        Assert.Equal("Title", heading.Value);
        Assert.Contains($"font-size:{size}px;", (string)heading.Attribute("style")!);
        Assert.Contains("font-weight:700;", (string)heading.Attribute("style")!);
    }

    [Fact]
    public void ParagraphSupportsEncodedStyledRunsAndInlineLinks()
    {
        var email = new EmailBuilder().Add(new Paragraph(new[]
        {
            new TextRun("Hello <Ada>, "),
            new TextRun("read & learn")
            {
                Url = "https://example.com/?a=1&b=2",
                Typography = new TypographyStyle { FontWeight = 700, Italic = true, Decoration = TextDecorationEnum.Underline }
            },
            new TextRun("\nThank you!")
        })).Build();
        var paragraph = Assert.Single(XDocument.Parse(email.Html).Descendants("p"));
        var link = Assert.Single(paragraph.Descendants("a"));

        Assert.Contains("Hello &lt;Ada&gt;", email.Html);
        Assert.Equal("https://example.com/?a=1&b=2", (string?)link.Attribute("href"));
        Assert.Contains("font-weight:700;", (string)link.Attribute("style")!);
        Assert.Contains("font-style:italic;", (string)link.Attribute("style")!);
        Assert.Single(paragraph.Descendants("br"));
        Assert.Equal($"Hello <Ada>, read & learn (https://example.com/?a=1&b=2){Environment.NewLine}Thank you!{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void TypographyInheritsThroughContainersAndCanBeOverridden()
    {
        var document = XDocument.Parse(new EmailBuilder(new Body(new Frame(
            new Paragraph("Inherited"),
            new Paragraph("Override") { Style = new ComponentStyle { Typography = new TypographyStyle { FontSize = 20, Color = "red" } } }))
        {
            Style = new ComponentStyle
            {
                Typography = new TypographyStyle { FontFamily = "Georgia,serif", FontSize = 18, Color = "#334155", LineHeight = 1.75 }
            }
        }).Build().Html);
        var paragraphs = document.Descendants("p").ToArray();

        Assert.Contains("font-family:Georgia,serif;font-size:18px;", (string)paragraphs[0].Attribute("style")!);
        Assert.Contains("color:#334155;line-height:1.75;", (string)paragraphs[0].Attribute("style")!);
        Assert.Contains("font-family:Georgia,serif;font-size:20px;", (string)paragraphs[1].Attribute("style")!);
        Assert.Contains("color:red;", (string)paragraphs[1].Attribute("style")!);
    }

    [Fact]
    public void ButtonRendersAnAccessibleStyledAnchorWithSafeAttributes()
    {
        var email = new EmailBuilder().Add(new Button("Confirm <account>", "https://example.com/?a=1&b=2")
        {
            Title = "Confirm \"now\"", OpenInNewWindow = true,
            Style = new ComponentStyle { Width = EmailWidth.Fixed(240), Padding = new Spacing(16, 32), BorderRadius = 8 }
        }).Build();
        var document = XDocument.Parse(email.Html);
        var anchor = Assert.Single(document.Descendants("a"));

        Assert.Equal("Confirm <account>", anchor.Value);
        Assert.Equal("Confirm \"now\"", (string?)anchor.Attribute("title"));
        Assert.Equal("_blank", (string?)anchor.Attribute("target"));
        Assert.Equal("noopener noreferrer", (string?)anchor.Attribute("rel"));
        Assert.Contains("padding:16px 32px 16px 32px;border-radius:8px;", (string)anchor.Attribute("style")!);
        Assert.Contains("color:#ffffff;", (string)anchor.Attribute("style")!);
        Assert.Empty(document.Descendants("button"));
    }

    [Fact]
    public void ImageEscapesAttributesAndPreservesAspectRatioByDefault()
    {
        var email = new EmailBuilder().Add(new Image("https://example.com/image.png?a=1&b=2", "\"Logo\" <Company>")
        {
            LinkUrl = "https://example.com", Style = new ComponentStyle { Width = EmailWidth.Fixed(180), BorderRadius = 8 }
        }).Build();
        var document = XDocument.Parse(email.Html);
        var image = Assert.Single(document.Descendants("img"));

        Assert.Equal("\"Logo\" <Company>", (string?)image.Attribute("alt"));
        Assert.Equal("https://example.com/image.png?a=1&b=2", (string?)image.Attribute("src"));
        Assert.Equal("180", (string?)image.Attribute("width"));
        Assert.Null(image.Attribute("height"));
        Assert.Contains("height:auto;", (string)image.Attribute("style")!);
        Assert.Equal("a", image.Parent!.Name.LocalName);
        Assert.Equal("https://example.com", (string?)image.Parent.Attribute("href"));
    }

    [Fact]
    public void DecorativeImagesHaveEmptyAltTextAndDoNotAddPlainText()
    {
        var email = new EmailBuilder().Add(new Image("cid:logo", "")).Build();

        Assert.Equal("", (string?)Assert.Single(XDocument.Parse(email.Html).Descendants("img")).Attribute("alt"));
        Assert.Empty(email.Plain);
    }

    [Fact]
    public void ImageHeightIsExplicitOnlyWhenRequested()
    {
        var email = new EmailBuilder().Add(new Image("https://example.com/image.png", "Banner") { Height = 120 }).Build();
        var image = Assert.Single(XDocument.Parse(email.Html).Descendants("img"));

        Assert.Equal("120", (string?)image.Attribute("height"));
        Assert.Contains("height:120px;", (string)image.Attribute("style")!);
    }

    [Fact]
    public void DividerAndSpacerHavePredictablePlainTextRepresentations()
    {
        var email = new EmailBuilder().Add(new Divider("#aabbcc", 2, BorderStyleEnum.Dotted))
            .Add(new Spacer(12.5)).Add(new Spacer(0)).Build();

        Assert.Contains("border-top:2px dotted #aabbcc;", email.Html);
        Assert.Contains("height:12.5px;line-height:12.5px;", email.Html);
        Assert.Contains("aria-hidden=\"true\"", email.Html);
        Assert.Equal($"---{Environment.NewLine}{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void CssNumbersAreCultureInvariant()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
            var email = new EmailBuilder().Add(new Paragraph("Text")
            {
                Style = new ComponentStyle { Typography = new TypographyStyle { FontSize = 16.5, LineHeight = 1.75, LetterSpacing = -0.5 } }
            }).Build();

            Assert.Contains("font-size:16.5px;", email.Html);
            Assert.Contains("line-height:1.75;letter-spacing:-0.5px;", email.Html);
        }
        finally { CultureInfo.CurrentCulture = before; }
    }

    [Fact]
    public void AllConvenienceMethodsReturnTheSameBuilder()
    {
        var builder = new EmailBuilder();

        Assert.Same(builder, builder.AddFrame(new Paragraph("Frame")));
        Assert.Same(builder, builder.AddRow(new Paragraph("Column")));
        Assert.Same(builder, builder.AddButton("Button", "https://example.com"));
        Assert.Same(builder, builder.AddLink("Link", "https://example.com"));
        Assert.Same(builder, builder.AddImage("https://example.com/image.png", "Image"));
        Assert.Same(builder, builder.AddDivider());
        Assert.Same(builder, builder.AddSpacer(16));
    }

    [Fact]
    public void StandaloneLinksSupportTextAlignmentAsABlock()
    {
        var email = new EmailBuilder().Add(new Link("Centered", "https://example.com")
        {
            Style = new ComponentStyle { Typography = new TypographyStyle { TextAlignment = ElementAlignEnum.Center } }
        }).Build();
        var link = Assert.Single(XDocument.Parse(email.Html).Descendants("a"));

        Assert.Contains("display:block;", (string)link.Attribute("style")!);
        Assert.Contains("text-align:center;", (string)link.Attribute("style")!);
    }

    [Fact]
    public void QuotedFontFamilyNamesRemainValidInAttributesAndResponsiveCss()
    {
        var email = new EmailBuilder().Add(new Paragraph("Text")
        {
            Style = new ComponentStyle { Typography = new TypographyStyle { FontFamily = "\"Times New Roman\",serif" } },
            MobileStyle = new ComponentStyle { Typography = new TypographyStyle { FontFamily = "'Segoe UI',sans-serif" } }
        }).Build();
        var document = XDocument.Parse(email.Html);

        Assert.Contains("font-family:\"Times New Roman\",serif;", (string)Assert.Single(document.Descendants("p")).Attribute("style")!);
        Assert.Contains("font-family:'Segoe UI',sans-serif!important;", Assert.Single(document.Descendants("style")).Value);
    }

    [Fact]
    public void LongUnbrokenContentUsesWrappingAndWidthConstrainedTableLayout()
    {
        var text = new string('x', 160);
        var email = new EmailBuilder().Add(new Paragraph(text)).Add(new Button(text, "https://example.com")).Build();
        var document = XDocument.Parse(email.Html);
        var paragraph = Assert.Single(document.Descendants("p"));
        var button = Assert.Single(document.Descendants("a"));

        Assert.All(new[] { paragraph, button }, element =>
            Assert.Contains("word-wrap:break-word;overflow-wrap:anywhere;", (string)element.Attribute("style")!));
        Assert.Contains("table-layout:fixed;", email.Html);
        Assert.Equal($"{text}{Environment.NewLine}{text} (https://example.com){Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void WordWrappingCanBeDisabledExplicitly()
    {
        var email = new EmailBuilder().Add(new Paragraph("Do not wrap")
        {
            Style = new ComponentStyle { Typography = new TypographyStyle { WordWrap = false, WhiteSpace = WhiteSpaceEnum.NoWrap } }
        }).Build();

        Assert.Contains("white-space:nowrap;word-wrap:normal;overflow-wrap:normal;", email.Html);
    }
}

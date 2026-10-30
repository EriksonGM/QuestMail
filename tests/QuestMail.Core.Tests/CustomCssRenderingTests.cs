using System.Xml.Linq;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class CustomCssRenderingTests
{
    private static readonly ComponentStyle VisualStyle = new()
    {
        CustomCss = CssDeclarations.Create(("opacity", "0.75"))
    };

    private static IEnumerable<(Func<ComponentStyle, EmailComponent> Create, string Tag, string Suffix)> Components()
    {
        yield return (style => new Body(new Paragraph("Sample")) { Style = style }, "table", "-box");
        yield return (style => new Frame(new Paragraph("Sample")) { Style = style }, "table", "-box");
        yield return (style => new Row(new Paragraph("Sample")) { Style = style }, "table", "-box");
        yield return (style => new Heading("Sample") { Style = style }, "h1", "-text");
        yield return (style => new Paragraph("Sample") { Style = style }, "p", "-text");
        yield return (style => new Line("Sample") { Style = style }, "p", "-text");
        yield return (style => new Link("Sample", "https://example.com") { Style = style }, "a", "-text");
        yield return (style => new Button("Sample", "https://example.com") { Style = style }, "a", "-text");
        yield return (style => new Image("cid:sample", "Sample") { Style = style }, "img", "-image");
        yield return (style => new Divider { Style = style }, "td", "-separator");
        yield return (style => new Spacer(12) { Style = style }, "div", "-spacer");
    }

    private static EmailContent Render(EmailComponent component) =>
        new EmailBuilder(component as Body ?? new Body(component)).Build();

    [Fact]
    public void CustomCssTargetsExactlyThePrimaryElementForEveryComponent()
    {
        foreach (var (create, tag, suffix) in Components())
        {
            var component = create(VisualStyle);
            var email = Render(component);
            var primary = Assert.Single(XDocument.Parse(email.Html).Descendants(),
                element => ((string?)element.Attribute("style"))?.Contains("opacity:0.75;", StringComparison.Ordinal) == true);

            Assert.Equal(tag, primary.Name.LocalName);
            Assert.EndsWith(suffix, (string?)primary.Attribute("class"));
            Assert.Equal(Render(create(new ComponentStyle())).Plain, email.Plain);
        }
    }

    [Fact]
    public void CustomCssOverridesTypedTypographyInlineAndIsHtmlEncoded()
    {
        var email = Render(new Paragraph("Original Case")
        {
            Style = new ComponentStyle
            {
                Typography = new TypographyStyle { Color = "red", FontSize = 16 },
                CustomCss = CssDeclarations.Create(("color", "blue"), ("font-size", "20px"),
                    ("text-transform", "uppercase"), ("font-family", "\"Times New Roman\",serif"))
            }
        });
        var style = (string)Assert.Single(XDocument.Parse(email.Html).Descendants("p")).Attribute("style")!;

        Assert.Contains("color:red;", style);
        Assert.Contains("color:blue;", style);
        Assert.True(style.IndexOf("color:blue;", StringComparison.Ordinal) > style.IndexOf("color:red;", StringComparison.Ordinal));
        Assert.Contains("font-size:20px;text-transform:uppercase;font-family:\"Times New Roman\",serif;", style);
        Assert.Contains("font-family:&quot;Times New Roman&quot;,serif;", email.Html);
        Assert.Equal($"Original Case{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void MobileCustomCssMergesWithDesktopCssAndWinsOverMobileTypedStyles()
    {
        var email = Render(new Paragraph("Sample")
        {
            Style = new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("color", "red"), ("text-transform", "uppercase"), ("word-spacing", "2px"))
            },
            MobileStyle = new ComponentStyle
            {
                Typography = new TypographyStyle { Color = "blue", FontSize = 14 },
                CustomCss = CssDeclarations.Create(("color", "green"), ("text-transform", "none"))
            }
        });
        var document = XDocument.Parse(email.Html);
        var css = Assert.Single(document.Descendants("style")).Value;

        Assert.Contains("@media screen and (max-width:480px)", css);
        Assert.Contains("font-size:14px!important;", css);
        Assert.Contains("color:blue!important;", css);
        Assert.DoesNotContain("color:red!important;", css);
        Assert.EndsWith("word-spacing:2px!important;color:green!important;text-transform:none!important;}}", css);
    }

    [Fact]
    public void ExplicitMobileTypographyOverridesConflictingDesktopCustomCss()
    {
        var email = Render(new Paragraph("Sample")
        {
            Style = new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("color", "red"), ("font-size", "30px"),
                    ("text-decoration", "underline"), ("text-decoration-color", "red"), ("word-wrap", "normal"))
            },
            MobileStyle = new ComponentStyle
            {
                Typography = new TypographyStyle { Color = "blue", FontSize = 14, Decoration = TextDecorationEnum.None, WordWrap = true }
            }
        });
        var css = Assert.Single(XDocument.Parse(email.Html).Descendants("style")).Value;

        Assert.Contains("color:blue!important;", css);
        Assert.Contains("font-size:14px!important;", css);
        Assert.Contains("text-decoration:none!important;", css);
        Assert.Contains("word-wrap:break-word!important;", css);
        Assert.DoesNotContain("color:red!important;", css);
        Assert.DoesNotContain("font-size:30px!important;", css);
        Assert.DoesNotContain("text-decoration-color:red!important;", css);
        Assert.DoesNotContain("word-wrap:normal!important;", css);
    }

    [Fact]
    public void EveryPrimaryElementGetsResponsiveCssWhenOnlyCustomCssChanges()
    {
        foreach (var (create, _, suffix) in Components())
        {
            var original = create(VisualStyle);
            var mobileStyle = new ComponentStyle { CustomCss = CssDeclarations.Create(("opacity", "0.5")) };
            EmailComponent component = original switch
            {
                Body body => body.WithMobileStyle(mobileStyle),
                Frame frame => frame.WithMobileStyle(mobileStyle),
                Row row => row.WithMobileStyle(mobileStyle),
                Heading => new Heading("Sample") { Style = VisualStyle, MobileStyle = mobileStyle },
                Paragraph => new Paragraph("Sample") { Style = VisualStyle, MobileStyle = mobileStyle },
                Line => new Line("Sample") { Style = VisualStyle, MobileStyle = mobileStyle },
                Link => new Link("Sample", "https://example.com") { Style = VisualStyle, MobileStyle = mobileStyle },
                Button => new Button("Sample", "https://example.com") { Style = VisualStyle, MobileStyle = mobileStyle },
                Image => new Image("cid:sample", "Sample") { Style = VisualStyle, MobileStyle = mobileStyle },
                Divider => new Divider { Style = VisualStyle, MobileStyle = mobileStyle },
                Spacer => new Spacer(12) { Style = VisualStyle, MobileStyle = mobileStyle },
                _ => throw new InvalidOperationException()
            };
            var document = XDocument.Parse(Render(component).Html);
            var primary = Assert.Single(document.Descendants(),
                element => ((string?)element.Attribute("style"))?.Contains("opacity:0.75;", StringComparison.Ordinal) == true);
            var css = Assert.Single(document.Descendants("style")).Value;
            var className = (string)primary.Attribute("class")!;

            Assert.EndsWith(suffix, className);
            Assert.Contains($".{className}{{", css);
            Assert.Contains("opacity:0.5!important;", css);
            Assert.DoesNotContain("opacity:0.75!important;", css);
        }
    }

    [Fact]
    public void MobileTypedContainerStylesOverrideDesktopCustomProperties()
    {
        var email = Render(new Frame(new Paragraph("Sample"))
        {
            Style = new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("background-color", "red"), ("color", "red"),
                    ("border-color", "red"), ("border-top-width", "4px"), ("border-radius", "20px"))
            },
            MobileStyle = new ComponentStyle
            {
                BackgroundColor = "blue", Typography = new TypographyStyle { Color = "green" },
                Border = new EmailBorder(1, "blue"), BorderRadius = 2
            }
        });
        var css = Assert.Single(XDocument.Parse(email.Html).Descendants("style")).Value;

        Assert.Contains("background-color:blue!important;", css);
        Assert.Contains("color:green!important;", css);
        Assert.Contains("border:1px solid blue!important;", css);
        Assert.Contains("border-radius:2px!important;", css);
        Assert.DoesNotContain("color:red!important;", css);
        Assert.DoesNotContain("border-top-width:4px!important;", css);
        Assert.DoesNotContain("border-radius:20px!important;", css);
    }

    [Fact]
    public void DeclarationOrderIsPreservedForShorthandsAndMobileOverrides()
    {
        var email = Render(new Paragraph("Sample")
        {
            Style = new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("text-decoration-color", "red"), ("text-decoration", "underline"))
            },
            MobileStyle = new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("text-decoration-color", "blue"))
            }
        });
        var document = XDocument.Parse(email.Html);
        var inline = (string)Assert.Single(document.Descendants("p")).Attribute("style")!;
        var css = Assert.Single(document.Descendants("style")).Value;

        Assert.EndsWith("text-decoration-color:red;text-decoration:underline;", inline);
        Assert.EndsWith("text-decoration:underline!important;text-decoration-color:blue!important;}}", css);
    }

    [Fact]
    public void EqualDeclarationsDoNotGenerateRedundantMediaRules()
    {
        var email = Render(new Paragraph("Sample")
        {
            Style = new ComponentStyle { CustomCss = CssDeclarations.Create(("color", "red")) },
            MobileStyle = new ComponentStyle { CustomCss = CssDeclarations.Create(("COLOR", "red")) }
        });

        Assert.Empty(XDocument.Parse(email.Html).Descendants("style"));
    }

    [Fact]
    public void CustomDeclarationsAreNotCopiedIntoChildComponentStyles()
    {
        var frame = new Frame(new Paragraph("Sample"))
        {
            Style = new ComponentStyle { CustomCss = CssDeclarations.Create(("text-transform", "uppercase")) }
        };
        var document = XDocument.Parse(Render(frame).Html);

        Assert.Contains("text-transform:uppercase;", (string)Assert.Single(document.Descendants("table"),
            table => (string?)table.Attribute("class") == "qm-1-box").Attribute("style")!);
        Assert.DoesNotContain("text-transform", (string)Assert.Single(document.Descendants("p")).Attribute("style")!);
        Assert.Null(Assert.Single(frame.Children).Style.CustomCss);
    }

    [Fact]
    public void EmptyCustomCssDoesNotChangeDefaultOutput()
    {
        foreach (var (create, _, _) in Components())
            Assert.Equal(Render(create(new ComponentStyle())),
                Render(create(new ComponentStyle { CustomCss = CssDeclarations.Empty })));

        var baseline = Render(new Frame(new Paragraph("Sample")));
        var emptyMobile = Render(new Frame(new Paragraph("Sample"))
        {
            MobileStyle = new ComponentStyle { CustomCss = CssDeclarations.Empty }
        });
        Assert.Equal(baseline, emptyMobile);
    }

    [Fact]
    public void MobileOnlyCustomCssDoesNotAppearInDesktopInlineStyles()
    {
        var email = Render(new Paragraph("Sample")
        {
            MobileStyle = new ComponentStyle { CustomCss = CssDeclarations.Create(("text-transform", "uppercase")) }
        });
        var document = XDocument.Parse(email.Html);

        Assert.DoesNotContain("text-transform", (string)Assert.Single(document.Descendants("p")).Attribute("style")!);
        Assert.Contains("text-transform:uppercase!important;", Assert.Single(document.Descendants("style")).Value);
    }

    [Fact]
    public void MobileTypedBorderRadiusOverridesDesktopCustomCssOnButtonsAndImages()
    {
        var desktop = new ComponentStyle { CustomCss = CssDeclarations.Create(("border-radius", "20px")) };
        var mobile = new ComponentStyle { BorderRadius = 2 };
        EmailComponent[] components =
        [
            new Button("Sample", "https://example.com") { Style = desktop, MobileStyle = mobile },
            new Image("cid:sample", "Sample") { Style = desktop, MobileStyle = mobile }
        ];

        foreach (var component in components)
        {
            var css = Assert.Single(XDocument.Parse(Render(component).Html).Descendants("style")).Value;
            Assert.Contains("border-radius:2px!important;", css);
            Assert.DoesNotContain("border-radius:20px!important;", css);
        }
    }

    [Fact]
    public void SharedCssAndFluentContainerSnapshotsRemainIndependentAndDeterministic()
    {
        var shared = CssDeclarations.Create(("text-transform", "uppercase"));
        var original = new Body().WithStyle(new ComponentStyle { CustomCss = shared }).AddParagraph("Sample");
        var changed = original.WithStyle(original.Style with { CustomCss = shared.Set("opacity", "0.5") });
        var first = new EmailBuilder(original).Build();

        Assert.Equal(first, new EmailBuilder(original).Build());
        Assert.DoesNotContain("opacity:0.5;", first.Html);
        Assert.Contains("opacity:0.5;", new EmailBuilder(changed).Build().Html);
        Assert.False(shared.ContainsKey("opacity"));
    }
}

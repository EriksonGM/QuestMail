using System.Xml.Linq;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class ResponsiveRenderingTests
{
    [Fact]
    public void RowsHaveTableColumnsOnDesktopAndOneColumnOnMobileByDefault()
    {
        var email = new EmailBuilder().Add(new Row(new Paragraph("First"), new Paragraph("Second"))).Build();
        var document = XDocument.Parse(email.Html);
        var row = Assert.Single(document.Descendants("table"), IsRow);
        var columns = row.Element("tbody")!.Element("tr")!.Elements("td").ToArray();

        Assert.Equal(2, columns.Length);
        Assert.All(columns, column => Assert.Null(column.Attribute("width")));
        Assert.Contains("@media screen and (max-width:480px)", email.Html);
        Assert.Contains("display:inline-block!important;width:100%!important;max-width:100%!important;", email.Html);
        Assert.Equal($"First{Environment.NewLine}Second{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void RowsSupportCustomColumnWidthsAndTwoColumnMobileGrids()
    {
        var row = new Row(
            new Frame(new Paragraph("First")) { Style = new ComponentStyle { Width = EmailWidth.Fixed(180) } },
            new Frame(new Paragraph("Second")) { Style = new ComponentStyle { Width = EmailWidth.Percent(60) } },
            new Paragraph("Third"), new Paragraph("Fourth"))
        {
            MobileColumns = 2, Gap = 16,
            Style = new ComponentStyle { VerticalAlignment = VerticalAlignEnum.Middle }
        };
        var email = new EmailBuilder().Add(row).Build();
        var columns = Assert.Single(XDocument.Parse(email.Html).Descendants("table"), IsRow)
            .Element("tbody")!.Element("tr")!.Elements("td").ToArray();

        Assert.Equal("180", (string?)columns[0].Attribute("width"));
        Assert.Equal("60%", (string?)columns[1].Attribute("width"));
        Assert.Null(columns[2].Attribute("width"));
        Assert.DoesNotContain("width:60%;", email.Html);
        Assert.DoesNotContain("width:180px;", email.Html);
        Assert.All(columns, column => Assert.Equal("middle", (string?)column.Attribute("valign")));
        Assert.Contains("width:50%!important;max-width:50%!important;", email.Html);
        Assert.Contains("padding:0 8px 16px!important;", email.Html);
    }

    [Fact]
    public void MobileStylesOverrideOnlySpecifiedProperties()
    {
        var email = new EmailBuilder().Add(new Frame(new Paragraph("Card"))
        {
            Style = new ComponentStyle
            {
                Width = EmailWidth.Fixed(400), Padding = new Spacing(24), BackgroundColor = "#ffffff",
                Border = new EmailBorder(1, "#cccccc"), BorderRadius = 12
            },
            MobileStyle = new ComponentStyle { Width = EmailWidth.Percent(100), Padding = new Spacing(12), BorderRadius = 0 }
        }).Build();

        Assert.Contains("width:400px;", email.Html);
        Assert.Contains("width:100%!important;max-width:none!important;", email.Html);
        Assert.Contains("background-color:#ffffff!important;border:1px solid #cccccc!important;border-radius:0px!important;", email.Html);
        Assert.Contains("padding:12px 12px 12px 12px!important;", email.Html);
    }

    [Fact]
    public void MobileTypographyInheritsThroughContainersWithoutOverwritingLocalStyles()
    {
        var email = new EmailBuilder(new Body(new Frame(
            new Paragraph("Inherited"),
            new Paragraph("Local") { Style = new ComponentStyle { Typography = new TypographyStyle { FontSize = 22 } } }))
        {
            Style = new ComponentStyle { Typography = new TypographyStyle { FontSize = 18 } },
            MobileStyle = new ComponentStyle { Typography = new TypographyStyle { FontSize = 14 } }
        }).Build();
        var document = XDocument.Parse(email.Html);
        var paragraphs = document.Descendants("p").ToArray();
        var inheritedClass = (string)paragraphs[0].Attribute("class")!;
        var localClass = (string)paragraphs[1].Attribute("class")!;
        var css = Assert.Single(document.Descendants("style")).Value;

        Assert.Contains("font-size:18px;", (string)paragraphs[0].Attribute("style")!);
        Assert.Contains($".{inheritedClass}{{", css);
        Assert.Contains("font-size:14px!important;", css);
        Assert.DoesNotContain($".{localClass}{{", css);
        Assert.Contains("font-size:22px;", (string)paragraphs[1].Attribute("style")!);
    }

    [Fact]
    public void MobileImageAndButtonStylesAreAppliedToTheirContent()
    {
        var email = new EmailBuilder()
            .Add(new Image("https://example.com/image.png", "Image")
            {
                Style = new ComponentStyle { Width = EmailWidth.Fixed(320) },
                MobileStyle = new ComponentStyle { Width = EmailWidth.Percent(100), BorderRadius = 4 }
            })
            .Add(new Button("Action", "https://example.com")
            {
                MobileStyle = new ComponentStyle { Padding = new Spacing(16), Typography = new TypographyStyle { FontSize = 18 } }
            }).Build();
        var document = XDocument.Parse(email.Html);
        var imageClass = (string)Assert.Single(document.Descendants("img")).Attribute("class")!;
        var buttonClass = (string)Assert.Single(document.Descendants("a")).Attribute("class")!;
        var css = Assert.Single(document.Descendants("style")).Value;

        Assert.Contains($".{imageClass}{{", css);
        Assert.Contains($".{buttonClass}{{", css);
        Assert.Contains("padding:16px 16px 16px 16px!important;", css);
        Assert.Contains("font-size:18px!important;", css);
    }

    [Fact]
    public void FullyHiddenComponentsAreOmittedFromBothFormats()
    {
        var email = new EmailBuilder().Add(new Frame(new Paragraph("Secret"))
        {
            Style = new ComponentStyle { Visible = false }
        }).Build();

        Assert.DoesNotContain("Secret", email.Html);
        Assert.Empty(email.Plain);
    }

    [Fact]
    public void MobileOnlyAndDesktopOnlyContentRemainAvailableInPlainText()
    {
        var email = new EmailBuilder()
            .Add(new Paragraph("Mobile")
            {
                Style = new ComponentStyle { Visible = false }, MobileStyle = new ComponentStyle { Visible = true }
            })
            .Add(new Paragraph("Desktop") { MobileStyle = new ComponentStyle { Visible = false } }).Build();

        Assert.Contains("display:none;max-height:0;overflow:hidden;mso-hide:all;", email.Html);
        Assert.Contains("display:table!important;max-height:none!important;overflow:visible!important;", email.Html);
        Assert.Contains("display:none!important;max-height:0!important;overflow:hidden!important;", email.Html);
        Assert.Equal($"Mobile{Environment.NewLine}Desktop{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void ChildrenCannotBecomeVisibleOnADeviceWhereTheirParentIsHidden()
    {
        var email = new EmailBuilder().Add(new Frame(new Paragraph("Never visible")
        {
            MobileStyle = new ComponentStyle { Visible = false }
        })
        {
            Style = new ComponentStyle { Visible = false }, MobileStyle = new ComponentStyle { Visible = true }
        }).Build();

        Assert.DoesNotContain("Never visible", email.Html);
        Assert.Empty(email.Plain);
    }

    [Fact]
    public void HiddenRowChildrenHideTheirColumnWithHigherSpecificityThanGridRules()
    {
        var email = new EmailBuilder().Add(new Row(
            new Paragraph("Desktop only") { MobileStyle = new ComponentStyle { Visible = false } },
            new Paragraph("Always"))).Build();
        var row = Assert.Single(XDocument.Parse(email.Html).Descendants("table"), IsRow);
        var column = row.Element("tbody")!.Element("tr")!.Elements("td").First();
        var classes = ((string)column.Attribute("class")!).Split(' ');

        Assert.Contains($".{classes[0]}.{classes[1]}{{display:none!important;", email.Html);
    }

    [Fact]
    public void ReusedComponentsGetUniqueDeterministicResponsiveClassesPerOccurrence()
    {
        var shared = new Paragraph("Reusable")
        {
            MobileStyle = new ComponentStyle { Typography = new TypographyStyle { FontSize = 14 } }
        };
        var builder = new EmailBuilder().Add(new Row(shared, shared));
        var first = builder.Build();
        var second = builder.Build();
        var paragraphs = XDocument.Parse(first.Html).Descendants("p").ToArray();

        Assert.NotEqual((string?)paragraphs[0].Attribute("class"), (string?)paragraphs[1].Attribute("class"));
        Assert.Equal(first, second);
    }

    private static bool IsRow(XElement table) => ((string?)table.Attribute("class"))?.EndsWith("-row", StringComparison.Ordinal) == true;

    [Fact]
    public void SpacerAndImageHeightsCanBeOverriddenOnMobile()
    {
        var email = new EmailBuilder()
            .Add(new Spacer(32) { MobileStyle = new ComponentStyle { Height = 16 } })
            .Add(new Image("https://example.com/image.png", "Image")
            {
                Height = 200, MobileStyle = new ComponentStyle { Height = 100 }
            }).Build();

        Assert.Contains("height:32px;line-height:32px;", email.Html);
        Assert.Contains("height:16px!important;line-height:16px!important;", email.Html);
        Assert.Contains("height:100px!important;", email.Html);
    }
}

using System.Xml.Linq;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;

namespace QuestMail.Core.Tests;

public class EmailBuilderTests
{
    [Fact]
    public void EmptyBuilderProducesAnHtmlDocumentAndEmptyPlainText()
    {
        var email = new EmailBuilder().Build();

        Assert.StartsWith("<!DOCTYPE html>", email.Html);
        Assert.Contains("<meta charset=\"utf-8\" />", email.Html);
        Assert.Contains("name=\"viewport\"", email.Html);
        Assert.Contains("<body", email.Html);
        Assert.EndsWith("</html>", email.Html);
        Assert.Equal(string.Empty, email.Plain);
    }

    [Fact]
    public void FluentMethodsReturnTheSameBuilder()
    {
        var builder = new EmailBuilder();

        Assert.Same(builder, builder.Add(new Line("Reusable")));
        Assert.Same(builder, builder.AddLine("Line"));
        Assert.Same(builder, builder.AddH1("Heading 1"));
        Assert.Same(builder, builder.AddH2("Heading 2"));
        Assert.Same(builder, builder.AddH3("Heading 3"));
        Assert.Same(builder, builder.AddParagraph("Paragraph"));
    }

    [Theory]
    [InlineData(LineSizeEnum.Body, "p", 16)]
    [InlineData(LineSizeEnum.Heading1, "h1", 32)]
    [InlineData(LineSizeEnum.Heading2, "h2", 24)]
    [InlineData(LineSizeEnum.Heading3, "h3", 20)]
    public void LineSizesRenderSemanticTagsAndInlineStyles(LineSizeEnum size, string tag, int fontSize)
    {
        var email = new EmailBuilder().AddLine("Content", size).Build();

        Assert.Contains($"<{tag} style=\"", email.Html);
        Assert.Contains($"font-size:{fontSize}px;", email.Html);
        Assert.Contains($">Content</{tag}>", email.Html);
        Assert.Equal($"Content{Environment.NewLine}", email.Plain);
    }

    [Theory]
    [InlineData(ElementAlignEnum.Left, "left")]
    [InlineData(ElementAlignEnum.Center, "center")]
    [InlineData(ElementAlignEnum.Right, "right")]
    public void AlignmentIsAppliedInline(ElementAlignEnum align, string expected)
    {
        var email = new EmailBuilder().AddLine("Content", align: align).Build();

        Assert.Contains($"text-align:{expected};", email.Html);
    }

    [Fact]
    public void ConvenienceMethodsUseTheirCorrespondingTagsAndForwardAlignment()
    {
        var email = new EmailBuilder()
            .AddH1("One", ElementAlignEnum.Center)
            .AddH2("Two", ElementAlignEnum.Right)
            .AddH3("Three", ElementAlignEnum.Center)
            .AddParagraph("Body", ElementAlignEnum.Right)
            .Build();

        var document = XDocument.Parse(email.Html);
        foreach (var (tag, text, size, align) in new[]
        {
            ("h1", "One", 32, "center"), ("h2", "Two", 24, "right"),
            ("h3", "Three", 20, "center"), ("p", "Body", 16, "right")
        })
        {
            var element = Assert.Single(document.Descendants(tag));
            Assert.Equal(text, element.Value);
            Assert.Contains($"font-size:{size}px;", element.Attribute("style")!.Value);
            Assert.Contains($"text-align:{align};", element.Attribute("style")!.Value);
        }
    }

    [Theory]
    [InlineData(LineSizeEnum.Body)]
    [InlineData(LineSizeEnum.Heading1)]
    [InlineData(LineSizeEnum.Heading2)]
    [InlineData(LineSizeEnum.Heading3)]
    public void TextIsHtmlEncodedButPreservedInPlainText(LineSizeEnum size)
    {
        const string text = "<script>alert(\"x\")</script> & 'quoted'";
        var email = new EmailBuilder().AddLine(text, size).Build();

        Assert.Contains("&lt;script&gt;alert(&quot;x&quot;)&lt;/script&gt; &amp; &#39;quoted&#39;", email.Html);
        Assert.DoesNotContain("<script>", email.Html);
        Assert.Equal(text + Environment.NewLine, email.Plain);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void NewlinesAreNormalizedAndPreservedInBothFormats(string newline)
    {
        var email = new EmailBuilder().AddLine($"First{newline}{newline}Last").Build();

        Assert.Contains(">First<br /><br />Last</p>", email.Html);
        Assert.Equal($"First{Environment.NewLine}{Environment.NewLine}Last{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void BlocksPreserveTheirOrderInBothFormats()
    {
        var email = new EmailBuilder().AddH1("First").AddParagraph("Second").Build();

        Assert.True(email.Html.IndexOf(">First</h1>", StringComparison.Ordinal) <
            email.Html.IndexOf(">Second</p>", StringComparison.Ordinal));
        Assert.Equal($"First{Environment.NewLine}Second{Environment.NewLine}", email.Plain);
    }

    [Fact]
    public void BuildReturnsAnIndependentSnapshotAndDoesNotConsumeTheBuilder()
    {
        var builder = new EmailBuilder().AddH1("First");
        var first = builder.Build();
        var repeated = builder.Build();
        var second = builder.AddParagraph("Second").Build();

        Assert.Equal(first, repeated);
        Assert.DoesNotContain("Second", first.Html);
        Assert.DoesNotContain("Second", first.Plain);
        Assert.Contains("Second", second.Html);
        Assert.Contains("Second", second.Plain);
    }

    [Fact]
    public void ALineCanBeReusedByMultipleBuilders()
    {
        var line = new Line("Reusable", LineSizeEnum.Heading2, ElementAlignEnum.Center);

        Assert.Equal(new EmailBuilder().Add(line).Build(), new EmailBuilder().Add(line).Build());
    }

    [Fact]
    public void EmptyTextIsAllowed()
    {
        var email = new EmailBuilder().AddLine(string.Empty).Build();

        Assert.Contains("></p>", email.Html);
        Assert.Equal(Environment.NewLine, email.Plain);
    }

    [Fact]
    public void NullTextIsRejectedWithoutChangingTheBuilder()
    {
        var builder = new EmailBuilder().AddLine("Existing");
        var before = builder.Build();

        Assert.Throws<ArgumentNullException>("text", () => builder.AddLine(null!));
        Assert.Equal(before, builder.Build());
    }

    [Fact]
    public void NullLineIsRejectedWithoutChangingTheBuilder()
    {
        var builder = new EmailBuilder();

        Assert.Throws<ArgumentNullException>("line", () => builder.Add(null!));
        Assert.Equal(string.Empty, builder.Build().Plain);
    }

    [Fact]
    public void NullBuilderIsRejected()
    {
        Assert.Throws<ArgumentNullException>("builder", () => LineExtensions.AddLine(null!, "Text"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void InvalidSizeIsRejectedWithoutChangingTheBuilder(int size)
    {
        var builder = new EmailBuilder().AddLine("Existing");
        var before = builder.Build();

        Assert.Throws<ArgumentOutOfRangeException>("size", () => builder.AddLine("Text", (LineSizeEnum)size));
        Assert.Equal(before, builder.Build());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void InvalidAlignmentIsRejectedWithoutChangingTheBuilder(int align)
    {
        var builder = new EmailBuilder().AddLine("Existing");
        var before = builder.Build();

        Assert.Throws<ArgumentOutOfRangeException>("align", () => builder.AddLine("Text", align: (ElementAlignEnum)align));
        Assert.Equal(before, builder.Build());
    }
}

using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class FluentValidationTests
{
    [Fact]
    public void CallbackFailuresDoNotAddPartialContainersToTheParent()
    {
        var builder = new EmailBuilder().AddParagraph("Existing");
        var before = builder.Build();

        Assert.Throws<InvalidOperationException>(() => builder.AddFrame(frame =>
        {
            frame.AddParagraph("Draft");
            throw new InvalidOperationException("Stop");
        }));
        Assert.Throws<InvalidOperationException>(() => builder.AddRow(row =>
        {
            row.AddParagraph("Draft");
            throw new InvalidOperationException("Stop");
        }));
        Assert.Equal(before, builder.Build());
    }

    [Fact]
    public void InvalidScopeStylesAreRejectedBeforeTheParentIsChanged()
    {
        var builder = new EmailBuilder().AddParagraph("Existing");
        var before = builder.Build();
        var invalid = new ComponentStyle { BackgroundColor = "red;display:none" };

        Assert.Throws<ArgumentException>(() => builder.AddFrame(frame => frame.WithStyle(invalid).AddParagraph("Draft")));
        Assert.Throws<ArgumentException>(() => builder.AddRow(row => row.WithMobileStyle(invalid).AddParagraph("Draft")));
        Assert.Equal(before, builder.Build());
    }

    [Fact]
    public void NullCallbacksAndNullCompositionTargetsAreRejected()
    {
        var body = new Body();
        Assert.Throws<ArgumentNullException>(() => body.AddFrame((Action<FrameBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => body.AddRow((Action<RowBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddHeading());
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddParagraph());
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddFrame(_ => { }));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddRow(_ => { }));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddImage("cid:logo", "Logo"));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddButton("Action", "https://example.com"));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddLink("Link", "https://example.com"));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddDivider());
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddSpacer(24));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).AddLine());
    }

    [Fact]
    public void NullContentAndCollectionsAreRejected()
    {
        var body = new Body();
        Assert.Throws<ArgumentNullException>(() => body.AddHeading((string)null!));
        Assert.Throws<ArgumentNullException>(() => body.AddHeading((IEnumerable<TextRun>)null!));
        Assert.Throws<ArgumentNullException>(() => body.AddParagraph((string)null!));
        Assert.Throws<ArgumentNullException>(() => body.AddParagraph((IEnumerable<TextRun>)null!));
        Assert.Throws<ArgumentNullException>(() => body.AddFrame((EmailComponent[])null!));
        Assert.Throws<ArgumentNullException>(() => body.AddRow((EmailComponent[])null!));
        Assert.Throws<ArgumentNullException>(() => body.Add(null!));
        Assert.Throws<ArgumentNullException>(() => new FrameBuilder().Add(null!));
        Assert.Throws<ArgumentNullException>(() => new RowBuilder().Add(null!));
        Assert.Empty(body.Children);
    }

    [Fact]
    public void BodyCannotBeNestedThroughAnyFluentTarget()
    {
        Assert.Throws<ArgumentException>(() => new Body().Add(new Body()));
        Assert.Throws<ArgumentException>(() => new Frame().Add(new Body()));
        Assert.Throws<ArgumentException>(() => new Row().Add(new Body()));
        Assert.Throws<ArgumentException>(() => new FrameBuilder().Add(new Body()));
        Assert.Throws<ArgumentException>(() => new RowBuilder().Add(new Body()));
        Assert.Throws<ArgumentException>(() => new Body().AddFrame(frame => frame.Add(new Body())));
    }

    [Fact]
    public void InvalidRootSettingsAndStylesLeaveTheOriginalUnchanged()
    {
        var body = new Body().AddParagraph("Original");
        var before = new EmailBuilder(body).Build();

        Assert.Throws<ArgumentNullException>(() => body.WithStyle(null!));
        Assert.Throws<ArgumentNullException>(() => body.WithLanguage(null!));
        Assert.Throws<ArgumentNullException>(() => body.WithCanvasColor(null!));
        Assert.Throws<ArgumentException>(() => body.WithLanguage("en;invalid"));
        Assert.Throws<ArgumentException>(() => body.WithCanvasColor("red;display:none"));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.WithStyle(new ComponentStyle { Height = -1 }));
        Assert.Throws<ArgumentException>(() => body.WithMobileStyle(new ComponentStyle { BackgroundColor = "red;display:none" }));
        Assert.Equal(before, new EmailBuilder(body).Build());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidRowGapsAreRejectedByBothConfigurationStyles(double gap)
    {
        var row = new Row().WithGap(8);
        var draft = new RowBuilder().WithGap(8);

        Assert.Throws<ArgumentOutOfRangeException>(() => row.WithGap(gap));
        Assert.Throws<ArgumentOutOfRangeException>(() => draft.WithGap(gap));
        Assert.Equal(8, row.Gap);
        Assert.Equal(8, draft.Build().Gap);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidMobileColumnsAreRejectedByBothConfigurationStyles(int columns)
    {
        var row = new Row().WithMobileColumns(2);
        var draft = new RowBuilder().WithMobileColumns(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => row.WithMobileColumns(columns));
        Assert.Throws<ArgumentOutOfRangeException>(() => draft.WithMobileColumns(columns));
        Assert.Equal(2, row.MobileColumns);
        Assert.Equal(2, draft.Build().MobileColumns);
    }

    [Fact]
    public void InvalidLeafContentUsesTheExistingValidationRules()
    {
        var body = new Body().AddParagraph("Original");

        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddHeading("Title", (HeadingLevelEnum)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddH1("Title", (ElementAlignEnum)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddParagraph("Text", (ElementAlignEnum)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddLine("Text", (LineSizeEnum)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddSpacer(-1));
        Assert.Throws<ArgumentException>(() => body.AddButton("Action", "javascript:alert(1)"));
        Assert.Throws<ArgumentException>(() => body.AddLink("Link", "javascript:alert(1)"));
        Assert.Throws<ArgumentException>(() => body.AddImage("data:image/png,invalid", "Image"));
        Assert.Throws<ArgumentException>(() => body.AddImage("cid:logo", "Logo", linkUrl: "javascript:alert(1)"));
        Assert.Throws<ArgumentOutOfRangeException>(() => body.AddImage("cid:logo", "Logo", height: -1));
        Assert.Throws<ArgumentException>(() => body.AddDivider("red;display:none"));
        Assert.Single(body.Children);
    }

    [Fact]
    public void InvalidLeavesDoNotChangeMutableScopesOrEmailBuilders()
    {
        var builder = new EmailBuilder().AddParagraph("Original");
        var scope = new FrameBuilder().AddParagraph("Original");
        var before = builder.Build();
        var invalid = new ComponentStyle { Height = -1 };

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddHeading("Draft", style: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => scope.AddHeading("Draft", style: invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddImage("cid:logo", "Logo", height: -1));
        Assert.Equal(before, builder.Build());
        Assert.Single(scope.Build().Children);
    }

    [Fact]
    public void FluentCompositionCannotBypassTheTreeDepthLimit()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var frame = new Frame().AddParagraph("Deep");
            for (var index = 0; index < 65; index++) frame = new Frame().Add(frame);
        });
    }

    [Fact]
    public void NullConfigurationReceiversAndScopeStylesAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).WithStyle(new ComponentStyle()));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).WithMobileStyle(null));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).WithLanguage("en"));
        Assert.Throws<ArgumentNullException>(() => ((Body)null!).WithCanvasColor("white"));
        Assert.Throws<ArgumentNullException>(() => ((Row)null!).WithGap(8));
        Assert.Throws<ArgumentNullException>(() => ((Row)null!).WithMobileColumns(2));
        Assert.Throws<ArgumentNullException>(() => new FrameBuilder().WithStyle(null!));
    }

    [Fact]
    public void InvalidStyleChangesLeaveScopeConfigurationUnchanged()
    {
        var desktop = new ComponentStyle { BackgroundColor = "white" };
        var mobile = new ComponentStyle { Padding = new Spacing(12) };
        var scope = new FrameBuilder().WithStyle(desktop).WithMobileStyle(mobile);
        var invalid = new ComponentStyle { BackgroundColor = "red;display:none" };

        Assert.Throws<ArgumentException>(() => scope.WithStyle(invalid));
        Assert.Throws<ArgumentException>(() => scope.WithMobileStyle(invalid));
        Assert.Same(desktop, scope.Build().Style);
        Assert.Same(mobile, scope.Build().MobileStyle);
    }
}

using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class FluentCompositionTests
{
    [Fact]
    public void RequestedSyntaxSupportsParameterlessTextBlocks()
    {
        var body = new Body().AddFrame(f => { f.AddHeading().AddParagraph(); }).AddSpacer(24);

        var frame = Assert.IsType<Frame>(body.Children[0]);
        Assert.Equal("", Assert.IsType<Heading>(frame.Children[0]).Text);
        Assert.Equal("", Assert.IsType<Paragraph>(frame.Children[1]).Text);
        Assert.Equal(24, Assert.IsType<Spacer>(body.Children[1]).Height);
        Assert.Equal(Environment.NewLine + Environment.NewLine + Environment.NewLine, new EmailBuilder(body).Build().Plain);
    }

    [Fact]
    public void AllComponentsRenderIdenticallyToConstructorComposition()
    {
        var desktop = new ComponentStyle { Padding = new Spacing(12), Typography = new TypographyStyle { Color = "#334155" } };
        var mobile = new ComponentStyle { Padding = new Spacing(6), Typography = new TypographyStyle { FontSize = 14 } };
        TextRun[] runs = [new("Hello <Ada> "), new("Guide") { Url = "https://example.com/guide" }];
        var direct = new Body(new Frame(
            new Heading(runs, HeadingLevelEnum.H2) { Style = desktop, MobileStyle = mobile },
            new Heading("Third", HeadingLevelEnum.H3),
            new Paragraph(runs) { Style = desktop, MobileStyle = mobile },
            new Row(new Paragraph("Left"), new Paragraph("Right"))
            {
                Gap = 16, MobileColumns = 2, Style = desktop, MobileStyle = mobile
            },
            new Image("https://example.com/image.png", "Image")
            {
                Height = 120, LinkUrl = "https://example.com/image", Style = desktop, MobileStyle = mobile
            },
            new Button("Open", "https://example.com")
            {
                Title = "Open account", OpenInNewWindow = true, Style = desktop, MobileStyle = mobile
            },
            new Link("Terms", "https://example.com/terms")
            {
                Title = "Read terms", OpenInNewWindow = true, Style = desktop, MobileStyle = mobile
            },
            new Divider("#cccccc", 2, BorderStyleEnum.Dashed) { Style = desktop, MobileStyle = mobile },
            new Spacer(24) { Style = desktop, MobileStyle = mobile })
        {
            Style = desktop, MobileStyle = mobile
        })
        {
            Language = "es-MX", CanvasColor = "#eeeeee", Style = desktop, MobileStyle = mobile
        };

        var fluent = new Body().WithLanguage("es-MX").WithCanvasColor("#eeeeee")
            .WithStyle(desktop).WithMobileStyle(mobile)
            .AddFrame(frame => frame.WithStyle(desktop).WithMobileStyle(mobile)
                .AddHeading(runs, HeadingLevelEnum.H2, style: desktop, mobileStyle: mobile)
                .AddH3("Third")
                .AddParagraph(runs, style: desktop, mobileStyle: mobile)
                .AddRow(row => row.WithGap(16).WithMobileColumns(2).WithStyle(desktop).WithMobileStyle(mobile)
                    .AddParagraph("Left").AddParagraph("Right"))
                .AddImage("https://example.com/image.png", "Image", desktop, mobile,
                    height: 120, linkUrl: "https://example.com/image")
                .AddButton("Open", "https://example.com", desktop, mobile, title: "Open account", openInNewWindow: true)
                .AddLink("Terms", "https://example.com/terms", desktop, mobile, title: "Read terms", openInNewWindow: true)
                .AddDivider("#cccccc", 2, BorderStyleEnum.Dashed, desktop, mobile)
                .AddSpacer(24, desktop, mobile));

        Assert.Equal(new EmailBuilder(direct).Build(), new EmailBuilder(fluent).Build());
    }

    [Fact]
    public void BodyChainsAreIndependentAndPreserveRootSettings()
    {
        var desktop = new ComponentStyle { Width = EmailWidth.Max(640) };
        var mobile = new ComponentStyle { Padding = new Spacing(12) };
        var original = new Body().WithLanguage("es-ES").WithCanvasColor("#eeeeee")
            .WithStyle(desktop).WithMobileStyle(mobile);
        Body first = original.AddHeading("First");
        Body second = original.AddParagraph("Second");

        Assert.Empty(original.Children);
        Assert.Single(first.Children);
        Assert.Single(second.Children);
        Assert.Equal("First", Assert.IsType<Heading>(first.Children[0]).Text);
        Assert.Equal("Second", Assert.IsType<Paragraph>(second.Children[0]).Text);
        Assert.Equal("es-ES", first.Language);
        Assert.Equal("#eeeeee", first.CanvasColor);
        Assert.Same(desktop, first.Style);
        Assert.Same(mobile, first.MobileStyle);
    }

    [Fact]
    public void StandaloneFramesAndRowsKeepTheirConcreteTypesAndSettings()
    {
        var desktop = new ComponentStyle { BackgroundColor = "#fafafa" };
        var mobile = new ComponentStyle { Visible = false };
        var empty = new Frame().WithStyle(desktop).WithMobileStyle(mobile);
        Frame frame = empty.AddHeading("Frame").AddParagraph("Text");
        Row row = new Row().WithGap(16).WithMobileColumns(2).WithStyle(desktop).WithMobileStyle(mobile)
            .Add(frame).AddFrame(f => f.AddParagraph("Other"));

        Assert.Empty(empty.Children);
        Assert.Equal(2, frame.Children.Count);
        Assert.Same(desktop, frame.Style);
        Assert.Same(mobile, frame.MobileStyle);
        Assert.Equal(16, row.Gap);
        Assert.Equal(2, row.MobileColumns);
        Assert.Equal(2, row.Children.Count);
        Assert.Same(desktop, row.Style);
        Assert.Same(mobile, row.MobileStyle);
    }

    [Fact]
    public void NestedCallbacksCreateIndependentScopes()
    {
        var body = new Body()
            .AddFrame(frame =>
            {
                frame.AddH1("Outer")
                    .AddRow(row => row.WithGap(8)
                        .AddFrame(left => left.AddH2("Left").AddParagraph("First"))
                        .AddFrame(right => right.AddH2("Right").AddParagraph("Second")))
                    .AddParagraph("After row");
            })
            .AddParagraph("After frame");

        var section = Assert.IsType<Frame>(body.Children[0]);
        var columns = Assert.IsType<Row>(section.Children[1]);
        Assert.Equal(2, body.Children.Count);
        Assert.Equal(3, section.Children.Count);
        Assert.Equal(2, columns.Children.Count);
        Assert.Equal(2, Assert.IsType<Frame>(columns.Children[0]).Children.Count);
        Assert.Equal($"Outer{Environment.NewLine}Left{Environment.NewLine}First{Environment.NewLine}" +
            $"Right{Environment.NewLine}Second{Environment.NewLine}After row{Environment.NewLine}After frame{Environment.NewLine}",
            new EmailBuilder(body).Build().Plain);
    }

    [Fact]
    public void CallbackDraftsCannotChangeComponentsAfterTheyAreAdded()
    {
        FrameBuilder? capturedFrame = null;
        RowBuilder? capturedRow = null;
        var body = new Body().AddFrame(frame =>
        {
            capturedFrame = frame;
            frame.AddParagraph("Original").AddRow(row =>
            {
                capturedRow = row;
                row.WithGap(8).AddParagraph("Column");
            });
        });
        var before = new EmailBuilder(body).Build();

        capturedFrame!.AddParagraph("Late frame").WithStyle(new ComponentStyle { BackgroundColor = "red" });
        capturedRow!.AddParagraph("Late row").WithGap(32).WithMobileColumns(2);

        Assert.Equal(before, new EmailBuilder(body).Build());
        var builtFrame = Assert.IsType<Frame>(body.Children[0]);
        var builtRow = Assert.IsType<Row>(builtFrame.Children[1]);
        Assert.Equal(2, builtFrame.Children.Count);
        Assert.Equal(8, builtRow.Gap);
        Assert.Equal(1, builtRow.MobileColumns);
        Assert.Single(builtRow.Children);
    }

    [Fact]
    public void BuildersReturnTheSameScopeButBuildIndependentSnapshots()
    {
        var frame = new FrameBuilder();
        var row = new RowBuilder();

        Assert.Same(frame, frame.AddHeading("Heading"));
        Assert.Same(frame, frame.AddParagraph("Text"));
        Assert.Same(frame, frame.WithStyle(new ComponentStyle()));
        Assert.Same(frame, frame.WithMobileStyle(null));
        var first = frame.Build();
        frame.AddSpacer(24);
        Assert.Equal(2, first.Children.Count);
        Assert.Equal(3, frame.Build().Children.Count);
        Assert.Same(row, row.WithGap(16).WithMobileColumns(2).AddParagraph("Column"));
        var firstRow = row.Build();
        row.WithGap(8).WithMobileColumns(1).AddParagraph("Other");
        Assert.Equal(16, firstRow.Gap);
        Assert.Equal(2, firstRow.MobileColumns);
        Assert.Single(firstRow.Children);
    }

    [Fact]
    public void ExistingComponentsCanBeMixedWithCallbacksAndArrays()
    {
        var shared = new Paragraph("Shared");
        EmailComponent[] columns = [shared, new Frame().AddParagraph("Reusable frame")];
        var body = new Body().AddFrame(shared).AddRow(columns).AddFrame(f => f.Add(shared));
        columns[0] = new Paragraph("Replaced");

        Assert.Equal(3, body.Children.Count);
        Assert.Same(shared, Assert.IsType<Row>(body.Children[1]).Children[0]);
        Assert.Same(shared, Assert.IsType<Frame>(body.Children[2]).Children[0]);
        Assert.DoesNotContain("Replaced", new EmailBuilder(body).Build().Plain);
    }

    [Fact]
    public void EmailBuilderSupportsBothNamespacesAndTheNewCallbacks()
    {
        var builder = new EmailBuilder();

        Assert.Same(builder, builder.AddHeading("Title"));
        Assert.Same(builder, builder.AddParagraph());
        Assert.Same(builder, builder.AddFrame(f => f.AddParagraph("Frame")));
        Assert.Same(builder, builder.AddRow(r => r.AddParagraph("Column")));
        Assert.Same(builder, builder.AddFrame(new Paragraph("Existing overload")));
        Assert.Same(builder, builder.AddParagraph("Aligned", ElementAlignEnum.Center));
        Assert.Same(builder, builder.AddButton("Action", "https://example.com", style: new ComponentStyle { BorderRadius = 8 }));
        Assert.Contains("Existing overload", builder.Build().Plain);
        Assert.Contains("text-align:center;", builder.Build().Html);
    }

    [Fact]
    public void HeadingShortcutsAndAlignmentPreserveOtherStyles()
    {
        var style = new ComponentStyle
        {
            BackgroundColor = "#fafafa", Typography = new TypographyStyle { FontFamily = "Georgia", Color = "red" }
        };
        var body = new Body().AddH1("One", ElementAlignEnum.Center, style)
            .AddH2("Two", ElementAlignEnum.Right).AddH3("Three")
            .AddParagraph("Body", ElementAlignEnum.Center, style);

        Assert.Equal(HeadingLevelEnum.H1, Assert.IsType<Heading>(body.Children[0]).Level);
        Assert.Equal(HeadingLevelEnum.H2, Assert.IsType<Heading>(body.Children[1]).Level);
        Assert.Equal(HeadingLevelEnum.H3, Assert.IsType<Heading>(body.Children[2]).Level);
        Assert.Equal(ElementAlignEnum.Center, body.Children[0].Style.Typography!.TextAlignment);
        Assert.Equal("Georgia", body.Children[0].Style.Typography!.FontFamily);
        Assert.Equal("red", body.Children[0].Style.Typography!.Color);
        Assert.Equal(ElementAlignEnum.Center, body.Children[3].Style.Typography!.TextAlignment);
        Assert.Null(style.Typography.TextAlignment);
    }

    [Fact]
    public void LineAndLeafOptionsAreForwardedWithoutLosingStyles()
    {
        var style = new ComponentStyle { Padding = new Spacing(12) };
        var mobile = new ComponentStyle { Padding = new Spacing(6) };
        var body = new Body().AddLine("Legacy", LineSizeEnum.Heading2, ElementAlignEnum.Right, style, mobile)
            .AddImage("cid:logo", "Logo", style, mobile, height: 120, linkUrl: "https://example.com/logo")
            .AddButton("Open", "https://example.com", style, mobile, "Title", true)
            .AddLink("Terms", "https://example.com/terms", style, mobile, "Terms title", true)
            .AddDivider("#cccccc", 2, BorderStyleEnum.Dotted, style, mobile)
            .AddSpacer(24, style, mobile);

        var line = Assert.IsType<Line>(body.Children[0]);
        var image = Assert.IsType<Image>(body.Children[1]);
        var button = Assert.IsType<Button>(body.Children[2]);
        var link = Assert.IsType<Link>(body.Children[3]);
        var divider = Assert.IsType<Divider>(body.Children[4]);
        Assert.Equal(LineSizeEnum.Heading2, line.Size);
        Assert.Equal(ElementAlignEnum.Right, line.Align);
        Assert.Equal(120, image.Height);
        Assert.Equal("https://example.com/logo", image.LinkUrl);
        Assert.Equal("Title", button.Title);
        Assert.True(button.OpenInNewWindow);
        Assert.Equal("Terms title", link.Title);
        Assert.True(link.OpenInNewWindow);
        Assert.Equal(BorderStyleEnum.Dotted, divider.LineStyle);
        Assert.Equal(2, divider.Thickness);
        Assert.All(body.Children, child =>
        {
            Assert.Same(style, child.Style);
            Assert.Same(mobile, child.MobileStyle);
        });
    }

    [Fact]
    public void MobileOverridesCanBeClearedWithoutChangingEarlierVersions()
    {
        var original = new Body().WithMobileStyle(new ComponentStyle { Visible = false }).AddParagraph("Visible");
        var cleared = original.WithMobileStyle(null);

        Assert.NotNull(original.MobileStyle);
        Assert.Null(cleared.MobileStyle);
        Assert.Equal(original.Children, cleared.Children);
        Assert.DoesNotContain("display:none!important", new EmailBuilder(cleared).Build().Html);
    }

    [Fact]
    public void EmptyContainersAndScopesAreSupported()
    {
        var body = new Body().AddFrame().AddRow().AddFrame(_ => { }).AddRow(_ => { });

        Assert.Equal(4, body.Children.Count);
        Assert.All(body.Children, child => Assert.Empty(Assert.IsAssignableFrom<ContainerComponent>(child).Children));
        Assert.Empty(new EmailBuilder(body).Build().Plain);
    }
}

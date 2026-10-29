using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

internal static class FluentExample
{
    public static Body CreateBody(ComponentStyle cardStyle) => new Body()
        .WithStyle(new ComponentStyle { Width = EmailWidth.Max(640), Padding = new Spacing(32) })
        .WithMobileStyle(new ComponentStyle { Padding = new Spacing(16) })
        .AddHeading("Welcome to QuestMail",
            style: new ComponentStyle { Typography = new TypographyStyle { TextAlignment = ElementAlignEnum.Center } },
            mobileStyle: new ComponentStyle { Typography = new TypographyStyle { FontSize = 28 } })
        .AddImage("https://placehold.co/576x200/png", "QuestMail component engine",
            style: new ComponentStyle { Width = EmailWidth.Max(576), Margin = new Spacing(0, 0, 24, 0), BorderRadius = 8 })
        .AddFrame(frame => frame
            .AddHeading("Your account is ready", HeadingLevelEnum.H2)
            .AddParagraph(new[]
            {
                new TextRun("Hello, "),
                new TextRun("Ada & team") { Typography = new TypographyStyle { FontWeight = 700 } },
                new TextRun(". Build readable emails with reusable C# components.")
            })
            .AddRow(row => row.WithGap(16).WithMobileColumns(1)
                .AddFrame(left => left.WithStyle(cardStyle)
                    .AddHeading("Compose", HeadingLevelEnum.H3)
                    .AddParagraph("Arrange text, images and actions in frames and rows."))
                .AddFrame(right => right.WithStyle(cardStyle)
                    .AddHeading("Adapt", HeadingLevelEnum.H3)
                    .AddParagraph("Desktop columns stack on mobile. Both email bodies stay in sync.")))
            .AddSpacer(24, mobileStyle: new ComponentStyle { Height = 16 })
            .AddButton("Open your dashboard", "https://example.com/dashboard",
                style: new ComponentStyle { HorizontalAlignment = ElementAlignEnum.Center },
                mobileStyle: new ComponentStyle { Width = EmailWidth.Percent(100) }))
        .AddDivider()
        .AddLink("Read the terms and conditions", "https://example.com/terms");
}

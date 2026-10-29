using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

var cardStyle = new ComponentStyle
{
    BackgroundColor = "#f8fafc", Padding = new Spacing(16),
    Border = new EmailBorder(1, "#e2e8f0"), BorderRadius = 8
};
var body = new Body(
    new Heading("Welcome to QuestMail")
    {
        Style = new ComponentStyle { Typography = new TypographyStyle { TextAlignment = ElementAlignEnum.Center } },
        MobileStyle = new ComponentStyle { Typography = new TypographyStyle { FontSize = 28 } }
    },
    new Image("https://placehold.co/576x200/png", "QuestMail component engine")
    {
        Style = new ComponentStyle { Width = EmailWidth.Max(576), Margin = new Spacing(0, 0, 24, 0), BorderRadius = 8 }
    },
    new Frame(
        new Heading("Your account is ready", HeadingLevelEnum.H2),
        new Paragraph(new[]
        {
            new TextRun("Hello, "),
            new TextRun("Ada & team") { Typography = new TypographyStyle { FontWeight = 700 } },
            new TextRun(". Build readable emails with reusable C# components.")
        }),
        new Row(
            new Frame(new Heading("Compose", HeadingLevelEnum.H3),
                new Paragraph("Arrange text, images and actions in frames and rows.")) { Style = cardStyle },
            new Frame(new Heading("Adapt", HeadingLevelEnum.H3),
                new Paragraph("Desktop columns stack on mobile. Both email bodies stay in sync.")) { Style = cardStyle })
        {
            Gap = 16, MobileColumns = 1
        },
        new Spacer(24) { MobileStyle = new ComponentStyle { Height = 16 } },
        new Button("Open your dashboard", "https://example.com/dashboard")
        {
            Style = new ComponentStyle { HorizontalAlignment = ElementAlignEnum.Center },
            MobileStyle = new ComponentStyle { Width = EmailWidth.Percent(100) }
        }),
    new Divider(),
    new Link("Read the terms and conditions", "https://example.com/terms"))
{
    Style = new ComponentStyle { Width = EmailWidth.Max(640), Padding = new Spacing(32) },
    MobileStyle = new ComponentStyle { Padding = new Spacing(16) }
};
var email = new EmailBuilder(args.Contains("--fluent", StringComparer.Ordinal)
    ? FluentExample.CreateBody(cardStyle)
    : body).Build();

if (args.Contains("--html", StringComparer.Ordinal))
{
    Console.WriteLine(email.Html);
    return;
}

Console.WriteLine("HTML:");
Console.WriteLine(email.Html);
Console.WriteLine();
Console.WriteLine("Plain text:");
Console.WriteLine(email.Plain);

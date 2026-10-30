using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

internal static class TemplateLayout
{
    public static Body Create(string title) => new Body()
        .WithLanguage("en")
        .WithCanvasColor("#f1f5f9")
        .WithStyle(new ComponentStyle
        {
            Width = EmailWidth.Max(640),
            BackgroundColor = "#ffffff",
            Padding = new Spacing(32),
            Typography = new TypographyStyle { FontFamily = "Arial, sans-serif", Color = "#1e293b", FontSize = 16 }
        })
        .WithMobileStyle(new ComponentStyle { Padding = new Spacing(16) })
        .AddParagraph("QUESTMAIL / LOCAL PREVIEW",
            style: new ComponentStyle { Typography = new TypographyStyle { FontSize = 12, Color = "#475569" } })
        .AddH1(title)
        .AddSpacer(16);

    public static ComponentStyle Card { get; } = new()
    {
        BackgroundColor = "#f8fafc",
        Padding = new Spacing(16),
        Border = new EmailBorder(1, "#cbd5e1"),
        BorderRadius = 8
    };
}

using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class BritflowWelcomeTemplate : IEmailTemplate
{
    public string Name => "britflow-welcome";
    public string Subject => "Britflow — wahoo, you're in!";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("phone-dashboard");

    public Body CreateBody() => ReferenceLayout.Create("#e7ece7")
        .AddFrame(brand => brand.WithStyle(ReferenceLayout.Section("#45574b", 20)).Add(ReferenceLayout.Brand("britflow", "#ffffff")))
        .AddFrame(main => main.WithStyle(ReferenceLayout.Section()).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddH1("Wahoo! You're In", style: ReferenceLayout.Text(28, centered: true, weight: 700))
            .AddParagraph("Explore something exciting!", style: ReferenceLayout.Text(13, centered: true))
            .AddButton("Start Exploring Now", "https://example.com/britflow/explore", style: ReferenceLayout.Action("#111111", radius: 32))
            .AddImage(ReferenceLayout.Cid("phone-dashboard"), "A mobile dashboard with projects and progress", style: ReferenceLayout.Picture(460, 24))
            .Add(Feature("01", "#276acf", "Exclusive Access", "Get early access to premium content, tools, and features that others can't see."))
            .Add(Feature("02", "#c1743c", "Personalized Dashboard", "Enjoy a customized experience tailored to your preferences and needs."))
            .Add(Feature("03", "#3a9476", "Unlimited Resources", "Gain access to a library of tutorials, guides, and resources at your fingertips."))
            .AddSpacer(16)
            .AddButton("Set Up Your Profile", "https://example.com/britflow/profile", style: ReferenceLayout.Action("#45574b", radius: 32))
            .AddParagraph("Thank you for joining us. If you have any questions, don't hesitate to reach out to our support team.", style: ReferenceLayout.Text(12, "#64748b", centered: true)))
        .Add(ReferenceLayout.Footer("britflow", "#45574b", "#ffffff"));

    private static Row Feature(string number, string color, string title, string text) => new Row().WithMobileColumns(2).WithGap(12)
        .WithStyle(new ComponentStyle { Padding = new Spacing(12), BackgroundColor = "#fafbfa" })
        .AddParagraph(number, style: ReferenceLayout.Text(20, color, centered: true, weight: 700) with { Width = EmailWidth.Percent(18) })
        .AddFrame(copy => copy.WithStyle(new ComponentStyle { Width = EmailWidth.Percent(82) })
            .AddH2(title, style: ReferenceLayout.Text(14, weight: 700))
            .AddParagraph(text, style: ReferenceLayout.Text(12, "#64748b")));
}

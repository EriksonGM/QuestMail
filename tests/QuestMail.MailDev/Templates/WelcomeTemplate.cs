using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class WelcomeTemplate : IEmailTemplate
{
    public string Name => "welcome";
    public string Subject => "Welcome to QuestMail";

    public Body CreateBody() => TemplateLayout.Create("Welcome aboard!")
        .AddParagraph("Olá, Ada & equipo! Your account is ready. Let's build your first email.")
        .AddFrame(frame => frame.WithStyle(TemplateLayout.Card)
            .AddH2("Your first steps")
            .AddParagraph("1. Complete your profile.\n2. Explore the dashboard.\n3. Invite your team."))
        .AddSpacer(24)
        .AddButton("Open your dashboard", "https://example.com/dashboard")
        .AddDivider()
        .AddParagraph("Need help? Reply to support@example.com.")
        .AddLink("Getting started guide", "https://example.com/guide");
}

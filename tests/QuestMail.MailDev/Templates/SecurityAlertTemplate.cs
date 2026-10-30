using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class SecurityAlertTemplate : IEmailTemplate
{
    public string Name => "security-alert";
    public string Subject => "New sign-in to your account";

    public Body CreateBody() => TemplateLayout.Create("Was this you?")
        .AddParagraph("We detected a new sign-in to your sample account.")
        .AddFrame(frame => frame.WithStyle(TemplateLayout.Card)
            .AddH2("Sign-in details")
            .AddParagraph("Device: Example browser on Windows\nLocation: Lisbon, Portugal\nIP address: 192.0.2.42"))
        .AddSpacer(24)
        .AddParagraph("If this was you, no action is needed. Otherwise, review your account security.")
        .AddButton("Review account activity", "https://example.com/security/activity")
        .AddDivider()
        .AddParagraph("This is a test notification. No real account activity has occurred.")
        .AddLink("Security help", "https://example.com/security/help");
}

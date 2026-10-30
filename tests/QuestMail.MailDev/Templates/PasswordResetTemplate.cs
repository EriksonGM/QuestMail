using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class PasswordResetTemplate : IEmailTemplate
{
    public string Name => "password-reset";
    public string Subject => "Reset your password";

    public Body CreateBody() => TemplateLayout.Create("Reset your password")
        .AddParagraph("Hi Ada, we received a request to reset your password.")
        .AddButton("Choose a new password", "https://example.com/reset-password?token=preview-only&source=email")
        .AddSpacer(24)
        .AddFrame(frame => frame.WithStyle(TemplateLayout.Card)
            .AddH2("This link expires in 30 minutes")
            .AddParagraph("This is a local test token, not a real password reset link."))
        .AddDivider()
        .AddParagraph("If you did not request this change, you can safely ignore this message.")
        .AddLink("Contact support", "mailto:support@example.com");
}

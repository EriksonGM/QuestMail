using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class MintifyAppWelcomeTemplate : IEmailTemplate
{
    public string Name => "mintify-app-welcome";
    public string Subject => "Mintify — welcome to the mobile app";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("welcome-purple");

    public Body CreateBody() => ReferenceLayout.Create("#e7e6f1", 18)
        .AddImage(ReferenceLayout.Cid("welcome-purple"), "A welcoming illustration with a woman holding a pink welcome banner", style: ReferenceLayout.Picture(600))
        .AddFrame(main => main.WithStyle(ReferenceLayout.Section(padding: 36)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddH1("Hi Nizam,", style: ReferenceLayout.Text(20, centered: true, weight: 700))
            .AddParagraph("Welcome to Mobile App! We're excited to have you on board. Whether you're here to explore the app's features or start something new, you're in the right place to get the most out of your experience.",
                style: ReferenceLayout.Text(14, centered: true))
            .AddButton("Explore Now!", "https://example.com/mintify/app", style: ReferenceLayout.Action("#ee568a", radius: 32))
            .AddDivider("#eeeeee")
            .AddH2("Need Help?", style: ReferenceLayout.Text(16, centered: true, weight: 700))
            .AddParagraph("Our support team is always ready to assist. You can reach us directly from the app or visit our Help Center.", style: ReferenceLayout.Text(12, "#64748b", centered: true)))
        .Add(ReferenceLayout.Footer("Mintify", "#4b37af", "#ffffff"));
}

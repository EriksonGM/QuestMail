using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class KootaAppWelcomeTemplate : IEmailTemplate
{
    public string Name => "koota-app-welcome";
    public string Subject => "Koota — you're welcome!";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("rocket");

    public Body CreateBody() => ReferenceLayout.Create("#f0eaf0", 18)
        .AddFrame(main => main.WithStyle(ReferenceLayout.Section("#ec3d93", 28)).WithMobileStyle(ReferenceLayout.MobileSection)
            .Add(ReferenceLayout.Brand("Koota", "#ffffff"))
            .AddFrame(card => card.WithStyle(ReferenceLayout.Section("#ffffff", 32) with { BorderRadius = 16 })
                .WithMobileStyle(ReferenceLayout.MobileSection)
                .AddImage(ReferenceLayout.Cid("rocket"), "A small rocket ready to launch", style: ReferenceLayout.Picture(160, 16))
                .AddH1("You're Welcome!", style: ReferenceLayout.Text(28, centered: true, weight: 700))
                .AddParagraph("Here at Company Name, our goal is to provide you with the best experience and help you get the most out of our services. Whether you're here to explore or make the most of your productivity, you're in great hands!",
                    style: ReferenceLayout.Text(13, centered: true))
                .AddButton("Explore Now!", "https://example.com/koota/explore", style: ReferenceLayout.Action("#ec3d93", radius: 4))))
        .Add(ReferenceLayout.Footer("Koota", "#f7f7f7"));
}

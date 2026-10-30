using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class MintifyOrderConfirmationTemplate : IEmailTemplate
{
    public string Name => "mintify-order-confirmation";
    public string Subject => "Mintify — thank you for your order";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("package-blue", "shirt-green", "shirt-blue");

    public Body CreateBody() => ReferenceLayout.Create("#e7edf5", 16)
        .AddFrame(header => header.WithStyle(ReferenceLayout.Section(padding: 18)).Add(ReferenceLayout.Brand("Mintify", ReferenceLayout.Blue)))
        .AddFrame(hero => hero.WithStyle(ReferenceLayout.Section("#eaf1ff", 28)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddImage(ReferenceLayout.Cid("package-blue"), "A confirmed parcel with a blue check mark", style: ReferenceLayout.Picture(210, 12))
            .AddH1("Thank you for order!", style: ReferenceLayout.Text(26, centered: true, weight: 700))
            .AddParagraph("Your order has been confirmed and is\ncurrently being processed.", style: ReferenceLayout.Text(13, centered: true))
            .AddButton("Track your order", "https://example.com/orders/3438", style: ReferenceLayout.Action()))
        .Add(ReferenceLayout.OrderItems())
        .Add(ReferenceLayout.Support())
        .Add(ReferenceLayout.Footer("Mintify", "#eaf1ff"));
}

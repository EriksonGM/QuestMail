using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class AattioOrderConfirmationTemplate : IEmailTemplate
{
    public string Name => "aattio-order-confirmation";
    public string Subject => "Aattio — your order is confirmed";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("package-green", "shirt-green", "shirt-blue");

    public Body CreateBody() => ReferenceLayout.Create("#edf1e8", 18)
        .AddFrame(header => header.WithStyle(ReferenceLayout.Section(padding: 16)).Add(ReferenceLayout.Brand("aattio", "#244c26")))
        .AddFrame(hero => hero.WithStyle(ReferenceLayout.Section(ReferenceLayout.Green, 28)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddImage(ReferenceLayout.Cid("package-green"), "Your confirmed parcel", style: ReferenceLayout.Picture(190, 12))
            .AddH1("Your Order is Confirmed!", style: ReferenceLayout.Text(26, "#142513", centered: true, weight: 700))
            .AddParagraph("Thank you for shopping at OnlineShop!", style: ReferenceLayout.Text(13, "#142513", centered: true))
            .AddButton("Track your Order", "https://example.com/orders/3438", style: ReferenceLayout.Action("#111111", radius: 32)))
        .AddFrame(note => note.WithStyle(ReferenceLayout.Section(padding: 24))
            .AddParagraph("Your order has been confirmed and is currently being processed.\nHere are the details of your purchase:", style: ReferenceLayout.Text(13, centered: true)))
        .Add(ReferenceLayout.OrderItems("Your item in this order"))
        .Add(ReferenceLayout.Support())
        .Add(ReferenceLayout.Footer("aattio", ReferenceLayout.Green, "#244c26"));
}

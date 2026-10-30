using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class StracShippingUpdateTemplate : IEmailTemplate
{
    public string Name => "strac-shipping-update";
    public string Subject => "Strac — your shipment is on the road";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("shirt-green", "shirt-blue");

    public Body CreateBody() => ReferenceLayout.Create("#eceaf8")
        .AddFrame(hero => hero.WithStyle(ReferenceLayout.Section(ReferenceLayout.Purple, 32)).WithMobileStyle(ReferenceLayout.MobileSection)
            .Add(ReferenceLayout.Brand("strac", "#ffffff"))
            .AddH1("Your shipment is\non the road!", style: ReferenceLayout.Text(30, "#ffffff", centered: true, weight: 700))
            .AddParagraph("Your order has been confirmed and is currently\nbeing carefully processed for shipment.", style: ReferenceLayout.Text(13, "#ffffff", centered: true))
            .AddFrame(status => status.WithStyle(ReferenceLayout.Section("#ffffff", 16) with { BorderRadius = 8 })
                .AddRow(row => row.WithMobileColumns(3).WithGap(8)
                    .AddParagraph("1\nConfirmed", style: ReferenceLayout.Text(12, "#07866d", centered: true, weight: 700))
                    .AddParagraph("2\nShipping", style: ReferenceLayout.Text(12, "#07866d", centered: true, weight: 700))
                    .AddParagraph("3\nDelivered", style: ReferenceLayout.Text(12, "#64748b", centered: true))))
            .AddSpacer(24)
            .AddButton("Track your Order", "https://example.com/shipping/3438", style: ReferenceLayout.Action("#ffdc43", "#17202c", 32)))
        .Add(ReferenceLayout.OrderItems("The item in this order"))
        .Add(ReferenceLayout.Support())
        .Add(ReferenceLayout.Footer("strac", ReferenceLayout.Purple, "#ffffff"));
}

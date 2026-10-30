using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class FashionWarehouseSaleTemplate : IEmailTemplate
{
    public string Name => "fashion-warehouse-sale";
    public string Subject => "Online warehouse sale — up to 80% off";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.Images("fashion-sale");

    public Body CreateBody() => ReferenceLayout.Create("#25251d", 12)
        .AddImage(ReferenceLayout.Cid("fashion-sale"), "Up to 80% off. Online warehouse sale, with a fashion illustration in a grand stone archway.",
            style: ReferenceLayout.Picture(600), linkUrl: "https://example.com/fashion/sale")
        .AddFrame(action => action.WithStyle(ReferenceLayout.Section("#171612", 28)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddButton("SHOP NOW", "https://example.com/fashion/sale", style: ReferenceLayout.Action("#ffffff", "#171612", 0))
            .AddParagraph("Discover the archive collection. Selected styles, limited availability.", style: ReferenceLayout.Text(12, "#ffffff", centered: true)));
}

using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class SaraSummerSaleTemplate : IEmailTemplate
{
    public string Name => "sara-summer-sale";
    public string Subject => "SARA — summer sale, up to 10% off";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("summer-sale");

    public Body CreateBody() => ReferenceLayout.Create("#eeece5", 16)
        .AddFrame(header => header.WithStyle(ReferenceLayout.Section(padding: 24))
            .Add(ReferenceLayout.Brand("S A R A", "#63418a"))
            .AddParagraph("MEN     WOMEN     NEW ARRIVALS     BOYS     GIRLS", style: ReferenceLayout.Text(10, "#46305e", centered: true)))
        .AddFrame(banner => banner.WithStyle(ReferenceLayout.Section("#e9164b", 12))
            .AddParagraph("Summer Sale Started! Use Code SUM24", style: ReferenceLayout.Text(13, "#ffffff", centered: true)))
        .AddImage(ReferenceLayout.Cid("summer-sale"), "Summer sale: up to 10% off, with a sun and beach umbrella", style: ReferenceLayout.Picture(600))
        .AddFrame(actions => actions.WithStyle(ReferenceLayout.Section(padding: 24)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddRow(row => row.WithGap(16).WithMobileColumns(1)
                .AddButton("SHOP MEN'S", "https://example.com/sara/men", style: ReferenceLayout.Action("#63418a", radius: 0))
                .AddButton("SHOP WOMEN'S", "https://example.com/sara/women", style: ReferenceLayout.Action("#63418a", radius: 0))))
        .Add(ReferenceLayout.Footer("SARA", "#f8f8f8"));
}

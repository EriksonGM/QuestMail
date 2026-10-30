using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class OrderConfirmationTemplate : IEmailTemplate
{
    public string Name => "order-confirmation";
    public string Subject => "Order #QM-2026-0042 confirmed";

    public Body CreateBody() => TemplateLayout.Create("Thanks for your order")
        .AddParagraph("Order #QM-2026-0042 is confirmed. All prices and addresses below are sample data.")
        .AddFrame(frame => frame.WithStyle(TemplateLayout.Card)
            .AddH2("Order summary")
            .AddRow(row => row.WithGap(16).WithMobileColumns(1)
                .AddParagraph("Email design handbook × 1")
                .AddParagraph("EUR 29.00"))
            .AddRow(row => row.WithGap(16).WithMobileColumns(1)
                .AddParagraph("Template starter pack × 2")
                .AddParagraph("EUR 20.00"))
            .AddDivider()
            .AddH3("Total: EUR 49.00"))
        .AddSpacer(24)
        .AddRow(row => row.WithGap(16).WithMobileColumns(1)
            .AddFrame(address => address.AddH2("Delivery address")
                .AddParagraph("Ada Example\n42 Sample Street\nLisbon, Portugal"))
            .AddFrame(delivery => delivery.AddH2("Estimated delivery")
                .AddParagraph("3–5 business days\nStandard shipping")))
        .AddButton("View your order", "https://example.com/orders/QM-2026-0042");
}

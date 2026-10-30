using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class PluginProductsNewsletterTemplate : IEmailTemplate
{
    public string Name => "plugin-products-newsletter";
    public string Subject => "Discover tools that make your website better";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("sailboat", "plugin-purple", "plugin-pink", "plugin-orange");

    public Body CreateBody() => ReferenceLayout.Create("#e6f0eb", 12)
        .AddImage(ReferenceLayout.Cid("sailboat"), "An orange sailboat on a turquoise sea", style: ReferenceLayout.Picture(600))
        .AddFrame(intro => intro.WithStyle(ReferenceLayout.Section()).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddParagraph("Want to show before-and-after photos on your WordPress site?\nHighlight subtle differences between images!", style: ReferenceLayout.Text(14, centered: true))
            .AddButton("CLAIM YOURS", "https://example.com/plugins", style: ReferenceLayout.Action("#ef4638", radius: 0))
            .AddSpacer(24)
            .AddH1("Our Products", style: ReferenceLayout.Text(25, centered: true, weight: 700))
            .Add(Product("ElementsKit", "plugin-purple", "An all-in-one toolkit for Elementor. Build expressive pages with reusable widgets and templates."))
            .Add(Product("ShopEngine", "plugin-pink", "Create a better store experience with flexible product, cart and checkout layouts."))
            .Add(Product("WP Social", "plugin-orange", "Connect your audience with simple sharing, login and social counters.")))
        .AddFrame(idea => idea.WithStyle(ReferenceLayout.Section()).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddH2("Have Any Groundbreaking Ideas for Our\nElementor Addons?", style: ReferenceLayout.Text(16, centered: true, weight: 700))
            .AddButton("SUBMIT YOUR IDEA", "https://example.com/ideas", style: ReferenceLayout.Action("#ef4638", radius: 0)))
        .Add(ReferenceLayout.Footer("Tools for better websites", "#f4f8f7"));

    private static Row Product(string name, string image, string description) => new Row().WithGap(16).WithMobileColumns(1)
        .WithStyle(new ComponentStyle { Margin = new Spacing(8, 0), Border = new EmailBorder(1, "#eeeeee") })
        .AddImage(ReferenceLayout.Cid(image), name + " product illustration", style: ReferenceLayout.Picture(150) with { Width = EmailWidth.Fixed(150) }, mobileStyle: ReferenceLayout.Picture(150))
        .AddFrame(copy => copy.WithStyle(new ComponentStyle { Padding = new Spacing(16) })
            .AddH2(name, style: ReferenceLayout.Text(17, weight: 700))
            .AddParagraph(description, style: ReferenceLayout.Text(12, "#64748b"))
            .AddLink("Try a live demo", "https://example.com/plugins/" + name.ToLowerInvariant(), style: ReferenceLayout.Text(12, "#cb3e32")));
}

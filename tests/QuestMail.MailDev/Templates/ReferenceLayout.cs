using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

internal static class ReferenceLayout
{
    public const string Blue = "#0757df";
    public const string Purple = "#6654d6";
    public const string Green = "#b8ff6b";

    public static Body Create(string canvas = "#e8e8e8", double radius = 0) => new Body()
        .WithLanguage("en").WithCanvasColor(canvas)
        .WithStyle(new ComponentStyle
        {
            Width = EmailWidth.Max(600), BackgroundColor = "#ffffff", Padding = new Spacing(0),
            Margin = new Spacing(24, 12), BorderRadius = radius,
            Typography = new TypographyStyle { FontFamily = "Arial,Helvetica,sans-serif", FontSize = 14, Color = "#17202c", LineHeight = 1.5 }
        })
        .WithMobileStyle(new ComponentStyle { Margin = new Spacing(12, 8) });

    public static ComponentStyle Section(string color = "#ffffff", double padding = 32) => new()
    {
        BackgroundColor = color, Padding = new Spacing(padding)
    };

    public static ComponentStyle MobileSection { get; } = new() { Padding = new Spacing(24, 16) };

    public static ComponentStyle Text(double size = 14, string color = "#17202c", bool centered = false, int weight = 400) => new()
    {
        Margin = new Spacing(0, 0, 16, 0),
        Typography = new TypographyStyle
        {
            FontSize = size, Color = color, FontWeight = weight, LineHeight = 1.5,
            TextAlignment = centered ? ElementAlignEnum.Center : ElementAlignEnum.Left
        }
    };

    public static ComponentStyle Action(string color = Blue, string text = "#ffffff", double radius = 4) => new()
    {
        BackgroundColor = color, BorderRadius = radius, Padding = new Spacing(12, 24),
        HorizontalAlignment = ElementAlignEnum.Center,
        Typography = new TypographyStyle { Color = text, FontSize = 13, FontWeight = 700, LineHeight = 1.5 }
    };

    public static ComponentStyle Picture(double width, double bottom = 0) => new()
    {
        Width = EmailWidth.Max(width), HorizontalAlignment = ElementAlignEnum.Center, Margin = new Spacing(0, 0, bottom, 0)
    };

    public static string Cid(string name) => new EmailInlineImage(name).Source;
    public static IReadOnlyList<EmailInlineImage> Images(params string[] names) => names.Select(name => new EmailInlineImage(name)).ToArray();

    public static Paragraph Brand(string name, string color = "#17202c") => new(name)
    {
        Style = Text(19, color, centered: true, weight: 700)
    };

    public static Row SocialLinks() => new Row(
        new Image(Cid("social-facebook"), "Facebook") { LinkUrl = "https://example.com/social/facebook", Style = new ComponentStyle { Width = EmailWidth.Fixed(44) } },
        new Image(Cid("social-instagram"), "Instagram") { LinkUrl = "https://example.com/social/instagram", Style = new ComponentStyle { Width = EmailWidth.Fixed(44) } },
        new Image(Cid("social-linkedin"), "LinkedIn") { LinkUrl = "https://example.com/social/linkedin", Style = new ComponentStyle { Width = EmailWidth.Fixed(44) } })
    {
        MobileColumns = 3, Gap = 0,
        Style = new ComponentStyle { Width = EmailWidth.Fixed(132), HorizontalAlignment = ElementAlignEnum.Center }
    };

    public static IReadOnlyList<EmailInlineImage> WithSocial(params string[] names) =>
        Images(names.Concat(["social-facebook", "social-instagram", "social-linkedin"]).ToArray());

    public static Frame Footer(string brand, string color = "#f6f8fa", string text = "#475569") => new Frame()
        .WithStyle(Section(color, 24)).WithMobileStyle(MobileSection)
        .Add(Brand(brand, text))
        .AddParagraph("42 Sample Street, New York, NY 10001\nSample email for local preview only.", style: Text(10, text, centered: true))
        .Add(SocialLinks())
        .AddLink("Unsubscribe / preferences", "https://example.com/preferences",
            style: Text(10, text, centered: true) with { Margin = new Spacing(8, 0, 0, 0) });

    public static Frame Support() => new Frame()
        .WithStyle(Section("#ffffff", 24)).WithMobileStyle(MobileSection)
        .AddH2("Need help with your order?", style: Text(17, centered: true, weight: 700))
        .AddParagraph("Let us know, and we'll make it right.", style: Text(12, "#475569", centered: true))
        .AddRow(row => row.WithGap(16).WithMobileColumns(1)
            .AddFrame(email => email.WithStyle(Section("#f7f8fa", 12))
                .AddParagraph("Email", style: Text(11) with { Margin = new Spacing(0) })
                .AddLink("hello@example.com", "mailto:hello@example.com", style: Text(12)))
            .AddFrame(phone => phone.WithStyle(Section("#f7f8fa", 12))
                .AddParagraph("Phone", style: Text(11) with { Margin = new Spacing(0) })
                .AddLink("+1 202 555 0142", "tel:+12025550142", style: Text(12))));

    public static Frame OrderItems(string heading = "The item you ordered") => new Frame()
        .WithStyle(Section("#ffffff", 24)).WithMobileStyle(MobileSection)
        .AddH2(heading, style: Text(19, centered: true, weight: 700))
        .AddParagraph("Order number: #3438", style: Text(11, "#475569", centered: true))
        .Add(OrderItem("shirt-green", "Single jersey knitted cotton polo", "Bottle green / L", "$350.00"))
        .Add(OrderItem("shirt-blue", "Single jersey knitted cotton polo", "Cloud blue / M", "$700.00"))
        .Add(Total("Subtotal", "$1,050.00"))
        .Add(Total("Standard delivery", "$20.00"))
        .AddDivider("#e5e7eb", style: new ComponentStyle { Margin = new Spacing(8, 0) })
        .Add(Total("Total", "$1,070.00", true))
        .AddSpacer(12)
        .AddRow(row => row.WithGap(16).WithMobileColumns(1)
            .AddFrame(address => address.WithStyle(Section("#f8f9fa", 12))
                .AddH3("Shipping address", style: Text(12, weight: 700))
                .AddParagraph("Ada Example\n42 Sample Street\nNew York, NY 10001", style: Text(11)))
            .AddFrame(address => address.WithStyle(Section("#f8f9fa", 12))
                .AddH3("Billing address", style: Text(12, weight: 700))
                .AddParagraph("Ada Example\n42 Sample Street\nNew York, NY 10001", style: Text(11))));

    private static Row OrderItem(string image, string name, string detail, string price) => new Row()
        .WithGap(12).WithMobileColumns(2)
        .WithStyle(new ComponentStyle { Padding = new Spacing(12), BackgroundColor = "#fafafa", Margin = new Spacing(0, 0, 8, 0) })
        .AddImage(Cid(image), name, style: Picture(64) with { Width = EmailWidth.Fixed(64) }, mobileStyle: Picture(64))
        .AddFrame(item => item.WithStyle(new ComponentStyle { Width = EmailWidth.Percent(80) })
            .AddParagraph(name, style: Text(12, weight: 700))
            .AddParagraph($"{detail}\nQuantity: 1\n{price}", style: Text(11)));

    private static Row Total(string name, string price, bool bold = false) => new Row()
        .WithMobileColumns(2).WithGap(12)
        .AddParagraph(name, style: Text(12, weight: bold ? 700 : 400) with { Margin = new Spacing(4, 0) })
        .AddParagraph(price, style: Text(12, weight: bold ? 700 : 400) with
        {
            Margin = new Spacing(4, 0), Typography = Text(12, weight: bold ? 700 : 400).Typography! with { TextAlignment = ElementAlignEnum.Right }
        });
}

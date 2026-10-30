using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class CustomCssTemplate : IEmailTemplate
{
    public string Name => "custom-css";
    public string Subject => "Custom inline and mobile CSS";

    public Body CreateBody() => TemplateLayout.Create("Custom CSS preview")
        .AddParagraph("Inspect this email at desktop and mobile widths. The plain-text alternative keeps the original text.")
        .AddFrame(frame => frame
            .WithStyle(TemplateLayout.Card with
            {
                CustomCss = CssDeclarations.Create(("border-style", "dashed"), ("border-color", "#2563eb"))
            })
            .WithMobileStyle(new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("border-style", "solid"), ("border-color", "#15803d"))
            })
            .AddHeading("A heading with custom CSS", style: new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("text-transform", "uppercase"), ("letter-spacing", "1px"), ("color", "#2563eb"))
            }, mobileStyle: new ComponentStyle
            {
                Typography = new TypographyStyle { Color = "#15803d", FontSize = 24 },
                CustomCss = CssDeclarations.Create(("text-transform", "none"), ("letter-spacing", "0px"))
            })
            .AddParagraph("The desktop heading is blue and uppercase. On mobile it is green and uses the original letter case.")
            .AddButton("View documentation", "https://example.com/custom-css", style: new ComponentStyle
            {
                CustomCss = CssDeclarations.Create(("text-transform", "uppercase"), ("word-spacing", "2px"))
            }))
        .AddDivider()
        .AddParagraph("CSS support varies between email clients. Use typed styles for layout and inherited typography.");
}

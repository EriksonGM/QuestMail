using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

namespace QuestMail.MailDev.Templates;

public sealed class NewsletterTemplate : IEmailTemplate
{
    public string Name => "newsletter";
    public string Subject => "Your monthly QuestMail digest";

    public Body CreateBody() => TemplateLayout.Create("The QuestMail digest")
        .AddParagraph("Two ideas for better emails, ready to explore on desktop and mobile.")
        .AddRow(row => row.WithGap(16).WithMobileColumns(1)
            .AddFrame(article => article.WithStyle(TemplateLayout.Card)
                .AddH2("Design for every screen")
                .AddParagraph("Keep content readable with responsive rows, generous spacing and clear headings.")
                .AddLink("Read the responsive guide", "https://example.com/articles/responsive"))
            .AddFrame(article => article.WithStyle(TemplateLayout.Card)
                .AddH2("Do not forget plain text")
                .AddParagraph("A useful plain-text alternative keeps your message accessible without HTML.")
                .AddLink("Explore plain-text emails", "https://example.com/articles/plain-text")))
        .AddSpacer(24)
        .AddButton("Browse all articles", "https://example.com/articles")
        .AddDivider()
        .AddParagraph("You are receiving this sample digest because this is a local preview.")
        .AddLink("Unsubscribe (preview only)", "https://example.com/unsubscribe?subscriber=preview");
}

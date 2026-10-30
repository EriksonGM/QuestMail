using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class MintifyCloudRegistrationTemplate : IEmailTemplate
{
    public string Name => "mintify-cloud-registration";
    public string Subject => "You're registered — Cloud Storage";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("cloud-person");

    public Body CreateBody() => ReferenceLayout.Create("#e9edf1", 18)
        .AddFrame(hero => hero.WithStyle(ReferenceLayout.Section("#e9efff", 24)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddRow(row => row.WithGap(12).WithMobileColumns(1)
                .AddFrame(title => title.WithStyle(new ComponentStyle { Padding = new Spacing(24, 0) })
                    .AddH1("You've registered\nCloud Storage", style: ReferenceLayout.Text(26, weight: 700))
                    .AddParagraph("A little more space for big ideas.", style: ReferenceLayout.Text(12, "#3869aa")))
                .AddImage(ReferenceLayout.Cid("cloud-person"), "A smiling person beside a cloud illustration", style: ReferenceLayout.Picture(240))))
        .AddFrame(main => main.WithStyle(ReferenceLayout.Section()).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddH2("Hi Nizam,", style: ReferenceLayout.Text(16, weight: 700))
            .AddParagraph("We're thrilled to let you know that you've officially registered and already started using Cloud Storage. Now you can access it with ease, and we have some awesome tools, exclusive cloud storage and all-new files and accessible anytime, anywhere.", style: ReferenceLayout.Text(14))
            .AddParagraph("If you have any questions or need assistance, our support team is always here for you. We can't wait to see you discover the tips and tools in action.", style: ReferenceLayout.Text(14))
            .AddLink("Google calendar / Other services", "https://example.com/cloud/integrations", style: ReferenceLayout.Text(12, ReferenceLayout.Blue))
            .AddButton("Get Started Now!", "https://example.com/cloud/start", style: ReferenceLayout.Action())
            .AddParagraph("See you there,\nThe Cloud Storage Team", style: ReferenceLayout.Text(13))
            .AddDivider("#eeeeee")
            .AddRow(row => row.WithGap(8).WithMobileColumns(1)
                .AddLink("Learn more", "https://example.com/cloud/learn", style: ReferenceLayout.Text(12, ReferenceLayout.Blue, centered: true))
                .AddLink("Read blog", "https://example.com/cloud/blog", style: ReferenceLayout.Text(12, "#b76a37", centered: true))
                .AddLink("Contact", "mailto:help@example.com", style: ReferenceLayout.Text(12, "#14815d", centered: true))))
        .Add(ReferenceLayout.Footer("Mintify", "#f7faf9"));
}

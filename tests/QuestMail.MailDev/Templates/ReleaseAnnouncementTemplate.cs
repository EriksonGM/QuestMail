using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class ReleaseAnnouncementTemplate : IEmailTemplate
{
    public string Name => "release-announcement";
    public string Subject => "Version 2.1 — Smart Distribute is here";

    public Body CreateBody() => ReferenceLayout.Create()
        .AddFrame(intro => intro.WithStyle(ReferenceLayout.Section(padding: 40)).WithMobileStyle(ReferenceLayout.MobileSection)
            .Add(ReferenceLayout.Brand("C  craftingemails"))
            .AddParagraph("Hi Candy,\nSay hello to version 2.1 Smart Distribute.\nIntroducing a whole new range of tools to make working with multiple layers quicker and easier.",
                style: ReferenceLayout.Text(17, centered: true))
            .AddButton("Download new release", "https://example.com/releases/2.1", style: ReferenceLayout.Action()))
        .AddFrame(history => history.WithStyle(ReferenceLayout.Section(padding: 40)).WithMobileStyle(ReferenceLayout.MobileSection)
            .Add(Release("2.1.0-alpha06", "May 25, 2020", "Herse Hedman", true))
            .Add(Release("2.1.0-alpha05", "May 22, 2020", "Elise Beverley", false)))
        .AddFrame(footer => footer.WithStyle(ReferenceLayout.Section(padding: 40)).WithMobileStyle(ReferenceLayout.MobileSection)
            .AddDivider("#e5e7eb")
            .AddParagraph("This email was sent to you as a registered member of craftingemails.com. Update your email preferences or read our Terms of Use and Privacy Statement.",
                style: ReferenceLayout.Text(10, "#64748b"))
            .AddLink("Email preferences", "https://example.com/preferences", style: ReferenceLayout.Text(10, ReferenceLayout.Blue))
            .AddParagraph("© 2019 Craftingemails. All rights reserved.", style: ReferenceLayout.Text(10, "#64748b")));

    private static Row Release(string version, string date, string author, bool extended) => new Row()
        .WithGap(20).WithMobileColumns(1)
        .AddFrame(label => label.WithStyle(new ComponentStyle { Width = EmailWidth.Percent(25), Padding = new Spacing(16, 0) })
            .AddLink(version, "https://example.com/releases/" + version, style: ReferenceLayout.Text(12, ReferenceLayout.Blue))
            .AddParagraph("e1019ee", style: ReferenceLayout.Text(9, "#64748b")))
        .AddFrame(details =>
        {
            details.WithStyle(new ComponentStyle
                {
                    Width = EmailWidth.Percent(75), Padding = new Spacing(16, 0, 24, 16),
                    CustomCss = CssDeclarations.Create(("border-left-width", "1px"), ("border-left-style", "solid"), ("border-left-color", "#eeeeee"))
                })
                .AddH2(date, style: ReferenceLayout.Text(17, weight: 700))
                .AddParagraph($"{author}  ·  released this 25 days ago", style: ReferenceLayout.Text(10, "#64748b"))
                .Add(Change("New", "#eaf3ff", ReferenceLayout.Blue, "Add Material Button Toggle Group"))
                .Add(Change("Legacy", "#ecfff6", "#07854d", "Add MotionSpec support to Extended Button"));
            if (extended)
                details.Add(Change("Feature", "#fff8e8", "#9c6200", "Add Extended FAB component"))
                    .Add(Change("Improved", "#fff0f2", "#b91c3c", "Add API to set height of a half expanded view"));
            details.AddLink("View all", "https://example.com/releases/" + version, style: ReferenceLayout.Text(12, ReferenceLayout.Blue, centered: true));
        });

    private static Row Change(string label, string background, string color, string text) => new Row()
        .WithGap(12).WithMobileColumns(2)
        .WithStyle(new ComponentStyle { Margin = new Spacing(6, 0) })
        .AddParagraph(label, style: ReferenceLayout.Text(10, color, centered: true) with
        {
            Width = EmailWidth.Percent(22), BackgroundColor = background, BorderRadius = 12, Padding = new Spacing(3, 5)
        })
        .AddParagraph(text, style: ReferenceLayout.Text(12, "#64748b") with { Width = EmailWidth.Percent(78) });
}

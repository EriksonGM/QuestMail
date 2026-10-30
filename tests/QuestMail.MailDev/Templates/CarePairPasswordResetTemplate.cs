using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

namespace QuestMail.MailDev.Templates;

public sealed class CarePairPasswordResetTemplate : IEmailTemplate
{
    public string Name => "carepair-password-reset";
    public string Subject => "CarePair — reset your password";
    public const string ResetUrl = "https://example.com/carepair/reset-password?token=preview-only";
    public IReadOnlyList<EmailInlineImage> InlineImages => ReferenceLayout.WithSocial("carepair-logo", "carepair-reset", "carepair-cta");

    public Body CreateBody() => ReferenceLayout.Create("#2460e9", 22)
        .WithStyle(ReferenceLayout.Create().Style with { Margin = new Spacing(8), BorderRadius = 22 })
        .WithMobileStyle(new ComponentStyle { Margin = new Spacing(6) })
        .AddFrame(main => main.WithStyle(ReferenceLayout.Section(padding: 36) with
            {
                CustomCss = CssDeclarations.Create(("border-radius", "22px 22px 0 0"))
            })
            .WithMobileStyle(new ComponentStyle { Padding = new Spacing(28, 24, 34, 24) })
            .AddImage(ReferenceLayout.Cid("carepair-logo"), "CarePair", style: ReferenceLayout.Picture(112, 20))
            .AddImage(ReferenceLayout.Cid("carepair-reset"), "An illustrated person with a key beside a blue padlock",
                style: ReferenceLayout.Picture(296, 28), mobileStyle: ReferenceLayout.Picture(240, 24))
            .AddH1("Hi Olivia,", style: ReferenceLayout.Text(18, "#111111", centered: true, weight: 600))
            .AddParagraph("Need to reset password? No problem.\nJust click the button below.",
                style: ReferenceLayout.Text(14, "#111111", centered: true) with { Margin = new Spacing(0, 0, 28, 0) })
            .AddImage(ReferenceLayout.Cid("carepair-cta"), "Reset password", linkUrl: ResetUrl,
                style: ReferenceLayout.Picture(176)))
        .AddFrame(footer => footer.WithStyle(ReferenceLayout.Section("#f6f6f6", 36) with
            {
                CustomCss = CssDeclarations.Create(("border-radius", "0 0 22px 22px"))
            }).WithMobileStyle(new ComponentStyle { Padding = new Spacing(28, 16, 16, 16) })
            .AddParagraph(new[]
            {
                new TextRun("If you did not initiate this request, please contact us at "),
                new TextRun("support@cairpair.com") { Url = "mailto:support@cairpair.com", Typography = new TypographyStyle { Color = ReferenceLayout.Blue } }
            }, style: ReferenceLayout.Text(13, "#525252"))
            .AddParagraph("Thanks,\nThe CarePair Team", style: ReferenceLayout.Text(13, "#525252"))
            .AddSpacer(32, mobileStyle: new ComponentStyle { Height = 20 })
            .AddRow(row => row.WithMobileColumns(2).WithGap(0)
                .AddParagraph("© 2024 CarePair", style: ReferenceLayout.Text(11, "#666666") with
                {
                    Width = EmailWidth.Percent(60), Margin = new Spacing(0), VerticalAlignment = VerticalAlignEnum.Middle
                })
                .AddFrame(social => social.WithStyle(new ComponentStyle { Width = EmailWidth.Percent(40) })
                    .Add(ReferenceLayout.SocialLinks().WithStyle(new ComponentStyle
                    {
                        Width = EmailWidth.Fixed(132), HorizontalAlignment = ElementAlignEnum.Right
                    })))));
}

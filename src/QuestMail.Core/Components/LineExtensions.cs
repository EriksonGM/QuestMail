using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Components;

public static class LineExtensions
{
    public static EmailBuilder AddLine(this EmailBuilder builder, string text,
        LineSizeEnum size = LineSizeEnum.Body, ElementAlignEnum align = ElementAlignEnum.Left)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Line(text, size, align));
    }

    public static EmailBuilder AddH1(this EmailBuilder builder, string text,
        ElementAlignEnum align = ElementAlignEnum.Left) =>
        AddHeading(builder, text, HeadingLevelEnum.H1, align);

    public static EmailBuilder AddH2(this EmailBuilder builder, string text,
        ElementAlignEnum align = ElementAlignEnum.Left) =>
        AddHeading(builder, text, HeadingLevelEnum.H2, align);

    public static EmailBuilder AddH3(this EmailBuilder builder, string text,
        ElementAlignEnum align = ElementAlignEnum.Left) =>
        AddHeading(builder, text, HeadingLevelEnum.H3, align);

    public static EmailBuilder AddParagraph(this EmailBuilder builder, string text,
        ElementAlignEnum align = ElementAlignEnum.Left) =>
        AddParagraphCore(builder, text, align);

    private static ComponentStyle Alignment(ElementAlignEnum align)
    {
        if (!Enum.IsDefined(align))
            throw new ArgumentOutOfRangeException(nameof(align));
        return new ComponentStyle { Typography = new TypographyStyle { TextAlignment = align } };
    }

    private static EmailBuilder AddHeading(EmailBuilder builder, string text, HeadingLevelEnum level, ElementAlignEnum align)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Heading(text, level) { Style = Alignment(align) });
    }

    private static EmailBuilder AddParagraphCore(EmailBuilder builder, string text, ElementAlignEnum align)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Paragraph(text) { Style = Alignment(align) });
    }
}

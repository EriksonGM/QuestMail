using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Internal;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Fluent;

public static class FluentComponentExtensions
{
    public static TSelf AddFrame<TSelf>(this IComponentComposer<TSelf> composer, Action<FrameBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(configure);
        var frame = new FrameBuilder();
        configure(frame);
        return composer.Add(frame.Build());
    }

    public static TSelf AddFrame<TSelf>(this IComponentComposer<TSelf> composer, params EmailComponent[] children)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Frame(children));
    }

    public static TSelf AddRow<TSelf>(this IComponentComposer<TSelf> composer, Action<RowBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(configure);
        var row = new RowBuilder();
        configure(row);
        return composer.Add(row.Build());
    }

    public static TSelf AddRow<TSelf>(this IComponentComposer<TSelf> composer, params EmailComponent[] children)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Row(children));
    }

    public static TSelf AddHeading<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        HeadingLevelEnum level = HeadingLevelEnum.H1, ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Heading(text, level) { Style = style ?? new(), MobileStyle = mobileStyle });
    }

    public static TSelf AddHeading<TSelf>(this IComponentComposer<TSelf> composer, IEnumerable<TextRun> content,
        HeadingLevelEnum level = HeadingLevelEnum.H1, ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Heading(content, level) { Style = style ?? new(), MobileStyle = mobileStyle });
    }

    public static TSelf AddH1<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        ElementAlignEnum? align = null, ComponentStyle? style = null, ComponentStyle? mobileStyle = null) =>
        composer.AddHeading(text, HeadingLevelEnum.H1, Alignment(style, align), mobileStyle);

    public static TSelf AddH2<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        ElementAlignEnum? align = null, ComponentStyle? style = null, ComponentStyle? mobileStyle = null) =>
        composer.AddHeading(text, HeadingLevelEnum.H2, Alignment(style, align), mobileStyle);

    public static TSelf AddH3<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        ElementAlignEnum? align = null, ComponentStyle? style = null, ComponentStyle? mobileStyle = null) =>
        composer.AddHeading(text, HeadingLevelEnum.H3, Alignment(style, align), mobileStyle);

    public static TSelf AddParagraph<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        ElementAlignEnum? align = null, ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Paragraph(text) { Style = Alignment(style, align), MobileStyle = mobileStyle });
    }

    public static TSelf AddParagraph<TSelf>(this IComponentComposer<TSelf> composer, IEnumerable<TextRun> content,
        ElementAlignEnum? align = null, ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Paragraph(content) { Style = Alignment(style, align), MobileStyle = mobileStyle });
    }

    public static TSelf AddLine<TSelf>(this IComponentComposer<TSelf> composer, string text = "",
        LineSizeEnum size = LineSizeEnum.Body, ElementAlignEnum align = ElementAlignEnum.Left,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Line(text, size, align) { Style = style ?? new(), MobileStyle = mobileStyle });
    }

    public static TSelf AddButton<TSelf>(this IComponentComposer<TSelf> composer, string text, string url,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null, string? title = null, bool openInNewWindow = false)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Button(text, url)
        {
            Style = style ?? new(), MobileStyle = mobileStyle, Title = title, OpenInNewWindow = openInNewWindow
        });
    }

    public static TSelf AddLink<TSelf>(this IComponentComposer<TSelf> composer, string text, string url,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null, string? title = null, bool openInNewWindow = false)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Link(text, url)
        {
            Style = style ?? new(), MobileStyle = mobileStyle, Title = title, OpenInNewWindow = openInNewWindow
        });
    }

    public static TSelf AddImage<TSelf>(this IComponentComposer<TSelf> composer, string sourceUrl, string altText,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null, double? height = null, string? linkUrl = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Image(sourceUrl, altText)
        {
            Style = style ?? new(), MobileStyle = mobileStyle, Height = height, LinkUrl = linkUrl
        });
    }

    public static TSelf AddDivider<TSelf>(this IComponentComposer<TSelf> composer, string color = "#d1d5db",
        double thickness = 1, BorderStyleEnum lineStyle = BorderStyleEnum.Solid,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Divider(color, thickness, lineStyle) { Style = style ?? new(), MobileStyle = mobileStyle });
    }

    public static TSelf AddSpacer<TSelf>(this IComponentComposer<TSelf> composer, double height,
        ComponentStyle? style = null, ComponentStyle? mobileStyle = null)
    {
        ArgumentNullException.ThrowIfNull(composer);
        return composer.Add(new Spacer(height) { Style = style ?? new(), MobileStyle = mobileStyle });
    }

    private static ComponentStyle Alignment(ComponentStyle? style, ElementAlignEnum? align)
    {
        if (align is null) return style ?? new();
        Guard.Defined(align.Value, nameof(align));
        return (style ?? new()) with
        {
            Typography = (style?.Typography ?? new()) with { TextAlignment = align }
        };
    }
}

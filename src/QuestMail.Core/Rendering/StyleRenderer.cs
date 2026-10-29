using System.Globalization;
using System.Net;
using System.Text;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Rendering;

internal static class StyleRenderer
{
    public static string Number(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);
    public static string Encode(string text) => WebUtility.HtmlEncode(text);
    public static string Text(string text) => Encode(text).Replace("\n", "<br />");
    public static string Horizontal(ElementAlignEnum? value) => value switch
    {
        ElementAlignEnum.Center => "center",
        ElementAlignEnum.Right => "right",
        _ => "left"
    };
    public static string Vertical(VerticalAlignEnum? value) => value switch
    {
        VerticalAlignEnum.Middle => "middle",
        VerticalAlignEnum.Bottom => "bottom",
        _ => "top"
    };
    public static string Border(EmailBorder border) =>
        $"{Number(border.Thickness)}px {border.Style.ToString().ToLowerInvariant()} {border.Color}";
    public static string Space(Spacing? value)
    {
        var spacing = value ?? default;
        return $"{Number(spacing.Top)}px {Number(spacing.Right)}px {Number(spacing.Bottom)}px {Number(spacing.Left)}px";
    }

    public static TypographyStyle Merge(TypographyStyle parent, TypographyStyle? value) => value is null ? parent : new()
    {
        FontFamily = value.FontFamily ?? parent.FontFamily,
        FontSize = value.FontSize ?? parent.FontSize,
        FontWeight = value.FontWeight ?? parent.FontWeight,
        Color = value.Color ?? parent.Color,
        LineHeight = value.LineHeight ?? parent.LineHeight,
        LetterSpacing = value.LetterSpacing ?? parent.LetterSpacing,
        TextAlignment = value.TextAlignment ?? parent.TextAlignment,
        Italic = value.Italic ?? parent.Italic,
        Decoration = value.Decoration ?? parent.Decoration,
        WhiteSpace = value.WhiteSpace ?? parent.WhiteSpace,
        WordWrap = value.WordWrap ?? parent.WordWrap
    };

    public static ComponentStyle Merge(ComponentStyle parent, ComponentStyle? value) => value is null ? parent : new()
    {
        Width = value.Width ?? parent.Width,
        MaxWidth = value.MaxWidth ?? parent.MaxWidth,
        Height = value.Height ?? parent.Height,
        Margin = value.Margin ?? parent.Margin,
        Padding = value.Padding ?? parent.Padding,
        BackgroundColor = value.BackgroundColor ?? parent.BackgroundColor,
        Border = value.Border ?? parent.Border,
        BorderRadius = value.BorderRadius ?? parent.BorderRadius,
        HorizontalAlignment = value.HorizontalAlignment ?? parent.HorizontalAlignment,
        VerticalAlignment = value.VerticalAlignment ?? parent.VerticalAlignment,
        Typography = Merge(parent.Typography ?? new(), value.Typography),
        Visible = value.Visible ?? parent.Visible
    };

    public static ComponentStyle Defaults(EmailComponent component)
    {
        var style = new ComponentStyle { Width = EmailWidth.Percent(100), Visible = true };
        return component switch
        {
            Body => style with
            {
                Width = EmailWidth.Max(600), BackgroundColor = "#ffffff", Padding = new Spacing(24),
                HorizontalAlignment = ElementAlignEnum.Center
            },
            Heading heading => TextDefaults(style, (int)heading.Level),
            Paragraph => style with { Margin = new Spacing(0, 0, 16, 0) },
            Link => style with
            {
                Margin = new Spacing(0, 0, 16, 0),
                Typography = new TypographyStyle { Color = "#1d4ed8", Decoration = TextDecorationEnum.Underline }
            },
            Line line => TextDefaults(style, (int)line.Size, line.Align),
            Button => style with
            {
                Width = EmailWidth.Auto, BackgroundColor = "#1d4ed8", Padding = new Spacing(12, 24),
                BorderRadius = 4, Margin = new Spacing(0, 0, 16, 0),
                Typography = new TypographyStyle { Color = "#ffffff", FontWeight = 700, Decoration = TextDecorationEnum.None }
            },
            Image image => style with { Width = EmailWidth.Auto, Height = image.Height },
            Spacer spacer => style with { Height = spacer.Height },
            Divider => style with { Margin = new Spacing(16, 0) },
            _ => style
        };
    }

    private static ComponentStyle TextDefaults(ComponentStyle style, int level, ElementAlignEnum? align = null) => style with
    {
        Margin = new Spacing(0, 0, 16, 0),
        Typography = new TypographyStyle
        {
            FontSize = level switch { 0 => null, 1 => 32, 2 => 24, _ => 20 },
            FontWeight = level == 0 ? null : 700, TextAlignment = align
        }
    };

    public static string Typography(TypographyStyle style)
    {
        var css = new StringBuilder();
        if (style.FontFamily is { } family) css.Append($"font-family:{family};");
        if (style.FontSize is { } size) css.Append($"font-size:{Number(size)}px;");
        if (style.FontWeight is { } weight) css.Append($"font-weight:{weight};");
        if (style.Color is { } color) css.Append($"color:{color};");
        if (style.LineHeight is { } height) css.Append($"line-height:{Number(height)};");
        if (style.LetterSpacing is { } spacing) css.Append($"letter-spacing:{Number(spacing)}px;");
        if (style.TextAlignment is { } align) css.Append($"text-align:{Horizontal(align)};");
        if (style.Italic is { } italic) css.Append($"font-style:{(italic ? "italic" : "normal")};");
        if (style.Decoration is { } decoration)
            css.Append($"text-decoration:{decoration switch { TextDecorationEnum.Underline => "underline", TextDecorationEnum.LineThrough => "line-through", _ => "none" }};");
        if (style.WhiteSpace is { } whiteSpace)
            css.Append($"white-space:{whiteSpace switch { WhiteSpaceEnum.NoWrap => "nowrap", WhiteSpaceEnum.PreWrap => "pre-wrap", _ => "normal" }};");
        // word-wrap aliases overflow-wrap; the legacy fallback must come first.
        if (style.WordWrap is { } wrap) css.Append($"word-wrap:{(wrap ? "break-word" : "normal")};overflow-wrap:{(wrap ? "anywhere" : "normal")};");
        return css.ToString();
    }

    public static double? Maximum(ComponentStyle style)
    {
        var widthMax = style.Width is { Mode: WidthModeEnum.Max } width ? width.Value : (double?)null;
        return widthMax is { } max && style.MaxWidth is { } explicitMax ? Math.Min(max, explicitMax) : widthMax ?? style.MaxWidth;
    }

    public static string Width(ComponentStyle style)
    {
        var width = style.Width ?? EmailWidth.Auto;
        var value = width.Mode switch
        {
            WidthModeEnum.Fixed => $"{Number(Math.Min(width.Value, style.MaxWidth ?? width.Value))}px",
            WidthModeEnum.Max => "100%",
            WidthModeEnum.Percent => $"{Number(width.Value)}%",
            _ => "auto"
        };
        return $"width:{value};max-width:{(Maximum(style) is { } max ? Number(max) + "px" : "none")};";
    }

    public static string WidthAttribute(ComponentStyle style)
    {
        var width = style.Width ?? EmailWidth.Auto;
        return width.Mode switch
        {
            WidthModeEnum.Fixed => $" width=\"{Number(Math.Min(width.Value, style.MaxWidth ?? width.Value))}\"",
            WidthModeEnum.Percent => $" width=\"{Number(width.Value)}%\"",
            WidthModeEnum.Max => $" width=\"{Number(Maximum(style)!.Value)}\"",
            _ => ""
        };
    }

    public static string Box(ComponentStyle style) =>
        "border-collapse:separate;box-sizing:border-box;float:none;" + Width(style) +
        $"table-layout:{(style.Width is null or { Mode: WidthModeEnum.Auto } ? "auto" : "fixed")};" +
        $"height:{(style.Height is { } height ? Number(height) + "px" : "auto")};" +
        $"background-color:{style.BackgroundColor ?? "transparent"};border:{(style.Border is { } border ? Border(border) : "0")};border-radius:{Number(style.BorderRadius ?? 0)}px;" +
        (style.HorizontalAlignment switch
        {
            ElementAlignEnum.Center => "margin-left:auto;margin-right:auto;",
            ElementAlignEnum.Right => "margin-left:auto;margin-right:0;",
            _ => "margin-left:0;margin-right:auto;"
        });
}

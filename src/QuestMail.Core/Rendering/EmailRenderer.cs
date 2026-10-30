using System.Text;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Internal;
using QuestMail.Core.Styles;
using static QuestMail.Core.Rendering.StyleRenderer;

namespace QuestMail.Core.Rendering;

internal sealed class EmailRenderer
{
    private const string Table = "role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"";
    private readonly StringBuilder _css = new();
    private readonly StringBuilder _plain = new();
    private int _nextId;

    private sealed record Context(TypographyStyle Desktop, TypographyStyle Mobile,
        bool DesktopVisible = true, bool MobileVisible = true);

    public EmailContent Render(Body body, IReadOnlyList<EmailComponent> children)
    {
        var typography = new TypographyStyle
        {
            FontFamily = "Arial,Helvetica,sans-serif", FontSize = 16, FontWeight = 400,
            Color = "#222222", LineHeight = 1.5, TextAlignment = ElementAlignEnum.Left,
            Italic = false, Decoration = TextDecorationEnum.None, WhiteSpace = WhiteSpaceEnum.Normal, WordWrap = true
        };
        var content = RenderBlock(body, new Context(typography, typography), children);
        var responsive = _css.Length == 0 ? "" : $"<style type=\"text/css\">@media screen and (max-width:480px){{{_css}}}</style>";
        var html = $"""
            <!DOCTYPE html>
            <html lang="{Encode(body.Language)}">
            <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            {responsive}
            </head>
            <body style="margin:0;padding:0;background-color:{body.CanvasColor};">
            <table {Table} width="100%" style="width:100%;background-color:{body.CanvasColor};"><tbody><tr><td>{content}</td></tr></tbody></table>
            </body>
            </html>
            """;
        return new EmailContent(html, _plain.ToString());
    }

    private static (ComponentStyle Desktop, ComponentStyle Mobile) Resolve(EmailComponent component, Context parent)
    {
        var defaults = Defaults(component);
        var desktop = Merge(defaults, component.Style, component) with
        {
            Typography = Merge(Merge(parent.Desktop, defaults.Typography), component.Style.Typography),
            Visible = parent.DesktopVisible && component.Style.Visible != false
        };
        var mobile = Merge(desktop, component.MobileStyle, component) with
        {
            Typography = Merge(Merge(Merge(parent.Mobile, defaults.Typography), component.Style.Typography), component.MobileStyle?.Typography),
            Visible = parent.MobileVisible && (component.MobileStyle?.Visible ?? component.Style.Visible) != false
        };
        return (desktop, mobile);
    }

    private string RenderBlock(EmailComponent component, Context parent,
        IReadOnlyList<EmailComponent>? rootChildren = null, bool fillColumn = false)
    {
        var (desktop, mobile) = Resolve(component, parent);
        if (desktop.Visible == false && mobile.Visible == false)
            return "";

        // A child's explicit width allocates its column, not a second percentage inside that column.
        if (fillColumn && component.Style.Width is { Mode: WidthModeEnum.Fixed or WidthModeEnum.Percent })
        {
            desktop = desktop with { Width = EmailWidth.Percent(100) };
            mobile = mobile with { Width = component.MobileStyle?.Width ?? EmailWidth.Percent(100) };
        }

        var id = $"qm-{_nextId++}";
        var context = new Context(desktop.Typography!, mobile.Typography!, desktop.Visible != false, mobile.Visible != false);
        var content = component switch
        {
            Body body => RenderChildren(rootChildren ?? body.Children, context),
            Frame frame => RenderChildren(frame.Children, context),
            Row row => RenderRow(row, id, desktop, mobile, context),
            Heading heading => RenderText($"h{(int)heading.Level}", heading.Content, id, desktop),
            Paragraph paragraph => RenderText("p", paragraph.Content, id, desktop),
            Line line => RenderText(line.Size == LineSizeEnum.Body ? "p" : $"h{(int)line.Size}",
                new[] { new TextRun(line.Text) }, id, desktop),
            Link link => RenderLink(link, id, desktop),
            Button button => RenderButton(button, id, desktop),
            Image image => RenderImage(image, id, desktop),
            Divider divider => RenderDivider(divider, id, desktop),
            Spacer => RenderSpacer(id, desktop, mobile),
            _ => throw new NotSupportedException($"Unknown component: {component.GetType().Name}")
        };

        RegisterResponsive(component, id, desktop, mobile);
        var hidden = desktop.Visible == false ? "display:none;max-height:0;overflow:hidden;mso-hide:all;" : "";
        var padding = component is Button ? $"padding:0;mso-padding-alt:{Space(desktop.Padding)};" : $"padding:{Space(desktop.Padding)};";
        var maximum = Maximum(desktop);
        if (desktop.Width is { Mode: WidthModeEnum.Fixed } fixedWidth && maximum is { } limit)
            maximum = Math.Min(fixedWidth.Value, limit);
        var ghostOpen = maximum is { } max
            ? $"<!--[if mso]><table {Table} width=\"{Number(max)}\" align=\"{Horizontal(desktop.HorizontalAlignment)}\"><tr><td><![endif]-->" : "";
        var ghostClose = maximum is not null ? "<!--[if mso]></td></tr></table><![endif]-->" : "";

        return $"<table {Table} class=\"{id}-wrap\" width=\"100%\" style=\"width:100%;{hidden}\"><tbody><tr>" +
            $"<td class=\"{id}-margin\" align=\"{Horizontal(desktop.HorizontalAlignment)}\" style=\"padding:{Space(desktop.Margin)};\">{ghostOpen}" +
            $"<table {Table} class=\"{id}-box\"{WidthAttribute(desktop)} align=\"{Horizontal(desktop.HorizontalAlignment)}\" style=\"{Encode(PrimaryBox(component, desktop))}\"><tbody><tr>" +
            $"<td class=\"{id}-content\" valign=\"{Vertical(desktop.VerticalAlignment)}\" style=\"{padding}vertical-align:{Vertical(desktop.VerticalAlignment)};\">{content}</td>" +
            $"</tr></tbody></table>{ghostClose}</td></tr></tbody></table>";
    }

    private string RenderChildren(IReadOnlyList<EmailComponent> children, Context context) =>
        string.Concat(children.Select(child => RenderBlock(child, context)));

    private string RenderRow(Row row, string id, ComponentStyle desktop, ComponentStyle mobile, Context context)
    {
        var cells = new StringBuilder();
        for (var index = 0; index < row.Children.Count; index++)
        {
            var child = row.Children[index];
            var (childDesktop, childMobile) = Resolve(child, context);
            var rendered = RenderBlock(child, context, fillColumn: true);
            if (rendered.Length == 0) continue;

            var cellClass = $"{id}-column-{index}";
            var width = child.Style.Width is { Mode: WidthModeEnum.Fixed or WidthModeEnum.Percent }
                ? WidthAttribute(childDesktop) : "";
            var vertical = child.Style.VerticalAlignment ?? desktop.VerticalAlignment;
            var mobileVertical = child.MobileStyle?.VerticalAlignment ?? child.Style.VerticalAlignment ?? mobile.VerticalAlignment;
            var hidden = childDesktop.Visible == false ? "display:none;mso-hide:all;" : "";
            cells.Append($"<td class=\"{id}-column {cellClass}\"{width} valign=\"{Vertical(vertical)}\" style=\"box-sizing:border-box;padding:0 {Number(row.Gap / 2)}px;vertical-align:{Vertical(vertical)};{hidden}\">{rendered}</td>");
            Rule($".{id}-row>tbody>tr>.{id}-column.{cellClass}",
                $"display:{(childMobile.Visible == false ? "none" : "inline-block")};vertical-align:{Vertical(mobileVertical)};");
        }

        Rule($".{id}-row,.{id}-row>tbody,.{id}-row>tbody>tr", "display:block;width:100%;");
        Rule($".{id}-row>tbody>tr>.{id}-column",
            $"display:inline-block;width:{Number(100d / row.MobileColumns)}%;max-width:{Number(100d / row.MobileColumns)}%;box-sizing:border-box;padding:0 {Number(row.Gap / 2)}px {Number(row.Gap)}px;");
        return $"<table {Table} class=\"{id}-row\" width=\"100%\" style=\"width:100%;table-layout:fixed;\"><tbody><tr>{cells}</tr></tbody></table>";
    }

    private string RenderText(string tag, IReadOnlyList<TextRun> runs, string id, ComponentStyle style)
    {
        var html = new StringBuilder();
        foreach (var run in runs)
        {
            var text = Text(run.Text);
            var typography = run.Url is null ? run.Typography : Merge(new TypographyStyle
            {
                Color = "#1d4ed8", Decoration = TextDecorationEnum.Underline
            }, run.Typography);
            var css = typography is null ? "" : $" style=\"{Encode(Typography(typography))}\"";
            html.Append(run.Url is null ? (typography is null ? text : $"<span{css}>{text}</span>") :
                $"<a href=\"{Encode(run.Url)}\"{css}>{text}</a>");
            _plain.Append(PlainText(run.Text));
            if (run.Url is not null) _plain.Append($" ({run.Url})");
        }
        _plain.AppendLine();
        return $"<{tag} style=\"{Encode("margin:0;" + TextCss(style))}\" class=\"{id}-text\">{html}</{tag}>";
    }

    private string RenderLink(Link link, string id, ComponentStyle style)
    {
        _plain.AppendLine($"{PlainText(link.Text)} ({link.Url})");
        return $"<a href=\"{Encode(link.Url)}\"{LinkAttributes(link.Title, link.OpenInNewWindow)} class=\"{id}-text\" style=\"{Encode("display:block;" + TextCss(style))}\">{Text(link.Text)}</a>";
    }

    private string RenderButton(Button button, string id, ComponentStyle style)
    {
        _plain.AppendLine($"{PlainText(button.Text)} ({button.Url})");
        return $"<a href=\"{Encode(button.Url)}\"{LinkAttributes(button.Title, button.OpenInNewWindow)} class=\"{id}-text\" style=\"{Encode(ButtonCss(style))}\">{Text(button.Text)}</a>";
    }

    private static string ButtonCss(ComponentStyle style) =>
        $"display:block;box-sizing:border-box;padding:{Space(style.Padding)};border-radius:{Number(style.BorderRadius ?? 0)}px;" + TextCss(style);

    private string RenderImage(Image image, string id, ComponentStyle style)
    {
        var height = style.Height is { } value ? $" height=\"{Number(value)}\"" : "";
        var html = $"<img src=\"{Encode(image.SourceUrl)}\" alt=\"{Encode(image.AltText)}\"{WidthAttribute(style)}{height} class=\"{id}-image\" style=\"{Encode(ImageCss(style))}\" />";
        if (image.LinkUrl is not null)
            html = $"<a href=\"{Encode(image.LinkUrl)}\">{html}</a>";
        if (image.AltText.Length > 0)
            _plain.AppendLine(PlainText(Guard.NormalizeText(image.AltText)) + (image.LinkUrl is null ? "" : $" ({image.LinkUrl})"));
        return html;
    }

    private static string ImageCss(ComponentStyle style) =>
        "display:block;border:0;outline:none;text-decoration:none;" + Width(style) +
        (Maximum(style) is null ? "max-width:100%;" : "") +
        $"height:{(style.Height is { } height ? Number(height) + "px" : "auto")};border-radius:{Number(style.BorderRadius ?? 0)}px;" + CustomCss(style);

    private string RenderDivider(Divider divider, string id, ComponentStyle style)
    {
        _plain.AppendLine("---");
        return $"<table {Table} width=\"100%\" style=\"width:100%;\"><tbody><tr><td role=\"separator\" class=\"{id}-separator\" style=\"{Encode(DividerCss(divider, style))}\">&#8203;</td></tr></tbody></table>";
    }

    private static string DividerCss(Divider divider, ComponentStyle style) =>
        $"border-top:{Border(new EmailBorder(divider.Thickness, divider.Color, divider.LineStyle))};font-size:0;line-height:0;" + CustomCss(style);

    private string RenderSpacer(string id, ComponentStyle desktop, ComponentStyle mobile)
    {
        if ((desktop.Visible != false && desktop.Height > 0) || (mobile.Visible != false && mobile.Height > 0))
            _plain.AppendLine();
        return $"<div class=\"{id}-spacer\" aria-hidden=\"true\" style=\"{SpacerCss(desktop)}\">&#160;</div>";
    }

    private static string SpacerCss(ComponentStyle style) =>
        $"height:{Number(style.Height ?? 0)}px;line-height:{Number(style.Height ?? 0)}px;font-size:1px;mso-line-height-rule:exactly;" + CustomCss(style);

    private static string LinkAttributes(string? title, bool newWindow) =>
        (title is null ? "" : $" title=\"{Encode(title)}\"") +
        (newWindow ? " target=\"_blank\" rel=\"noopener noreferrer\"" : "");

    private static string PlainText(string text) => text.Replace("\n", Environment.NewLine);

    private void RegisterResponsive(EmailComponent component, string id, ComponentStyle desktop, ComponentStyle mobile)
    {
        if (desktop.Visible != mobile.Visible)
            Rule($".{id}-wrap", mobile.Visible == false ? "display:none;max-height:0;overflow:hidden;" :
                "display:table;max-height:none;overflow:visible;");
        if (PrimaryBox(component, desktop) != PrimaryBox(component, mobile)) Rule($".{id}-box", PrimaryBox(component, mobile));
        if (desktop.Margin != mobile.Margin) Rule($".{id}-margin", $"padding:{Space(mobile.Margin)};");
        if (desktop.Padding != mobile.Padding && component is not Button)
            Rule($".{id}-content", $"padding:{Space(mobile.Padding)};");
        if (desktop.VerticalAlignment != mobile.VerticalAlignment)
            Rule($".{id}-content", $"vertical-align:{Vertical(mobile.VerticalAlignment)};");
        if (component is TextComponent or Line or Link && TextCss(desktop) != TextCss(mobile))
            Rule($".{id}-text", TextCss(mobile));
        if (component is Button && ButtonCss(desktop) != ButtonCss(mobile))
            Rule($".{id}-text", ButtonCss(mobile));
        if (component is Image && ImageCss(desktop) != ImageCss(mobile))
            Rule($".{id}-image", ImageCss(mobile));
        if (component is Spacer && SpacerCss(desktop) != SpacerCss(mobile))
            Rule($".{id}-spacer", SpacerCss(mobile));
        if (component is Divider divider && DividerCss(divider, desktop) != DividerCss(divider, mobile))
            Rule($".{id}-separator", DividerCss(divider, mobile));
    }

    private void Rule(string selector, string css) =>
        _css.Append(selector).Append('{').Append(css.Replace(";", "!important;", StringComparison.Ordinal)).Append('}');
}

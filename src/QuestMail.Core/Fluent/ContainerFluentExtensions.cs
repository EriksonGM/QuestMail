using QuestMail.Core.Components;
using QuestMail.Core.Internal;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Fluent;

public static class ContainerFluentExtensions
{
    public static TContainer WithStyle<TContainer>(this TContainer container, ComponentStyle style)
        where TContainer : ContainerComponent
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(style);
        return ContainerOperations.Copy(container, container.Children, style, container.MobileStyle);
    }

    public static TContainer WithMobileStyle<TContainer>(this TContainer container, ComponentStyle? style)
        where TContainer : ContainerComponent
    {
        ArgumentNullException.ThrowIfNull(container);
        return ContainerOperations.Copy(container, container.Children, container.Style, style);
    }

    public static Body WithLanguage(this Body body, string language)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(language);
        return ContainerOperations.Validate(new Body(body.Children)
        {
            Language = language, CanvasColor = body.CanvasColor, Style = body.Style, MobileStyle = body.MobileStyle
        });
    }

    public static Body WithCanvasColor(this Body body, string color)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(color);
        return ContainerOperations.Validate(new Body(body.Children)
        {
            Language = body.Language, CanvasColor = color, Style = body.Style, MobileStyle = body.MobileStyle
        });
    }

    public static Row WithGap(this Row row, double gap)
    {
        ArgumentNullException.ThrowIfNull(row);
        return ContainerOperations.Validate(new Row(row.Children)
        {
            Gap = gap, MobileColumns = row.MobileColumns, Style = row.Style, MobileStyle = row.MobileStyle
        });
    }

    public static Row WithMobileColumns(this Row row, int columns)
    {
        ArgumentNullException.ThrowIfNull(row);
        return ContainerOperations.Validate(new Row(row.Children)
        {
            Gap = row.Gap, MobileColumns = columns, Style = row.Style, MobileStyle = row.MobileStyle
        });
    }
}

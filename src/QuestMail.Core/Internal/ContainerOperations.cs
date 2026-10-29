using QuestMail.Core.Components;
using QuestMail.Core.Rendering;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Internal;

internal static class ContainerOperations
{
    public static TContainer Append<TContainer>(TContainer container, EmailComponent component)
        where TContainer : ContainerComponent
    {
        ArgumentNullException.ThrowIfNull(component);
        if (component is Body)
            throw new ArgumentException("Body can only be the root of an email.", nameof(component));
        return Copy(container, container.Children.Append(component), container.Style, container.MobileStyle);
    }

    public static TContainer Copy<TContainer>(TContainer container, IEnumerable<EmailComponent> children,
        ComponentStyle style, ComponentStyle? mobileStyle) where TContainer : ContainerComponent
    {
        ContainerComponent copy = container switch
        {
            Body body => new Body(children)
            {
                Language = body.Language, CanvasColor = body.CanvasColor, Style = style, MobileStyle = mobileStyle
            },
            Frame => new Frame(children) { Style = style, MobileStyle = mobileStyle },
            Row row => new Row(children)
            {
                Gap = row.Gap, MobileColumns = row.MobileColumns, Style = style, MobileStyle = mobileStyle
            },
            _ => throw new NotSupportedException($"Container type {container.GetType().Name} is not supported.")
        };
        return Validate((TContainer)copy);
    }

    public static TContainer Validate<TContainer>(TContainer container) where TContainer : ContainerComponent
    {
        ComponentValidation.Validate(container, depth: container is Body ? 0 : 1);
        return container;
    }
}

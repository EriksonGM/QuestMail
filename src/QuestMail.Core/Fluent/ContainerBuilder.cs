using QuestMail.Core.Components;
using QuestMail.Core.Internal;
using QuestMail.Core.Rendering;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Fluent;

/// <summary>A mutable composition scope. Build returns an independent, immutable component.</summary>
public abstract class ContainerBuilder<TSelf, TContainer> : IComponentComposer<TSelf>
    where TSelf : ContainerBuilder<TSelf, TContainer>
    where TContainer : ContainerComponent
{
    private readonly List<EmailComponent> _children = new();
    private ComponentStyle _style = new();
    private ComponentStyle? _mobileStyle;

    private protected ContainerBuilder() { }

    public TSelf Add(EmailComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (component is Body)
            throw new ArgumentException("Body can only be the root of an email.", nameof(component));
        ComponentValidation.Validate(component, depth: 2);
        _children.Add(component);
        return (TSelf)this;
    }

    public TSelf WithStyle(ComponentStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        ComponentValidation.ValidateStyle(style);
        _style = style;
        return (TSelf)this;
    }

    public TSelf WithMobileStyle(ComponentStyle? style)
    {
        if (style is not null) ComponentValidation.ValidateStyle(style);
        _mobileStyle = style;
        return (TSelf)this;
    }

    public TContainer Build() => ContainerOperations.Validate(Create(_children, _style, _mobileStyle));

    private protected abstract TContainer Create(IEnumerable<EmailComponent> children,
        ComponentStyle style, ComponentStyle? mobileStyle);
}

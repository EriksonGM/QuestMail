using QuestMail.Core.Components;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Fluent;

public sealed class FrameBuilder : ContainerBuilder<FrameBuilder, Frame>
{
    private protected override Frame Create(IEnumerable<EmailComponent> children,
        ComponentStyle style, ComponentStyle? mobileStyle) =>
        new(children) { Style = style, MobileStyle = mobileStyle };
}

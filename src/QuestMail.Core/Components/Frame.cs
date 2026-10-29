using QuestMail.Core.Fluent;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

/// <summary>A vertically arranged group of components, such as a card or section.</summary>
public sealed class Frame : ContainerComponent, IComponentComposer<Frame>
{
    public Frame(params EmailComponent[] children) : base(children) { }
    public Frame(IEnumerable<EmailComponent> children) : base(children) { }

    /// <summary>Returns a new frame containing the additional component; this frame is unchanged.</summary>
    public Frame Add(EmailComponent component) => ContainerOperations.Append(this, component);
}

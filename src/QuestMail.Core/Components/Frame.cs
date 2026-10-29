namespace QuestMail.Core.Components;

/// <summary>A vertically arranged group of components, such as a card or section.</summary>
public sealed class Frame : ContainerComponent
{
    public Frame(params EmailComponent[] children) : base(children) { }
    public Frame(IEnumerable<EmailComponent> children) : base(children) { }
}

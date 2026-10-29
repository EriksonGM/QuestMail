namespace QuestMail.Core.Components;

/// <summary>Desktop table columns, wrapping to MobileColumns at 480px and below.</summary>
public sealed class Row : ContainerComponent
{
    public int MobileColumns { get; init; } = 1;
    public double Gap { get; init; }

    public Row(params EmailComponent[] children) : base(children) { }
    public Row(IEnumerable<EmailComponent> children) : base(children) { }
}

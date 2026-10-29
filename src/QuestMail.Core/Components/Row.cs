using QuestMail.Core.Fluent;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

/// <summary>Desktop table columns, wrapping to MobileColumns at 480px and below.</summary>
public sealed class Row : ContainerComponent, IComponentComposer<Row>
{
    public int MobileColumns { get; init; } = 1;
    public double Gap { get; init; }

    public Row(params EmailComponent[] children) : base(children) { }
    public Row(IEnumerable<EmailComponent> children) : base(children) { }

    /// <summary>Returns a new row containing the additional component; this row is unchanged.</summary>
    public Row Add(EmailComponent component) => ContainerOperations.Append(this, component);
}

using QuestMail.Core.Styles;

namespace QuestMail.Core.Components;

public abstract class EmailComponent
{
    /// <summary>Desktop properties; unset values use component defaults or inherited typography.</summary>
    public ComponentStyle Style { get; init; } = new();
    /// <summary>Overrides at viewport widths of 480px or less. Unset values retain desktop values.</summary>
    public ComponentStyle? MobileStyle { get; init; }
}

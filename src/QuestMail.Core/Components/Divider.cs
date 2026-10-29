using QuestMail.Core.Enums;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

public sealed class Divider : EmailComponent
{
    public string Color { get; }
    public double Thickness { get; }
    public BorderStyleEnum LineStyle { get; }

    public Divider(string color = "#d1d5db", double thickness = 1,
        BorderStyleEnum lineStyle = BorderStyleEnum.Solid)
    {
        Guard.Color(color, nameof(color));
        Guard.Positive(thickness, nameof(thickness));
        Guard.Defined(lineStyle, nameof(lineStyle));
        Color = color;
        Thickness = thickness;
        LineStyle = lineStyle;
    }
}

using QuestMail.Core.Enums;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Styles;

public sealed record EmailBorder
{
    public double Thickness { get; }
    public string Color { get; }
    public BorderStyleEnum Style { get; }

    public EmailBorder(double thickness, string color, BorderStyleEnum style = BorderStyleEnum.Solid)
    {
        Guard.NonNegative(thickness, nameof(thickness));
        Guard.Color(color, nameof(color));
        Guard.Defined(style, nameof(style));
        Thickness = thickness;
        Color = color;
        Style = style;
    }
}

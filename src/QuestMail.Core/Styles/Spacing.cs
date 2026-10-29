using QuestMail.Core.Internal;

namespace QuestMail.Core.Styles;

/// <summary>Non-negative pixel spacing, in CSS top/right/bottom/left order.</summary>
public readonly record struct Spacing
{
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }
    public double Left { get; }

    public Spacing(double all) : this(all, all, all, all) { }
    public Spacing(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }

    public Spacing(double top, double right, double bottom, double left)
    {
        Guard.NonNegative(top, nameof(top));
        Guard.NonNegative(right, nameof(right));
        Guard.NonNegative(bottom, nameof(bottom));
        Guard.NonNegative(left, nameof(left));
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }
}

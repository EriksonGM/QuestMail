using QuestMail.Core.Enums;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Styles;

public readonly record struct EmailWidth
{
    public WidthModeEnum Mode { get; }
    public double Value { get; }

    private EmailWidth(WidthModeEnum mode, double value)
    {
        Guard.Positive(value, nameof(value));
        if (mode == WidthModeEnum.Percent && value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Percentage width cannot exceed 100.");
        Mode = mode;
        Value = value;
    }

    public static EmailWidth Auto => default;
    public static EmailWidth Fixed(double pixels) => new(WidthModeEnum.Fixed, pixels);
    public static EmailWidth Max(double pixels) => new(WidthModeEnum.Max, pixels);
    public static EmailWidth Percent(double percent) => new(WidthModeEnum.Percent, percent);
}

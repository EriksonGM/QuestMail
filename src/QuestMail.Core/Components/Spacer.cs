using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

public sealed class Spacer : EmailComponent
{
    public double Height { get; }

    public Spacer(double height)
    {
        Guard.NonNegative(height, nameof(height));
        Height = height;
    }
}

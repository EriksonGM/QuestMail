using QuestMail.Core.Enums;

namespace QuestMail.Core.Components;

/// <summary>A reusable text block. Text is always encoded, never interpreted as HTML.</summary>
public sealed class Line : EmailComponent
{
    public string Text { get; }
    public LineSizeEnum Size { get; }
    public ElementAlignEnum Align { get; }

    public Line(string text, LineSizeEnum size = LineSizeEnum.Body,
        ElementAlignEnum align = ElementAlignEnum.Left)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!Enum.IsDefined(size))
            throw new ArgumentOutOfRangeException(nameof(size), size, "Unknown line size.");
        if (!Enum.IsDefined(align))
            throw new ArgumentOutOfRangeException(nameof(align), align, "Unknown text alignment.");

        Text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        Size = size;
        Align = align;
    }
}

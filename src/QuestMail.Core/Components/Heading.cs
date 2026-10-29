using QuestMail.Core.Enums;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

public sealed class Heading : TextComponent
{
    public HeadingLevelEnum Level { get; }

    public Heading(string text, HeadingLevelEnum level = HeadingLevelEnum.H1)
        : this(new[] { new TextRun(text) }, level) { }

    public Heading(IEnumerable<TextRun> content, HeadingLevelEnum level = HeadingLevelEnum.H1) : base(content)
    {
        Guard.Defined(level, nameof(level));
        Level = level;
    }
}

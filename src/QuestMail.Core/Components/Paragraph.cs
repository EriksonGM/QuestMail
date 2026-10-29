namespace QuestMail.Core.Components;

public sealed class Paragraph : TextComponent
{
    public Paragraph(string text) : base(new[] { new TextRun(text) }) { }
    public Paragraph(IEnumerable<TextRun> content) : base(content) { }
}

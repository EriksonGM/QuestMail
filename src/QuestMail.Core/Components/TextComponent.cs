namespace QuestMail.Core.Components;

public abstract class TextComponent : EmailComponent
{
    public IReadOnlyList<TextRun> Content { get; }
    public string Text => string.Concat(Content.Select(run => run.Text));

    protected TextComponent(IEnumerable<TextRun> content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var snapshot = content.ToArray();
        if (snapshot.Any(run => run is null))
            throw new ArgumentException("Text runs cannot contain null.", nameof(content));
        Content = Array.AsReadOnly(snapshot);
    }
}

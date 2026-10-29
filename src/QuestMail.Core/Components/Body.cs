namespace QuestMail.Core.Components;

/// <summary>The email root. Defaults to centered, 600px-wide content on a neutral canvas.</summary>
public sealed class Body : ContainerComponent
{
    public string CanvasColor { get; init; } = "#f3f4f6";
    public string Language { get; init; } = "en";

    public Body(params EmailComponent[] children) : base(children) { }
    public Body(IEnumerable<EmailComponent> children) : base(children) { }
}

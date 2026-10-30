using QuestMail.Core.Fluent;
using QuestMail.Core.Internal;

namespace QuestMail.Core.Components;

/// <summary>The email root. Defaults to centered, 600px-wide content with a 15px outer margin on a neutral canvas. Override with Style.Margin.</summary>
public sealed class Body : ContainerComponent, IComponentComposer<Body>
{
    public string CanvasColor { get; init; } = "#f3f4f6";
    public string Language { get; init; } = "en";
    /// <summary>Optional absolute HTTP(S) URL rendered as a non-layout 1×1 open-tracking image. Null disables tracking.</summary>
    public string? TrackingPixelUrl { get; init; }

    public Body(params EmailComponent[] children) : base(children) { }
    public Body(IEnumerable<EmailComponent> children) : base(children) { }

    /// <summary>Returns a new body containing the additional component; this body is unchanged.</summary>
    public Body Add(EmailComponent component) => ContainerOperations.Append(this, component);
}

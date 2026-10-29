namespace QuestMail.Core.Components;

public abstract class ContainerComponent : EmailComponent
{
    public IReadOnlyList<EmailComponent> Children { get; }

    protected ContainerComponent(IEnumerable<EmailComponent> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        var snapshot = children.ToArray();
        if (snapshot.Any(child => child is null))
            throw new ArgumentException("Children cannot contain null components.", nameof(children));
        if (snapshot.Any(child => child is Body))
            throw new ArgumentException("Body can only be the root of an email.", nameof(children));
        Children = Array.AsReadOnly(snapshot);
    }
}

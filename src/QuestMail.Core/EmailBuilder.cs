using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.Core.Rendering;

namespace QuestMail.Core;

public class EmailBuilder : IComponentComposer<EmailBuilder>
{
    private readonly Body _body;
    private readonly List<EmailComponent> _components;

    public EmailBuilder() : this(new Body()) { }

    public EmailBuilder(Body body)
    {
        ArgumentNullException.ThrowIfNull(body);
        ComponentValidation.Validate(body);
        _body = body;
        _components = new List<EmailComponent>(body.Children);
    }

    public EmailBuilder Add(Line line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return Add((EmailComponent)line);
    }

    public EmailBuilder Add(EmailComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (component is Body)
            throw new ArgumentException("Use the EmailBuilder(Body) constructor to configure the root.", nameof(component));
        ComponentValidation.Validate(component, depth: 1);
        _components.Add(component);
        return this;
    }

    public EmailContent Build()
    {
        return new EmailRenderer().Render(_body, _components);
    }
}



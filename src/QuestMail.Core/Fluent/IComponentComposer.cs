using QuestMail.Core.Components;

namespace QuestMail.Core.Fluent;

/// <summary>A typed composition target. Add returns the target for the next fluent call.</summary>
public interface IComponentComposer<TSelf>
{
    TSelf Add(EmailComponent component);
}

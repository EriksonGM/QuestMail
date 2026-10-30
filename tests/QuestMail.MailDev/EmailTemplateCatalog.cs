namespace QuestMail.MailDev;

public static class EmailTemplateCatalog
{
    public static IReadOnlyList<IEmailTemplate> Discover()
    {
        var templates = typeof(IEmailTemplate).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(IEmailTemplate).IsAssignableFrom(type))
            .Select(type => (IEmailTemplate)Activator.CreateInstance(type)!)
            .OrderBy(template => template.Name, StringComparer.Ordinal)
            .ToArray();

        if (templates.Length == 0)
            throw new InvalidOperationException("No email templates were found.");
        if (templates.Any(template => string.IsNullOrWhiteSpace(template.Name) || string.IsNullOrWhiteSpace(template.Subject)))
            throw new InvalidOperationException("Every template must define a name and subject.");
        if (templates.Select(template => template.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != templates.Length)
            throw new InvalidOperationException("Email template names must be unique (case-insensitive).");

        return templates;
    }

    public static IReadOnlyList<IEmailTemplate> Select(string? name)
    {
        var templates = Discover();
        if (name is null)
            return templates;

        var selected = templates.FirstOrDefault(template => string.Equals(template.Name, name, StringComparison.OrdinalIgnoreCase));
        if (selected is null)
            throw new ArgumentException($"Unknown template '{name}'. Available templates: {string.Join(", ", templates.Select(template => template.Name))}.");
        return [selected];
    }
}

namespace QuestMail.Core.Components;

public static class ComponentExtensions
{
    public static EmailBuilder AddFrame(this EmailBuilder builder, params EmailComponent[] children)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Frame(children));
    }

    public static EmailBuilder AddRow(this EmailBuilder builder, params EmailComponent[] children)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Row(children));
    }

    public static EmailBuilder AddButton(this EmailBuilder builder, string text, string url)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Button(text, url));
    }

    public static EmailBuilder AddLink(this EmailBuilder builder, string text, string url)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Link(text, url));
    }

    public static EmailBuilder AddImage(this EmailBuilder builder, string sourceUrl, string altText)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Image(sourceUrl, altText));
    }

    public static EmailBuilder AddDivider(this EmailBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Divider());
    }

    public static EmailBuilder AddSpacer(this EmailBuilder builder, double height)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(new Spacer(height));
    }
}

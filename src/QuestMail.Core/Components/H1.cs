using QuestMail.Core.Enums;

namespace QuestMail.Core.Components;

public static class H1
{
    public static void AddH1(this EmailBuilder builder,  string text, AddH1Parameters parameters = default)
    {
        if(parameters is null)
            parameters = new AddH1Parameters();

        var align = "";

        switch (parameters.TextAlign)
        {
            case ElementAlignEnum.Center:
                align = "align-center";
                break;
            case ElementAlignEnum.Right:
                align = "text-right";
                break;
            case ElementAlignEnum.Left:
                align = "text-left";
                break;
        }
        
        builder.Html.AppendLine($"<h1 class=\"{parameters.Classes}\">{text}</h1>");
    }

}

public class AddH1Parameters
{
    public string Classes { get; init; } = string.Empty;
    public ElementAlignEnum TextAlign { get; init; } = ElementAlignEnum.Left;
}


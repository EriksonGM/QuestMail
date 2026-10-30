namespace QuestMail.MailDev;

public sealed record CommandLineOptions(string? Template, bool List, bool Help)
{
    public static CommandLineOptions Parse(string[] args)
    {
        string? template = null;
        var list = false;
        var help = false;
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--help" or "-h":
                    help = true;
                    break;
                case "--list":
                    list = true;
                    break;
                case "--template":
                    if (template is not null || index + 1 >= args.Length || args[index + 1].StartsWith('-') ||
                        string.IsNullOrWhiteSpace(args[index + 1]))
                        throw new ArgumentException("Specify --template once, followed by a template name.");
                    template = args[++index];
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'.");
            }
        }

        return new CommandLineOptions(template, list, help);
    }
}

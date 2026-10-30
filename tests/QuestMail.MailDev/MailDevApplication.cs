using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace QuestMail.MailDev;

public static class MailDevApplication
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = CommandLineOptions.Parse(args);
            if (options.Help)
            {
                Console.WriteLine("QuestMail local email previews");
                Console.WriteLine("Usage: dotnet run --project tests/QuestMail.MailDev -- [--list | --template <name> | --help]");
                Console.WriteLine("Without arguments, sends every template to MailDev.");
                Console.WriteLine("Settings: MAILDEV_SMTP_HOST, MAILDEV_SMTP_PORT, MAILDEV_FROM, MAILDEV_TO.");
                return 0;
            }

            var templates = EmailTemplateCatalog.Select(options.Template);
            if (options.List)
            {
                foreach (var template in templates)
                    Console.WriteLine($"{template.Name}: {template.Subject}");
                return 0;
            }

            var settings = MailDevSettings.FromEnvironment();
            var messages = new List<MimeMessage>();
            try
            {
                // Render all templates before connecting, so rendering failures do not send a partial batch.
                foreach (var template in templates)
                    messages.Add(EmailMessageFactory.Create(template, settings));

                using var client = new SmtpClient { Timeout = 10_000 };
                // Deliberately unencrypted and unauthenticated: this tool is for local MailDev only.
                await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.None);
                foreach (var message in messages)
                {
                    await client.SendAsync(message);
                    Console.WriteLine($"Sent: {message.Subject}");
                }
                await client.DisconnectAsync(true);
            }
            finally
            {
                foreach (var message in messages)
                    message.Dispose();
            }

            Console.WriteLine($"Sent {messages.Count} email(s) to {settings.Host}:{settings.Port}.");
            var webPort = Environment.GetEnvironmentVariable("MAILDEV_WEB_PORT") ?? "1080";
            Console.WriteLine($"Open http://localhost:{webPort} to inspect HTML and plain-text bodies.");
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine("Use --help for usage and --list for available templates.");
            return 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Could not send the email previews: {exception.Message}");
            Console.Error.WriteLine("Start MailDev with: docker compose -f tests/docker-compose.yml up -d --wait");
            return 1;
        }
    }
}

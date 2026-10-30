using System.Globalization;
using MimeKit;

namespace QuestMail.MailDev;

public sealed record MailDevSettings(string Host, int Port, MailboxAddress From, MailboxAddress To)
{
    public static MailDevSettings FromEnvironment(Func<string, string?>? getVariable = null)
    {
        getVariable ??= Environment.GetEnvironmentVariable;
        var host = getVariable("MAILDEV_SMTP_HOST") ?? "127.0.0.1";
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("MAILDEV_SMTP_HOST must not be empty.");

        var portValue = getVariable("MAILDEV_SMTP_PORT") ?? "1025";
        if (!int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out var port) || port is < 1 or > 65535)
            throw new ArgumentException("MAILDEV_SMTP_PORT must be an integer between 1 and 65535.");

        return new MailDevSettings(host, port,
            ParseAddress(getVariable("MAILDEV_FROM") ?? "QuestMail <sender@questmail.test>", "MAILDEV_FROM"),
            ParseAddress(getVariable("MAILDEV_TO") ?? "Email previews <recipient@questmail.test>", "MAILDEV_TO"));
    }

    private static MailboxAddress ParseAddress(string value, string setting)
    {
        if (!MailboxAddress.TryParse(value, out var address) || !address.Address.Contains('@') ||
            value.Contains('\r') || value.Contains('\n'))
            throw new ArgumentException($"{setting} must contain one valid email address.");
        return address;
    }
}

namespace QuestMail.MailDev.Tests;

public class ConfigurationTests
{
    [Fact]
    public void DefaultsSendAllTemplatesToLocalMailDevWithTestAddresses()
    {
        var options = CommandLineOptions.Parse([]);
        var settings = MailDevSettings.FromEnvironment(_ => null);

        Assert.Null(options.Template);
        Assert.False(options.List);
        Assert.False(options.Help);
        Assert.Equal("127.0.0.1", settings.Host);
        Assert.Equal(1025, settings.Port);
        Assert.Equal("sender@questmail.test", settings.From.Address);
        Assert.Equal("recipient@questmail.test", settings.To.Address);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    public void HelpIsRecognized(string argument) => Assert.True(CommandLineOptions.Parse([argument]).Help);

    [Fact]
    public void ListAndTemplateSelectionAreRecognized()
    {
        Assert.True(CommandLineOptions.Parse(["--list"]).List);
        Assert.Equal("newsletter", CommandLineOptions.Parse(["--template", "newsletter"]).Template);
    }

    [Fact]
    public void InvalidArgumentsFail()
    {
        string[][] cases =
        [
            ["--template"],
            ["--template", ""],
            ["--template", "--list"],
            ["--template", "welcome", "--template", "newsletter"],
            ["--unknown"]
        ];
        foreach (var args in cases)
            Assert.Throws<ArgumentException>(() => CommandLineOptions.Parse(args));
    }

    [Fact]
    public void SmtpSettingsCanBeOverridden()
    {
        var variables = new Dictionary<string, string>
        {
            ["MAILDEV_SMTP_HOST"] = "maildev",
            ["MAILDEV_SMTP_PORT"] = "2525",
            ["MAILDEV_FROM"] = "Sender <from@example.test>",
            ["MAILDEV_TO"] = "Recipient <to@example.test>"
        };

        var settings = MailDevSettings.FromEnvironment(name => variables.GetValueOrDefault(name));

        Assert.Equal("maildev", settings.Host);
        Assert.Equal(2525, settings.Port);
        Assert.Equal("from@example.test", settings.From.Address);
        Assert.Equal("to@example.test", settings.To.Address);
    }

    [Theory]
    [InlineData("MAILDEV_SMTP_PORT", "0")]
    [InlineData("MAILDEV_SMTP_PORT", "65536")]
    [InlineData("MAILDEV_SMTP_PORT", "abc")]
    [InlineData("MAILDEV_SMTP_PORT", "")]
    [InlineData("MAILDEV_SMTP_HOST", " ")]
    [InlineData("MAILDEV_FROM", "not-an-email")]
    [InlineData("MAILDEV_TO", "")]
    [InlineData("MAILDEV_TO", "first@example.test, second@example.test")]
    [InlineData("MAILDEV_FROM", "sender@example.test\r\nBcc: other@example.test")]
    public void InvalidSettingsFailWithTheSettingName(string setting, string value)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MailDevSettings.FromEnvironment(name => name == setting ? value : null));

        Assert.Contains(setting, exception.Message);
    }
}

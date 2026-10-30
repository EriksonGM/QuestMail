# Email previews with MailDev

This local test harness sends QuestMail-generated HTML and plain-text emails to [MailDev](https://github.com/maildev/maildev). It does not add SMTP dependencies to the QuestMail library or send messages to real recipients.

## Start MailDev and send the samples

Prerequisites: .NET 10 SDK and Docker with Docker Compose. Run these commands from the repository root:

```sh
docker compose -f tests/docker-compose.yml up -d --wait
dotnet run --project tests/QuestMail.MailDev
```

Open **http://localhost:1080** to view the five emails. Use MailDev's HTML/plain-text views and responsive preview to inspect their content and layout.

The Compose service follows MailDev's [Docker documentation](https://github.com/maildev/maildev/blob/main/docs/docker.md), pins `maildev/maildev:3.0.0`, and exposes SMTP on **127.0.0.1:1025** and the web UI on **127.0.0.1:1080**. Both ports are local-only. No SMTP authentication, TLS, outgoing server or auto-relay is configured. This setup is for local development, not production.

The inbox is temporary: no persistent mail volume is configured, so do not rely on messages surviving a restart. Each console run adds new messages and leaves existing emails untouched.

## Select a template

Without arguments, the console sends every discovered template. To list them or send only one:

```sh
dotnet run --project tests/QuestMail.MailDev -- --list
dotnet run --project tests/QuestMail.MailDev -- --template welcome
dotnet run --project tests/QuestMail.MailDev -- --help
```

`--list` and `--help` do not connect to SMTP. Unknown template names and invalid settings fail with a nonzero exit code. Template selection is case-insensitive.

| Template | Preview coverage |
| --- | --- |
| `welcome` | Onboarding, Unicode, HTML escaping, multiline text and a call to action |
| `password-reset` | Transactional email, query-string link and expiry notice |
| `order-confirmation` | Order details, totals and responsive delivery/address columns |
| `newsletter` | Two-column article cards that stack on mobile, links and unsubscribe footer |
| `security-alert` | Account notification and multiline sign-in details |

All identities, addresses, tokens, prices and links are sample data. Every email includes both bodies in a MIME `multipart/alternative` message, a `[QuestMail preview]` subject prefix, and an `X-QuestMail-Template` header.

## Optional settings

Set environment variables before running the console if you need different local settings:

| Variable | Default |
| --- | --- |
| `MAILDEV_SMTP_HOST` | `127.0.0.1` |
| `MAILDEV_SMTP_PORT` | `1025` |
| `MAILDEV_FROM` | `QuestMail <sender@questmail.test>` |
| `MAILDEV_TO` | `Email previews <recipient@questmail.test>` |

Compose also reads `MAILDEV_SMTP_PORT` (default `1025`) and `MAILDEV_WEB_PORT` (default `1080`) for its host-side port mappings. Set them before starting Compose and the console if those ports are already in use. For example, in PowerShell:

```powershell
$env:MAILDEV_SMTP_PORT = "11025"
$env:MAILDEV_WEB_PORT = "11080"
docker compose -f tests/docker-compose.yml up -d --wait
dotnet run --project tests/QuestMail.MailDev
# Open http://localhost:11080
```

The container ports remain `1025` and `1080`. When running a sender in the same Docker network, use the service hostname `maildev` and container port `1025`. The console uses `MAILDEV_WEB_PORT` for its local browser hint.

## Add a template

Templates live in [`QuestMail.MailDev/Templates`](QuestMail.MailDev/Templates). Add a public, non-abstract class implementing `IEmailTemplate`, with a public parameterless constructor, a unique case-insensitive name and a subject:

```csharp
using QuestMail.Core.Components;
using QuestMail.Core.Fluent;
using QuestMail.MailDev;

namespace QuestMail.MailDev.Templates;

public sealed class InvoiceTemplate : IEmailTemplate
{
    public string Name => "invoice";
    public string Subject => "Your sample invoice";

    public Body CreateBody() => new Body()
        .AddH1("Invoice ready")
        .AddParagraph("This is a local preview.")
        .AddButton("View invoice", "https://example.com/invoices/sample");
}
```

Reuse `TemplateLayout.Create(title)` and `TemplateLayout.Card` for the existing visual layout. The next `dotnet run` compiles and automatically discovers the new class; no registry or console changes are needed. Then run `--template invoice` to send it. Template files are C# source, not runtime-loaded HTML files.

## Automated tests and shutdown

```sh
dotnet test --solution QuestMail.slnx --configuration Release
docker compose -f tests/docker-compose.yml down
```

Automated tests validate template discovery, rendering, MIME alternatives, arguments and SMTP settings without requiring Docker or sending any emails. Actual delivery and visual inspection are performed by running the console against MailDev.

If sending fails, check `docker compose -f tests/docker-compose.yml ps` and `docker compose -f tests/docker-compose.yml logs maildev`. Ensure local ports 1025 and 1080 are available and that MailDev is healthy before retrying.

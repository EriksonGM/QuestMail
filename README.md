# QuestMail

QuestMail is a C# library for building HTML and plain-text email content with a fluent, strongly typed API.

**Status:** Early Development

**Current release:** [0.1.0](https://github.com/EriksonGM/QuestMail/releases/tag/v0.1.0) · .NET 10 · MIT

## Overview

Compose a tree of reusable email components in C#, then pass the generated bodies to your email provider. QuestMail builds content; it does not send messages or manage SMTP connections.

## Features

- Body, Frame, Row, Heading H1–H3, Paragraph, Button, Link, Divider, Spacer and Image components.
- Optional fluent composition on Body/Frame/Row and EmailBuilder, including nested callbacks.
- Structured, reusable component trees with defensive, read-only child collections.
- Typed dimensions, spacing, borders, alignment and typography; reusable style records.
- Inline desktop styles, presentation tables, Outlook maximum-width fallbacks and mobile overrides at 480px.
- Plain text generated alongside HTML, including link destinations and image alternative text.
- Encoded text/attributes, URL scheme validation and constrained CSS values.
- Immutable output snapshots, independent of subsequent builder changes.
- One NuGet package (`QuestMail`) without internal package dependencies.

Dynamic variables, conditions, repetition, custom HTML, compound catalog blocks, a visual editor, JSON persistence and provider integrations are not implemented. Builder instances are mutable and should not be shared between concurrent operations.

## Getting Started

### Prerequisites

- .NET 10 SDK. `global.json` allows stable .NET 10 feature-band and patch updates.

### Installation

Install QuestMail 0.1.0 from NuGet:

```sh
dotnet add package QuestMail --version 0.1.0
```

Alternatively, build from source and use a project reference or a locally generated NuGet package. See the [changelog and migration notes](https://github.com/EriksonGM/QuestMail/blob/master/CHANGELOG.md) for the changes included in this release.

### Usage

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;

var email = new EmailBuilder()
    .AddH1("Welcome!", ElementAlignEnum.Center)
    .AddParagraph("Hello, Ada & team.")
    .AddH2("Next steps")
    .AddLine("Confirm your account.\nStart exploring.")
    .Build();

string htmlBody = email.Html;
string plainTextBody = email.Plain;
// Pass these strings to your email provider as the two alternative bodies.
```

All text is treated as content, never raw HTML. For example, `<script>` becomes `&lt;script&gt;` in HTML and remains literal text in the plain-text body. CRLF, LF and CR input line endings become `<br />` in HTML and platform line endings in plain text. Text blocks add a trailing plain-text newline.

`AddLine` defaults to `LineSizeEnum.Body` and left alignment. Available sizes are `Body`, `Heading1`, `Heading2` and `Heading3`; alignments are `Left`, `Center` and `Right`. Null inputs and undefined enum values throw before adding content. Empty text is allowed.

Components can be reused without sharing builder state:

```csharp
var footer = new Line("Thanks for reading!", align: ElementAlignEnum.Center);
var firstEmail = new EmailBuilder().AddH1("First email").Add(footer).Build();
var secondEmail = new EmailBuilder().AddH1("Second email").Add(footer).Build();
```

### Fluent API

Import `QuestMail.Core.Fluent` to compose containers without constructing each child explicitly:

See the [Fluent API guide](https://github.com/EriksonGM/QuestMail/blob/master/docs/fluent-api.md) for the method reference, callback behavior, immutable reuse and validation rules.

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Fluent;
using QuestMail.Core.Styles;

var body = new Body()
    .WithLanguage("en")
    .WithStyle(new ComponentStyle { Width = EmailWidth.Max(640), Padding = new Spacing(24) })
    .WithMobileStyle(new ComponentStyle { Padding = new Spacing(12) })
    .AddFrame(frame =>
    {
        frame.WithStyle(new ComponentStyle { BackgroundColor = "#f8fafc", Padding = new Spacing(16) })
            .AddHeading("Welcome, Ada!")
            .AddParagraph("Your account is ready.")
            .AddRow(row => row.WithGap(16).WithMobileColumns(1)
                .AddFrame(left => left.AddHeading("Explore", HeadingLevelEnum.H2).AddParagraph("Find your next steps."))
                .AddFrame(right => right.AddButton("Open account", "https://example.com/account")));
    })
    .AddSpacer(24)
    .AddDivider()
    .AddLink("Terms and conditions", "https://example.com/terms");

var email = new EmailBuilder(body).Build();
```

- `AddFrame` and `AddRow` accept callbacks, existing child components, or component arrays. Callbacks can be expression lambdas or statement blocks and may be nested.
- All content components have fluent methods: `AddHeading`, `AddH1`–`AddH3`, `AddParagraph`, `AddLine`, `AddImage`, `AddButton`, `AddLink`, `AddDivider` and `AddSpacer`. Parameterless heading/paragraph calls create empty text blocks, so `new Body().AddFrame(f => { f.AddHeading().AddParagraph(); }).AddSpacer(24)` is valid.
- Leaf methods accept optional named `style` and `mobileStyle` records. `AddImage` also accepts `height`/`linkUrl`; buttons and links accept `title`/`openInNewWindow`. Headings and paragraphs also accept `IEnumerable<TextRun>` for rich text.
- `WithStyle` replaces desktop overrides; `WithMobileStyle` replaces mobile overrides and accepts `null` to clear them. Body additionally supports `WithLanguage`/`WithCanvasColor`; Row supports `WithGap`/`WithMobileColumns`.
- **Body, Frame and Row remain immutable:** fluent operations return a new container of the same type. Keep the returned value or chain the next call. This allows an original section to be reused without changing previous emails.
- Inside callbacks, `FrameBuilder` and `RowBuilder` are mutable composition scopes, so calls such as `f.AddHeading().AddParagraph()` work without assignment. Completed callbacks create immutable snapshots; later changes to a captured builder cannot change the added component. You can also use these builders explicitly and call `Build()` to obtain a Frame/Row snapshot.
- Callbacks are completed and validated before their component is added to the parent. A failed callback or invalid content does not append a partial section. The existing depth, URL, style and null validation rules remain in effect.
- The same methods work on `EmailBuilder`, retaining its existing mutable behavior. Both namespaces can be imported together, and the constructor-based API remains supported. Existing EmailBuilder shortcuts keep their original defaults.

`src/QuestMail.Terminal/FluentExample.cs` builds the same complete email as the constructor-based example. Both generate identical HTML and plain text.

Remember to keep the result of operations on immutable containers:

```csharp
var original = new Body();
var updated = original.AddHeading("Welcome!");
// original.Children is still empty; updated contains the heading.
```

Inside a callback, the builder collects children in place, so `frame.AddHeading("Title");` does not require reassignment. `FrameBuilder.Build()` returns a `Frame`, `RowBuilder.Build()` returns a `Row`, and `EmailBuilder.Build()` returns the final `EmailContent` with `Html` and `Plain` bodies.

### Component engine

Use `new EmailBuilder(body)` for root configuration and `.Add(component)` for any non-root component. `Body` cannot be nested. The builder appends to its own child list without modifying the original body. Existing `Line`/`AddLine` calls remain supported, while the heading and paragraph shortcuts now use their dedicated components.

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Enums;
using QuestMail.Core.Styles;

var cardStyle = new ComponentStyle
{
    BackgroundColor = "#f8fafc",
    Padding = new Spacing(24),
    Border = new EmailBorder(1, "#e2e8f0"),
    BorderRadius = 8
};

var body = new Body(
    new Heading("Welcome, Ada!"),
    new Row(
        new Frame(
            new Image("https://example.com/banner.png", "Account overview")
            {
                Style = new ComponentStyle { Width = EmailWidth.Percent(100) }
            }) { Style = cardStyle },
        new Frame(
            new Heading("Get started", HeadingLevelEnum.H2),
            new Paragraph("Your account is ready."),
            new Button("Open account", "https://example.com/account"))
        { Style = cardStyle })
    {
        Gap = 16,
        MobileColumns = 1 // Set to 2 for a two-column mobile grid.
    },
    new Spacer(24) { MobileStyle = new ComponentStyle { Height = 12 } },
    new Divider(),
    new Link("Terms and conditions", "https://example.com/terms"))
{
    Language = "en",
    CanvasColor = "#f3f4f6",
    Style = new ComponentStyle { Width = EmailWidth.Max(640), Padding = new Spacing(32) },
    MobileStyle = new ComponentStyle { Padding = new Spacing(16) }
};

var email = new EmailBuilder(body).Build();
```

| Component | Content and behavior |
| --- | --- |
| `Body` | Root children, `Language`, `CanvasColor`. Default centered white content, maximum 600px, padding 24px. |
| `Frame` | Vertical grouping of child components. |
| `Row` | Horizontal table columns, `Gap`, `MobileColumns` (default 1). Child `Style.Width` sets desktop column width; unspecified columns share remaining space. The child fills its allocated column rather than applying the percentage twice. |
| `Heading` | Text or `IEnumerable<TextRun>`; `HeadingLevelEnum.H1`, `H2` or `H3`. Default font sizes 32, 24 and 20px. |
| `Paragraph` | Text or `IEnumerable<TextRun>`, inheriting document/container typography. |
| `Button` | Text, URL, optional `Title` and `OpenInNewWindow`. Styled anchor in a table, blue/white by default, with 12px/24px padding. |
| `Link` | Text, URL, optional `Title` and `OpenInNewWindow`. Blue and underlined by default. |
| `Image` | Source URL, required alt text (empty for decorative images), optional `Height` and `LinkUrl`. Height is automatic unless explicitly supplied. |
| `Divider` | Color, thickness and solid/dashed/dotted/double line style. |
| `Spacer` | Non-negative height in pixels, with optional responsive height overrides. |

All components expose `Style` and `MobileStyle`. Style records support `with` expressions for reusable variants:

| Property | Values |
| --- | --- |
| `Width` | `EmailWidth.Auto`, `.Fixed(pixels)`, `.Max(pixels)`, `.Percent(value)` where `0 < value <= 100`. Max mode fills available width up to its cap. |
| `MaxWidth`, `Height` | Optional pixel dimensions; image/spacer height can also be overridden on mobile. |
| `Margin`, `Padding` | `new Spacing(all)`, `(vertical, horizontal)` or `(top, right, bottom, left)`. Margins are implemented as exterior table-cell padding for email compatibility. |
| `BackgroundColor`, `Border`, `BorderRadius` | Hex/named CSS colors, `new EmailBorder(thickness, color, style)` and pixel radius. A zero-width border removes a border in a mobile override. |
| `HorizontalAlignment`, `VerticalAlignment` | Left/Center/Right for block positioning; Top/Middle/Bottom for content/columns. Text alignment is a separate typography property. |
| `Typography` | `TypographyStyle` with font family, pixel size, weight, color, unitless line height, letter spacing, text alignment, italic, decoration, white space and word wrapping. |
| `Visible` | `false` hides a component; set `Style.Visible = false` and `MobileStyle.Visible = true` for mobile-only content. |

Mobile styles override only supplied properties at widths of 480px or less. Typography inherits through containers, including mobile overrides, while explicit local values win. Widths, padding and other box properties do not inherit. A hidden parent cannot be made visible by its children. Plain text includes blocks visible on either device, in tree order; blocks hidden on both are omitted. Links include destinations, non-decorative images include alt text, dividers become `---`, and positive-height spacers add a blank line.

Rich text uses encoded fragments rather than raw HTML:

```csharp
var paragraph = new Paragraph(new[]
{
    new TextRun("Hello, "),
    new TextRun("Ada") { Typography = new TypographyStyle { FontWeight = 700, Italic = true } },
    new TextRun(". Read the guide") { Url = "https://example.com/guide" }
});
```

Validation rejects null children/text, undefined enums, non-finite or invalid dimensions, unsafe URL schemes, CSS delimiters in font families/colors and trees deeper than 64 levels. Links accept absolute HTTP/HTTPS, `mailto:` and `tel:` URLs; images accept HTTP/HTTPS and `cid:` URLs. Assets are not downloaded or embedded. Dynamic provider syntax and raw HTML are not interpreted.

**Client compatibility:** the renderer uses table layouts and inline defaults rather than flexbox or grid. Mobile overrides require media-query support. Outlook for Windows may ignore rounded corners, anchor padding, white-space rules or other CSS; maximum-width conditional tables provide a fallback, not a guarantee of identical rendering. Test final templates in the email clients you support. No cross-client certification or VML-based rounded buttons is provided.

### Build, test and run

```sh
dotnet restore QuestMail.slnx
dotnet build QuestMail.slnx --configuration Release --no-restore
dotnet test --solution QuestMail.slnx --configuration Release --no-build --no-restore
dotnet run --project src/QuestMail.Terminal --configuration Release --no-build
dotnet pack src/QuestMail.Core/QuestMail.Core.csproj --configuration Release --no-build --no-restore --output artifacts
```

The terminal example demonstrates all ten components. Pass `-- --html` to print only the HTML document for preview/export, or `-- --fluent` to use the equivalent fluent example. Options can be combined:

```sh
dotnet run --project src/QuestMail.Terminal --configuration Release --no-build -- --fluent --html
```

## Project Structure

For local email delivery and visual previews, see [MailDev test setup](https://github.com/EriksonGM/QuestMail/blob/master/tests/README.md).

The solution contains the following projects:

- **QuestMail.Core**: The core library containing the main logic for building emails.
- **QuestMail.Terminal**: Runnable example that prints HTML and plain text.
- **QuestMail.Core.Tests**: Automated component, responsive CSS, encoding, validation and builder-state tests.
- **QuestMail.MailDev**: Local console that sends the templates in `tests/QuestMail.MailDev/Templates` to MailDev.
- **QuestMail.MailDev.Tests**: Automated preview-template, MIME message and configuration tests (no SMTP server required).

The previously empty Shared project has been consolidated into Core to avoid a separate package dependency.

Core namespaces:

- `QuestMail.Core.Components`: Immutable components and existing EmailBuilder shortcuts.
- `QuestMail.Core.Fluent`: Composition extensions, `FrameBuilder` and `RowBuilder` callback scopes.
- `QuestMail.Core.Styles`: Reusable component styles, typography, spacing, widths and borders.
- `QuestMail.Core.Enums`: Typed heading levels, alignment and other style values.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the [MIT License](https://github.com/EriksonGM/QuestMail/blob/master/LICENSE).

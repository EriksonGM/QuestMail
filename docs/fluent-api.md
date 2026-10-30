# Fluent API guide

This guide covers QuestMail 0.1.0 and requires .NET 10. See the [changelog](../CHANGELOG.md) for release highlights and migration notes.

QuestMail supports both constructor-based composition and fluent composition. Both build the same component tree and use the same HTML/plain-text renderer. The Fluent API does not introduce a second template format or send messages.

## Getting started

Import `QuestMail.Core.Fluent` alongside the component namespace:

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

var body = new Body()
    .AddFrame(frame =>
    {
        frame.AddHeading("Welcome!")
             .AddParagraph("Your account is ready.")
             .AddButton("Open account", "https://example.com/account");
    })
    .AddSpacer(24)
    .AddLink("Terms", "https://example.com/terms");

var email = new EmailBuilder(body).Build();
string html = email.Html;
string plainText = email.Plain;
```

The same methods work on `Body`, `Frame`, `Row`, `FrameBuilder`, `RowBuilder` and `EmailBuilder`. Existing constructor-based components can be mixed into a chain with `.Add(component)`.

## Return values and mutability

| Target | Operation | Result |
| --- | --- | --- |
| `Body`, `Frame`, `Row` | `Add...`, `With...` | A new container of the same type; the original is unchanged. |
| `FrameBuilder`, `RowBuilder` | `Add...`, `With...` | The same mutable composition scope. |
| `FrameBuilder` | `Build()` | An immutable `Frame` snapshot. |
| `RowBuilder` | `Build()` | An immutable `Row` snapshot. |
| `EmailBuilder` | `Add...` | The same mutable email builder. |
| `EmailBuilder` | `Build()` | An immutable `EmailContent` snapshot with `Html` and `Plain`. |

Keep the result when modifying an immutable container:

```csharp
var empty = new Body();
var welcome = empty.AddHeading("Welcome");
var reminder = empty.AddParagraph("Please confirm your account.");
// empty has no children. welcome and reminder are independent bodies.
```

Calling `empty.AddHeading("Ignored");` without using its return value does not change `empty`. In contrast, callback builders deliberately collect children in place.

Builders are not thread-safe. Do not share a mutable scope or EmailBuilder across concurrent operations. Completed components have read-only child collections and can be reused.

## Nested containers and callbacks

`AddFrame` accepts an `Action<FrameBuilder>` and `AddRow` accepts an `Action<RowBuilder>`. Statement blocks and expression lambdas are both supported:

```csharp
using QuestMail.Core.Enums;

var body = new Body().AddFrame(frame =>
{
    frame.AddHeading("Your next steps")
         .AddRow(row => row.WithGap(16).WithMobileColumns(1)
             .AddFrame(left => left
                 .AddHeading("Explore", HeadingLevelEnum.H2)
                 .AddParagraph("Discover the available features."))
             .AddFrame(right => right
                 .AddHeading("Start", HeadingLevelEnum.H2)
                 .AddButton("Open dashboard", "https://example.com/dashboard")))
         .AddParagraph("Contact us if you need help.");
});
```

The callback adds children only to its own scope. The completed component is validated and snapshotted before it is appended to its parent. Exceptions propagate to the caller; a failed callback does not append its partially built section. Later modifications to a captured scope do not change an already appended component.

Existing components and arrays are supported too:

```csharp
var shared = new Paragraph("Thanks for reading.");
var body = new Body()
    .AddFrame(shared)
    .AddRow(new Paragraph("Left"), new Paragraph("Right"))
    .Add(new Frame().AddHeading("Reusable section").Add(shared));
```

`Body` is the document root and cannot be added inside another component. Configure it before passing it to `new EmailBuilder(body)`.

## Component method reference

| Method | Content/options |
| --- | --- |
| `AddFrame(...)` | Callback or `params EmailComponent[]` children. |
| `AddRow(...)` | Callback or `params EmailComponent[]` children. |
| `AddHeading(text, level)` | Text; `HeadingLevelEnum.H1` by default. Also accepts `IEnumerable<TextRun>`. |
| `AddH1(text)`, `AddH2(text)`, `AddH3(text)` | Heading shortcuts; optional `align`. |
| `AddParagraph(text)` | Text or `IEnumerable<TextRun>`; optional `align`. |
| `AddLine(text, size, align)` | Compatibility text component; defaults to body size and left alignment. |
| `AddImage(sourceUrl, altText)` | Required URL/alt text; optional `height` and `linkUrl`. Empty alt text is allowed for decorative images. |
| `AddButton(text, url)` | Required label/destination; optional `title` and `openInNewWindow`. Renders a styled anchor, not an HTML button. |
| `AddLink(text, url)` | Required label/destination; optional `title` and `openInNewWindow`. |
| `AddDivider(color, thickness, lineStyle)` | Defaults to `#d1d5db`, 1px, `BorderStyleEnum.Solid`. |
| `AddSpacer(height)` | Non-negative height in pixels. |

Text methods support empty content. For example, `new Body().AddFrame(f => { f.AddHeading().AddParagraph(); }).AddSpacer(24)` is valid, but produces empty text blocks. Supply text for real content.

Leaf methods accept optional named `style` and `mobileStyle` parameters. When both `QuestMail.Core.Components` and `QuestMail.Core.Fluent` are imported, the original EmailBuilder shortcuts remain available and retain their existing defaults. The generic composition contract is `IComponentComposer<TSelf>.Add(EmailComponent)`.

## Styles and responsive configuration

Import `QuestMail.Core.Styles` for the existing typed style records:

```csharp
using QuestMail.Core.Styles;

var cardStyle = new ComponentStyle
{
    BackgroundColor = "#f8fafc",
    Padding = new Spacing(24),
    Border = new EmailBorder(1, "#e2e8f0"),
    BorderRadius = 8
};

var body = new Body()
    .WithLanguage("en")
    .WithCanvasColor("#f3f4f6")
    .WithStyle(new ComponentStyle { Width = EmailWidth.Max(640), Padding = new Spacing(32) })
    .WithMobileStyle(new ComponentStyle { Padding = new Spacing(16) })
    .AddFrame(frame => frame.WithStyle(cardStyle)
        .AddHeading("Welcome", mobileStyle: new ComponentStyle
        {
            Typography = new TypographyStyle { FontSize = 28 }
        })
        .AddImage("https://example.com/banner.png", "Account overview",
            style: new ComponentStyle { Width = EmailWidth.Percent(100) })
        .AddButton("Continue", "https://example.com/account",
            style: new ComponentStyle { HorizontalAlignment = QuestMail.Core.Enums.ElementAlignEnum.Center },
            mobileStyle: new ComponentStyle { Width = EmailWidth.Percent(100) }))
    .AddSpacer(24, mobileStyle: new ComponentStyle { Height = 12 });
```

- `WithStyle` replaces the container's desktop override record. It does not merge with previously supplied overrides; component defaults still apply to unset fields. Use `style with { ... }` to derive a variant while retaining other fields.
- `WithMobileStyle` replaces the mobile override record. Passing `null` clears it. At widths of 480px or less, only specified properties override desktop values.
- `Body` supports `WithLanguage` and `WithCanvasColor`.
- `Body` defaults to a 15px outer margin on all sides. Override it with `.WithStyle(new ComponentStyle { Margin = new Spacing(12) })`, or use `new Spacing(0)` to remove it. `WithMobileStyle` can override the margin independently on mobile.
- `Row` and `RowBuilder` support `WithGap` and `WithMobileColumns`. Desktop layout uses table columns; mobile layout wraps to the requested positive number of columns, defaulting to one.
- `TypographyStyle` inherits through containers. Other box properties, such as width and padding, do not inherit. Explicit local typography overrides inherited values.
- An explicitly supplied `align` on heading shortcuts or paragraphs overrides the style's text alignment without removing its other typography settings.
- `ComponentStyle.CustomCss` accepts immutable `CssDeclarations` for a restricted set of visual properties, emitted inline on the primary element. `MobileStyle.CustomCss` supplies mobile overrides. See the [custom CSS guide](custom-css.md) for examples, element targets, precedence, value validation and email-client limitations.

For the full style model and client compatibility limitations, see the [component engine reference in the README](../README.md#component-engine). Media-query support and Outlook CSS support vary by email client; a browser preview is not cross-client certification.

## Reusable builder snapshots

Builders can also be used outside callbacks:

```csharp
var draft = new FrameBuilder().AddHeading("Shared heading");
Frame first = draft.Build();
draft.AddParagraph("Added later");
Frame second = draft.Build();
// first still has only its heading; second includes the paragraph.

var body = new Body().Add(first).Add(second);
```

The constructor-based API continues to work. Reusing a component or style does not connect its state to later builder changes.

## Validation and output

Composition uses the same validation as the component engine:

- Null targets, callbacks, text, children and required URLs are rejected. Empty text is allowed.
- Undefined enum values, negative/non-finite dimensions, zero-width fixed/max sizes and non-positive mobile column counts are rejected.
- Links allow absolute HTTP/HTTPS, `mailto:` and `tel:` URLs; images allow HTTP/HTTPS and `cid:` URLs. Files are not downloaded or attached.
- Text/attributes are HTML-encoded. Constrained style values prevent arbitrary CSS injection. Raw HTML and dynamic template expressions are not evaluated.
- Trees cannot exceed 64 levels of nesting. Fluent composition cannot bypass that limit.

HTML and plain text are generated together, in component order. Plain text includes link destinations and non-decorative image alt text, uses `---` for dividers and a blank line for positive-height spacers. A completed EmailContent snapshot does not change when an EmailBuilder is subsequently extended.

## Run the equivalent examples

From the repository root, with the .NET 10 SDK:

```sh
dotnet restore QuestMail.slnx
dotnet build QuestMail.slnx --configuration Release --no-restore
dotnet test --solution QuestMail.slnx --configuration Release --no-build --no-restore
dotnet run --project src/QuestMail.Terminal --configuration Release --no-build
dotnet run --project src/QuestMail.Terminal --configuration Release --no-build -- --fluent
dotnet run --project src/QuestMail.Terminal --configuration Release --no-build -- --fluent --html
```

The first two terminal modes produce identical HTML and plain text. `--html` prints only the HTML document. The fluent example is implemented in `src/QuestMail.Terminal/FluentExample.cs`; the constructor-based version is in `src/QuestMail.Terminal/Program.cs`.

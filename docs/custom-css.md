# Custom CSS

Prefer QuestMail's typed `ComponentStyle` and `TypographyStyle` properties for layout, visibility and inherited typography. Use `ComponentStyle.CustomCss` for additional visual declarations on a component's primary HTML element.

## Usage

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Styles;

var paragraph = new Paragraph("Content in its original case")
{
    Style = new ComponentStyle
    {
        Typography = new TypographyStyle { Color = "#334155" },
        CustomCss = CssDeclarations.Create(
            ("text-transform", "uppercase"),
            ("word-spacing", "2px"))
    },
    MobileStyle = new ComponentStyle
    {
        CustomCss = CssDeclarations.Create(("text-transform", "none"))
    }
};

var email = new EmailBuilder().Add(paragraph).Build();
```

Desktop declarations are emitted inline. At viewport widths of 480px or less, mobile declarations use the existing media query with `!important`. CSS affects HTML only: `email.Plain` retains the original text and is not transformed or hidden by custom visual declarations.

The same property works with fluent methods:

```csharp
using QuestMail.Core.Fluent;

var body = new Body()
    .AddParagraph("Highlighted", style: new ComponentStyle
    {
        CustomCss = CssDeclarations.Create(("text-transform", "uppercase"))
    });
```

## Immutable collections

`CssDeclarations.Create` accepts property/value tuples or an `IEnumerable<KeyValuePair<string, string>>`, such as a dictionary. Input is copied and validated. The resulting collection is read-only and can be safely reused.

```csharp
var shared = CssDeclarations.Create(("text-transform", "uppercase"));
var variant = shared.Set("word-spacing", "2px");
var withoutTransform = variant.Remove("text-transform");
// shared and variant are unchanged by later operations.
```

- Names are trimmed and normalized to lowercase; lookups are case-insensitive.
- Values are trimmed. A repeated property replaces its earlier value and moves to the end.
- Declaration order is preserved, including shorthand/longhand cascade order.
- Equality includes both values and order, allowing deterministic rendering and avoiding redundant rules for equivalent declarations.
- `null` or `CssDeclarations.Empty` supplies no new declarations. On mobile, omitted properties retain desktop declarations. To reset a desktop effect on mobile, explicitly set a value such as `text-transform: none`; removing a key from the mobile collection does not reset the desktop value.

## Where CSS is applied

| Component | Primary HTML element |
| --- | --- |
| `Body`, `Frame`, `Row` | The component's `qm-…-box` table, not the outer wrapper, HTML `<body>`, or row grid table |
| `Heading`, `Paragraph`, `Line` | Its `<h1>`–`<h3>` or `<p>` |
| `Button`, `Link` | Its `<a>` |
| `Image` | Its `<img>`, not an optional enclosing link |
| `Divider` | Its separator `<td>` |
| `Spacer` | Its spacer `<div>` |

Custom declarations are not copied into child `ComponentStyle` records. Normal browser/email-client CSS inheritance still applies for properties not overridden by descendants. Child text elements already declare resolved typography inline, so custom table-level typography may not affect them. Use typed `TypographyStyle` for predictable inheritance through the component tree. Explicitly styled `TextRun` spans and inline links may likewise override their enclosing text element's styles.

Typed box properties can target a wrapper or cell instead of the primary element. Custom CSS does not relocate or rewrite those properties; for example, custom paragraph `background-color` colors the `<p>`, while typed `BackgroundColor` colors its component table.

## Precedence

For declarations on the same primary element:

1. Component defaults and resolved typed styles.
2. Desktop `CustomCss`, in declaration order.
3. Explicit mobile typed overrides, where that typed property is rendered on the primary element.
4. Mobile `CustomCss`, in declaration order.

Unspecified desktop custom properties remain active on mobile. Explicit mobile typed typography removes conflicting desktop custom typography; a typed mobile border on a container replaces desktop custom border color/style/width declarations. Mobile custom declarations can override these typed mobile values again. No cross-element precedence is implied for properties rendered on different wrappers or child elements.

## Supported properties and value syntax

This first version intentionally uses a closed set of visual properties and a restricted token grammar, **not a general CSS parser or full CSS semantic validator**. Passing validation does not guarantee that a value is meaningful for its property or supported by an email client.

| Group | Allowed properties |
| --- | --- |
| Colors | `color`, `background-color` |
| Typography | `font-family`, `font-size`, `font-style`, `font-weight`, `font-variant`, `line-height`, `letter-spacing`, `word-spacing` |
| Text | `text-align`, `text-decoration`, `text-decoration-color`, `text-decoration-line`, `text-decoration-style`, `text-indent`, `text-transform`, `text-shadow`, `white-space`, `word-break`, `word-wrap`, `overflow-wrap` |
| Borders | `border-radius`, `border-color`, `border-style`, `border-width`, and `border-top/right/bottom/left-color/style/width` longhands |
| Other visual styles | `outline-color`, `outline-style`, `outline-width`, `box-shadow`, `opacity` |

Values must be 1–512 characters long and contain only letters, digits, spaces, `#`, `.`, `,`, `%`, `+`, `-`, and balanced single/double quotes. Examples include `uppercase`, `2px`, `-0.5px`, `1.5`, `#334155`, `"Times New Roman", serif` and `1px 2px 3px #000000`.

Rejected inputs throw an `ArgumentException` (or `ArgumentNullException` for null arguments) when the collection is created or changed:

- Layout/structural properties such as `display`, `position`, `width`, `height`, `margin`, `padding`, `table-layout`, `visibility` and `all`. Use typed styles instead where available.
- Unknown properties, CSS variables, vendor-specific properties, raw rules, selectors and at-rules.
- `!important`, semicolons, braces, comments, escapes, control characters and HTML delimiters.
- All CSS functions, including `url(...)`, `expression(...)`, `var(...)`, `calc(...)` and `rgb(...)`. Use hexadecimal/named colors instead of color functions.

The renderer HTML-encodes inline attributes. Responsive declarations use validated tokens directly; no external stylesheet or external CSS resource is loaded. Extending this API to unrestricted functions or arbitrary CSS would require a real parser and a separate validation policy, not weakening the token checks.

Email clients may ignore supported declarations, especially shadows and mobile overrides. Browser or MailDev previews do not certify Outlook/Gmail compatibility. See the `custom-css` template in [the local MailDev harness](../tests/README.md) for an interactive example.

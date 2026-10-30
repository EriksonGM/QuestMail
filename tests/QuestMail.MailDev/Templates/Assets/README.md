# Embedded preview artwork

The reference-inspired templates use the checked-in PNGs in this directory as MIME inline images (`cid:` URLs). They are embedded into the console assembly by `QuestMail.MailDev.csproj`; sending or viewing an email requires no image server, Node.js, SVG renderer, working-directory assumptions or external image downloads.

`Sources/*.svg` contains editable artwork created for these previews. Brand names and layout/color choices follow the supplied visual references, but these are **inspired recreations**, not official branded templates or reproductions of the original photographs. The fashion/portrait/product images are original vector illustrations. The Facebook glyph is from [Simple Icons](https://simpleicons.org/) (CC0); the remaining icons and illustrations are drawn for the preview collection. All links, order data, identities and password tokens are sample content.

## Regenerate the PNGs (optional)

Only asset authors need Node.js and `@resvg/resvg-js` 2.6.2. With that package available on Node's module path, run from the repository root:

```sh
node tests/QuestMail.MailDev/Templates/Assets/render-assets.cjs
```

The script rasterizes the SVGs at 2× resolution and writes the corresponding PNGs. Text uses Arial/system sans-serif; font rendering may differ across operating systems. Do not remove the PNGs after generating them: they are the email-compatible assets used by the .NET console, while SVGs remain source files only.

## Add a local image to a template

1. Add `my-image.png` to this directory (optionally keep its editable SVG in `Sources`).
2. Declare `new EmailInlineImage("my-image")` in the template's `InlineImages` collection.
3. Use `new EmailInlineImage("my-image").Source` as an `Image` component's source and supply meaningful alt text.

```csharp
public IReadOnlyList<EmailInlineImage> InlineImages =>
    [new EmailInlineImage("my-image")];

// In CreateBody():
// .AddImage(new EmailInlineImage("my-image").Source, "A meaningful description")
```

The factory packages images in `multipart/related` within the HTML alternative, sets their content type to `image/png` and disposition to `inline`, and preserves a separate plain-text alternative. Missing resources and duplicate Content-IDs fail before SMTP delivery. Images are local preview resources, not automatic image downloading/attachment functionality in QuestMail.Core.

CarePair's illustrated reset button is a linked image to preserve the circular arrow from the reference; its `Reset password` alt text remains an accessible link when images are blocked, and the full reset URL is included in plain text. Other actions use live text buttons. Email-client rendering, image blocking, Outlook round corners and media-query support still vary.

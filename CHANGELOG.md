# Changelog

All project documentation and release notes are maintained in English.

## Unreleased

### Changed

- Release authentication now uses NuGet Trusted Publishing and GitHub OIDC instead of a long-lived repository API key.
- Publishing is manual-only from `master`, with a required Maior, Minor or Fix selection and automatic version increments from the highest stable Git tag (or the project version when no tags exist).
- Successful NuGet publications create a matching Git tag and GitHub release at the validated commit. Release runs are serialized; automatic triggers and existing-tag recovery publishing have been removed.

## [0.1.0] - 2026-10-29

QuestMail 0.1.0 introduces a reusable email component engine and fluent composition for .NET 10, with HTML and plain-text output from the same component tree.

### Added

- Structural components: `Body`, `Frame` and `Row`.
- Content components: `Heading` H1-H3, `Paragraph`, `Button`, `Link`, `Divider`, `Spacer` and `Image`.
- Fluent composition through `QuestMail.Core.Fluent`, including nested `AddFrame` and `AddRow` callbacks, heading shortcuts and all content components.
- Mutable `FrameBuilder` and `RowBuilder` scopes that produce independent, immutable component snapshots.
- Reusable typed styles for automatic, fixed, maximum and percentage widths; spacing; borders; colors; alignment; typography; height; and visibility.
- Mobile style overrides at 480px and below, including stacked rows and multi-column mobile grids.
- Table-based email HTML with inline styles, maximum-width Outlook fallbacks, accessible image alt text and semantic headings.
- Plain-text rendering with link destinations, image alt text, readable separators and preserved line breaks.
- Rich text through encoded `TextRun` fragments, optional links and local typography overrides.
- Validation for null inputs, undefined enum values, unsafe URL schemes, invalid CSS values, non-finite dimensions and excessive nesting.
- Long-word wrapping and width-constrained table layouts to prevent unintended overflow in supported clients.
- 128 automated tests covering rendering, responsive rules, validation, immutable reuse, callback isolation and constructor/fluent output equivalence.
- Complete constructor-based and fluent terminal examples, including HTML-only export with `--html`.
- A Fluent API guide and expanded component, style, compatibility and release documentation.
- MIT licensing, with the license text included in the NuGet package and SPDX metadata declared as `MIT`.

### Changed

- `EmailBuilder` now builds a component tree and returns an immutable `EmailContent` snapshot containing `Html` and `Plain`.
- Components and child collections are reusable without sharing mutable builder state.
- The empty Shared project has been consolidated into Core. The `QuestMail` NuGet package has no external package dependencies.
- Nullable reference types are enabled in Core.
- CI restores, builds and tests the entire solution using .NET 10 before packaging.
- NuGet publication is restricted to validated stable version tags such as `v0.1.0`; the tag determines the package version.
- Shared Rider project settings are included; local workspace and user-specific IDE files remain ignored.

### Migration notes

- This release targets `net10.0`. Consumers need a compatible .NET 10 application.
- Use `EmailBuilder.Build()` and read `EmailContent.Html` / `EmailContent.Plain` instead of modifying public HTML or plain-text buffers.
- `Body`, `Frame` and `Row` fluent methods return new containers. Keep the returned value or continue the chain. Callback builders collect children in place and snapshot them when the callback completes.
- Existing `AddLine` and heading shortcuts remain available. Use `ComponentStyle` and `TypographyStyle` for typed styling rather than legacy heading parameter objects or raw CSS class strings.
- Text is always HTML-encoded. Raw HTML, variables, conditions and repetition are not evaluated.
- The API is still in early development; future 0.x releases may introduce breaking changes.

### Known limitations

- QuestMail builds email content; it does not send messages, manage SMTP connections or integrate with an email service provider.
- Mobile overrides depend on client media-query support. Outlook for Windows may ignore some CSS, including rounded corners or anchor padding. Cross-client rendering certification and VML-based rounded buttons are not included.
- A visual editor, JSON persistence, custom HTML, dynamic template processing and compound catalog blocks are not implemented.

### Install

```sh
dotnet add package QuestMail --version 0.1.0
```

For a runnable example, use:

```sh
dotnet run --project src/QuestMail.Terminal --configuration Release -- --fluent --html
```

[0.1.0]: https://github.com/EriksonGM/QuestMail/releases/tag/v0.1.0

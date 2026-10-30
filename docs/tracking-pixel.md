# Optional email open-tracking pixel

`Body.TrackingPixelUrl` adds a decorative 1×1 remote image to the HTML email. It is **disabled by default** (`null`) and must be explicitly configured by the caller.

```csharp
using QuestMail.Core;
using QuestMail.Core.Components;
using QuestMail.Core.Fluent;

var body = new Body()
    .WithTrackingPixel("https://tracking.example.com/open.gif?message=sample-123")
    .AddHeading("Welcome!")
    .AddParagraph("Your account is ready.");

var email = new EmailBuilder(body).Build();

// Equivalent constructor-based configuration:
var otherBody = new Body(new Paragraph("Hello"))
{
    TrackingPixelUrl = "https://tracking.example.com/open.gif?message=sample-456"
};

// Returns a new body without tracking; the original body is unchanged.
var withoutTracking = body.WithTrackingPixel(null);
```

## Rendering and endpoint

- Only absolute HTTP/HTTPS URLs are accepted. Relative URLs, `cid:`, `data:`, `mailto:`, unsafe schemes, empty values and control characters are rejected. Prefer HTTPS.
- The URL is HTML-encoded into the image's `src`. Rendering **does not make an HTTP request**; it only produces HTML. A mail client may request the image when loading the email.
- The pixel is emitted once, directly under the HTML `<body>`, after the content table. It does not use a component wrapper or change generated component IDs.
- Its HTML and CSS dimensions are 1×1. Absolute positioning removes it from normal layout and opacity hides it; empty alt text, `role="presentation"` and `aria-hidden="true"` keep it decorative for assistive technology. It does not use lazy loading or `display:none`.
- The plain-text alternative is unchanged and does not contain the tracking URL.
- All fluent body operations preserve the setting. Passing `null` to `WithTrackingPixel` disables it on a new body.

Provide an endpoint you control that returns a **transparent 1×1 GIF or PNG** with the appropriate image content type. Record the request server-side if you need an open signal. QuestMail does not implement the endpoint, store analytics, inject recipient identifiers or attach an image for this URL. Use opaque per-message identifiers rather than email addresses or other personal information in query strings.

## Reliability and privacy

An image request is **not proof that a person opened or read the message**. Clients can block remote images entirely, reuse cached images, proxy requests, prefetch images, or request them as part of privacy/security scanning. Apple Mail Privacy Protection and similar mechanisms can load images without a human opening the email. Plain-text-only views do not load the pixel. Requests may happen zero, one or multiple times, independently of actual opens.

Email clients may remove or reinterpret CSS, so invisible/non-layout behavior is best-effort across clients; returning a transparent image is important even when opacity or positioning is ignored. Do not rely on this mechanism for delivery/read guarantees or exact open counts.

Enable tracking only where appropriate for your recipients, privacy notice, consent requirements and applicable policies. The sample MailDev templates do not enable tracking automatically, and the unit tests generate pixel markup without contacting the example endpoints.

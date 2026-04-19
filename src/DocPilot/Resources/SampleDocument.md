# DocPilot — Technical Overview

**DocPilot** is a native Windows desktop application that turns any PDF, DOCX,
or TXT document into a conversational Claude-powered chat session. It is
built to showcase modern .NET desktop engineering — MVVM, dependency
injection, streaming HTTP, and runtime theming — in a single, cohesive app.

---

## 1. Product Goals

DocPilot was designed around three north-star goals:

1. **Document-first conversational UX** — drop a file, ask anything, receive
   a streaming Markdown-rendered response with visible "thinking" state.
2. **Native Windows feel** — the app should feel at home next to Outlook and
   Visual Studio: DPI-aware, Fluent-inspired chrome, dark + light themes.
3. **Production-grade engineering** — strict MVVM layering, testable services
   behind interfaces, DI container at the composition root, and encrypted
   credential storage via Windows DPAPI.

## 2. Architecture

The application is layered into five well-defined tiers:

- **Views** — XAML, no logic beyond input routing.
- **ViewModels** — `CommunityToolkit.Mvvm` source-generated `ObservableObject`s.
- **Services** — cross-cutting capabilities behind interfaces.
- **Models** — plain data types, immutable where possible.
- **Converters / Themes / Resources** — pure presentation assets.

The composition root in `App.xaml.cs` wires everything through
`Microsoft.Extensions.Hosting`, giving the application the same DI model
that ASP.NET Core projects use.

## 3. Streaming Responses

DocPilot's chat panel reveals the assistant's reply token-by-token, just
like Claude.ai. Internally, `ClaudeAIService.SendMessageStreamAsync`
returns an `IAsyncEnumerable<string>`, and a small, dedicated
`ClaudeStreamParser` reads the Server-Sent Events (SSE) frames coming
back from `/v1/messages`, yielding `delta.text` chunks as they arrive.

The `ChatViewModel` consumes the enumerable with `await foreach`,
appending every chunk to an `ObservableProperty` on the current message,
which in turn triggers a partial repaint of the Markdown bubble. The
result is Claude.ai-quality streaming — natively on Windows — in under
80 lines of networking code.

## 4. Document Parsing

Three parser implementations live behind a single `IDocumentParser`
interface, selected at runtime by `DocumentParserFactory`:

- `PdfParser` — wraps **PdfPig** for text extraction.
- `DocxParser` — uses the **OpenXML SDK** and preserves hard page breaks.
- `TxtParser` — treats plain text as virtual pages of ~4000 characters.

Adding a new format is as simple as implementing `IDocumentParser` and
registering it with the DI container. The factory picks it up
automatically.

## 5. Themes & Localisation

Dark is the default palette; Light is one click away in **Settings**.
Theme swapping replaces a single merged `ResourceDictionary` at runtime
and updates ModernWPF's `ThemeManager`, propagating the change to every
control that uses `DynamicResource` bindings — no window reload needed.

Localisation uses strongly-typed `Strings.resx` resources, with English
and Simplified Chinese available out of the box.

## 6. Security

The Claude API key never touches the disk in clear text. `SettingsService`
encrypts it with **Windows DPAPI** (current-user scope) before writing
`%AppData%\DocPilot\settings.json`, and decrypts on read. Losing the
settings file leaks nothing; copying it to another machine makes the key
unreadable.

## 7. What's Next

Planned enhancements:

- Vector retrieval for long documents (replace the 10,000-character cap)
- A browsable conversation history list in the sidebar
- Multi-provider support (OpenAI, Gemini) behind the same `IAIService`
  abstraction
- MSIX packaging and auto-update via GitHub Releases

---

*Drop this document into DocPilot, or open your own — the assistant will
answer questions grounded in the contents.*

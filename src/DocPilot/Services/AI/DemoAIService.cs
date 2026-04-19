using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.AI;

/// <summary>
/// Offline <see cref="IAIService"/> used when no Claude API key is configured.
/// Streams hand-crafted Markdown responses so reviewers can experience the
/// full chat flow — typing animation, Markdown rendering, Stop button —
/// without signing up for the Anthropic API.
/// </summary>
/// <remarks>
/// The response is picked heuristically from the user message so quick-action
/// presets (Summarize / Translate / Ask Questions) get distinct scripted
/// answers. A free-form question falls back to a generic assistant reply.
/// </remarks>
public sealed class DemoAIService : IAIService
{
    private const int WordDelayMs = 25;

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        string documentContext,
        IReadOnlyList<ChatMessage> history,
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Small initial delay so the "thinking" dots appear before the text.
        await Task.Delay(500, ct).ConfigureAwait(false);

        var script = PickScript(userMessage);
        foreach (var token in TokeniseForStreaming(script))
        {
            ct.ThrowIfCancellationRequested();
            yield return token;
            await Task.Delay(WordDelayMs, ct).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default) =>
        Task.FromResult(false);

    private static string PickScript(string userMessage)
    {
        var m = userMessage?.ToLowerInvariant() ?? string.Empty;

        if (m.Contains("summari"))
            return Summary;
        if (m.Contains("translat"))
            return Translation;
        if (m.Contains("question"))
            return Questions;

        return Generic;
    }

    /// <summary>
    /// Split response into word-ish tokens so the stream feels like a real
    /// model (punctuation rides the preceding word, whitespace preserved).
    /// </summary>
    private static IEnumerable<string> TokeniseForStreaming(string text)
    {
        var buffer = new System.Text.StringBuilder();
        foreach (var ch in text)
        {
            buffer.Append(ch);
            if (ch == ' ' || ch == '\n')
            {
                yield return buffer.ToString();
                buffer.Clear();
            }
        }
        if (buffer.Length > 0)
            yield return buffer.ToString();
    }

    private const string Summary = """
        **DocPilot** is a native Windows desktop app that brings Claude-style
        streaming document chat to the desktop. Here are the highlights:

        ### 🎯 Product goals
        - **Document-first UX** — drop a PDF/DOCX/TXT, ask anything, see streaming Markdown replies
        - **Native Windows feel** — DPI-aware, Fluent chrome, Dark + Light themes
        - **Production-grade engineering** — MVVM, DI, DPAPI-encrypted credentials

        ### 🏛 Architecture (5 layers)
        1. **Views** — XAML only, zero logic
        2. **ViewModels** — `CommunityToolkit.Mvvm` source-generated observables
        3. **Services** — everything behind interfaces, swap-friendly
        4. **Models** — immutable plain data
        5. **Converters / Themes / Resources** — pure presentation

        ### ⚡ Streaming
        Responses flow token-by-token via `IAsyncEnumerable<string>` backed
        by a small **Server-Sent Events** parser. Every chunk updates an
        `ObservableProperty`, repainting the Markdown bubble in-place.

        ### 🔐 Security
        The API key is encrypted with **Windows DPAPI** (current-user scope)
        before it ever touches disk — copying the settings file to another
        machine makes the key unreadable.

        *You're currently running in **Demo Mode**. Add a Claude API key in
        Settings to talk to the real model.*
        """;

    private const string Translation = """
        ### 翻译示例 (Translation Example)

        **DocPilot** 是一款原生 Windows 桌面应用，让你能够和任何 PDF、
        DOCX 或 TXT 文档进行对话式交互，由 Claude API 提供智能支持。

        核心特性：
        - 📄 **文档优先**：拖入文件，立即开始提问
        - ⚡ **流式回复**：像 Claude.ai 一样逐字呈现
        - 🎨 **原生体验**：深色/浅色主题，支持 DPI 缩放
        - 🔐 **安全可靠**：API 密钥通过 Windows DPAPI 加密保护

        > 当前为 **Demo 模式**，在"设置"中填入 Claude API Key 即可接入
        > 真实模型。
        """;

    private const string Questions = """
        Here are **five insightful questions** you could ask about this document:

        1. *"What architectural patterns does DocPilot use, and why were they chosen?"*
        2. *"How does the streaming response pipeline work, end-to-end?"*
        3. *"What makes DPAPI a better choice than storing the API key in plaintext JSON?"*
        4. *"How would I add support for a new document format, say `.epub`?"*
        5. *"What's on the roadmap for replacing the 10,000-character context cap?"*

        Pick any one and I'll expand on it — or type your own question in the
        input box below.

        *You're running in **Demo Mode**. Add a Claude API key in Settings
        for live answers grounded in your document.*
        """;

    private const string Generic = """
        👋 Hi! You're currently running DocPilot in **Demo Mode**, which means
        I'm streaming a scripted reply instead of calling the real Claude API.

        Try one of the quick-action buttons above:

        - **Summarize** — get a structured overview of the loaded document
        - **Translate** — see a sample translation
        - **Ask Questions** — get suggested questions to explore

        When you're ready for real answers, head to **Settings** and paste a
        Claude API key from <https://console.anthropic.com/>. The app will
        immediately switch to live mode.
        """;
}

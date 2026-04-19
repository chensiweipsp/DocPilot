using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.Settings;
using Microsoft.Extensions.Logging;

namespace DocPilot.Services.AI;

/// <summary>
/// Claude-flavoured <see cref="IAIService"/> that streams responses from the
/// Anthropic <c>/v1/messages</c> endpoint via Server-Sent Events.
/// </summary>
public sealed class ClaudeAIService : IAIService
{
    private const string MessagesPath = "v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    private readonly HttpClient _http;
    private readonly ISettingsService _settings;
    private readonly ILogger<ClaudeAIService> _logger;

    /// <summary>Create the service with a pre-configured <see cref="HttpClient"/>.</summary>
    /// <param name="http">Injected typed client (base address set by <c>AddHttpClient</c>).</param>
    /// <param name="settings">Settings service for key retrieval.</param>
    /// <param name="logger">DI logger.</param>
    public ClaudeAIService(HttpClient http, ISettingsService settings, ILogger<ClaudeAIService> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        string documentContext,
        IReadOnlyList<ChatMessage> history,
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var current = await _settings.LoadAsync().ConfigureAwait(false);
        var apiKey = _settings.GetApiKey(current);
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Claude API key is not configured.");

        var payload = BuildRequestBody(current, documentContext, history, userMessage, stream: true);

        using var request = new HttpRequestMessage(HttpMethod.Post, MessagesPath)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogError("Claude API returned {Status}: {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException($"Claude API error {(int)response.StatusCode}: {Truncate(body, 500)}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        await foreach (var delta in ClaudeStreamParser.ReadDeltasAsync(reader, ct))
            yield return delta;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        // Smallest possible valid request: one user turn, 1 token, no stream.
        var body = JsonSerializer.Serialize(new
        {
            model = "claude-haiku-4-5",
            max_tokens = 1,
            messages = new[] { new { role = "user", content = "ping" } },
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, MessagesPath)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);

        try
        {
            using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude API key validation failed.");
            return false;
        }
    }

    /// <summary>
    /// Build the request body sent to Claude's <c>/v1/messages</c> endpoint.
    /// Exposed for testing.
    /// </summary>
    public static string BuildRequestBody(
        AppSettings settings,
        string documentContext,
        IReadOnlyList<ChatMessage> history,
        string userMessage,
        bool stream)
    {
        var messages = new List<object>(history.Count + 1);
        foreach (var m in history)
        {
            if (m.Role is MessageRole.User or MessageRole.Assistant && !string.IsNullOrEmpty(m.Content))
                messages.Add(new { role = m.Role == MessageRole.User ? "user" : "assistant", content = m.Content });
        }
        messages.Add(new { role = "user", content = userMessage });

        var systemText = BuildSystemPrompt(documentContext, settings.DocumentContextCharLimit);

        var payload = new Dictionary<string, object?>
        {
            ["model"] = settings.Model,
            ["max_tokens"] = settings.MaxOutputTokens,
            ["messages"] = messages,
        };
        if (!string.IsNullOrEmpty(systemText))
            payload["system"] = systemText;
        if (stream)
            payload["stream"] = true;

        return JsonSerializer.Serialize(payload);
    }

    private static string BuildSystemPrompt(string documentContext, int charLimit)
    {
        if (string.IsNullOrWhiteSpace(documentContext))
        {
            return "You are DocPilot, a helpful assistant for analysing documents. " +
                   "No document is currently loaded; ask the user to open one when relevant.";
        }

        var trimmed = documentContext.Length > charLimit
            ? documentContext[..charLimit]
            : documentContext;

        var sb = new StringBuilder();
        sb.AppendLine("You are DocPilot, a helpful assistant for analysing documents.");
        sb.AppendLine("Base your answers on the document content below. When quoting, be accurate.");
        sb.AppendLine("If the answer is not in the document, say so explicitly.");
        sb.AppendLine();
        sb.AppendLine("--- DOCUMENT CONTENT ---");
        sb.AppendLine(trimmed);
        if (documentContext.Length > charLimit)
            sb.AppendLine($"[... truncated: only the first {charLimit:N0} characters were provided ...]");
        sb.AppendLine("--- END OF DOCUMENT ---");
        return sb.ToString();
    }

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max] + "…";
}

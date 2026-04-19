using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.Settings;

namespace DocPilot.Services.AI;

/// <summary>
/// Top-level <see cref="IAIService"/> seen by the view-model. Routes each
/// call to either the live Claude client or the offline <see cref="DemoAIService"/>
/// depending on whether an API key is configured.
/// </summary>
/// <remarks>
/// Keeping the routing here means the rest of the app is blissfully unaware
/// of Demo Mode — the chat pipeline looks identical either way.
/// </remarks>
public sealed class AIServiceRouter : IAIService
{
    private readonly ClaudeAIService _live;
    private readonly DemoAIService _demo;
    private readonly ISettingsService _settings;

    /// <summary>Create the router with both backends injected.</summary>
    public AIServiceRouter(ClaudeAIService live, DemoAIService demo, ISettingsService settings)
    {
        _live = live;
        _demo = demo;
        _settings = settings;
    }

    /// <summary>True if no API key is configured — DocPilot is in Demo Mode.</summary>
    public bool IsDemoMode
    {
        get
        {
            var current = _settings.LoadAsync().GetAwaiter().GetResult();
            var key = _settings.GetApiKey(current);
            return string.IsNullOrWhiteSpace(key);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> SendMessageStreamAsync(
        string documentContext,
        IReadOnlyList<ChatMessage> history,
        string userMessage,
        CancellationToken ct = default)
    {
        return IsDemoMode
            ? _demo.SendMessageStreamAsync(documentContext, history, userMessage, ct)
            : _live.SendMessageStreamAsync(documentContext, history, userMessage, ct);
    }

    /// <inheritdoc />
    public Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default) =>
        _live.ValidateApiKeyAsync(apiKey, ct);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.AI;

/// <summary>
/// Sends chat turns to an AI provider and yields streaming text deltas back.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Send a user message and stream the assistant's response back as text
    /// chunks as they arrive from the server.
    /// </summary>
    /// <param name="documentContext">
    ///   The document text injected into the system prompt. May be empty if no
    ///   document is loaded.
    /// </param>
    /// <param name="history">
    ///   Prior user/assistant messages to include as conversation history.
    ///   The current user message is <b>not</b> in this list.
    /// </param>
    /// <param name="userMessage">Current user message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    IAsyncEnumerable<string> SendMessageStreamAsync(
        string documentContext,
        IReadOnlyList<ChatMessage> history,
        string userMessage,
        CancellationToken ct = default);

    /// <summary>
    /// Quick liveness check against the provider to confirm the key works.
    /// </summary>
    /// <param name="apiKey">API key to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default);
}

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace DocPilot.Services.AI;

/// <summary>
/// Parses Anthropic's Server-Sent Events stream into a stream of text deltas.
/// Only handles the <c>content_block_delta</c> event; others are ignored.
/// </summary>
/// <remarks>
/// SSE frames look like:
/// <code>
///   event: content_block_delta
///   data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}
/// </code>
/// and are separated by blank lines.
/// </remarks>
public static class ClaudeStreamParser
{
    /// <summary>Asynchronously yield text deltas until the stream ends.</summary>
    /// <param name="reader">Reader over the HTTP response body.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async IAsyncEnumerable<string> ReadDeltasAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string? line;
        string? currentEvent = null;

        while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) is not null)
        {
            if (line.Length == 0)
            {
                currentEvent = null;
                continue;
            }

            if (line.StartsWith("event:", System.StringComparison.Ordinal))
            {
                currentEvent = line[6..].Trim();
                continue;
            }

            if (!line.StartsWith("data:", System.StringComparison.Ordinal))
                continue;

            var payload = line[5..].TrimStart();
            if (payload.Length == 0 || payload == "[DONE]")
                continue;

            // If the event hint says this is a delta, parse and yield.
            if (currentEvent is null or "content_block_delta")
            {
                var delta = TryExtractDelta(payload);
                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
            }
        }
    }

    private static string? TryExtractDelta(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var typeEl))
            {
                var type = typeEl.GetString();
                if (type != "content_block_delta") return null;
            }

            if (root.TryGetProperty("delta", out var deltaEl) &&
                deltaEl.TryGetProperty("text", out var textEl))
            {
                return textEl.GetString();
            }
        }
        catch (JsonException)
        {
            // Malformed frame — skip silently.
        }

        return null;
    }
}

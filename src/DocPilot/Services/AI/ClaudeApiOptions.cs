namespace DocPilot.Services.AI;

/// <summary>
/// Tunable parameters for <see cref="ClaudeAIService"/>. Bound from settings
/// rather than a static config section so changes apply live.
/// </summary>
public sealed class ClaudeApiOptions
{
    /// <summary>Anthropic API base URL.</summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com/";

    /// <summary>Anthropic API version header value.</summary>
    public string AnthropicVersion { get; set; } = "2023-06-01";

    /// <summary>Claude model identifier, e.g. <c>claude-sonnet-4-5</c>.</summary>
    public string Model { get; set; } = "claude-sonnet-4-5";

    /// <summary>Upper bound on generated tokens per call.</summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>Hard cap on document characters injected into the system prompt.</summary>
    public int DocumentContextCharLimit { get; set; } = 10_000;
}

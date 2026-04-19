using System.Text.Json.Serialization;

namespace DocPilot.Models;

/// <summary>
/// Persisted user configuration. Lives under <c>%AppData%\DocPilot\settings.json</c>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// DPAPI-encrypted Claude API key (Base64 of the protected blob).
    /// Never stored in clear text on disk.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ProtectedApiKey { get; set; }

    /// <summary>Selected Claude model identifier (e.g. <c>claude-sonnet-4-5</c>).</summary>
    public string Model { get; set; } = "claude-sonnet-4-5";

    /// <summary>Active visual theme.</summary>
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;

    /// <summary>UI culture name (e.g. <c>en</c>, <c>zh-CN</c>).</summary>
    public string Language { get; set; } = "en";

    /// <summary>Hard cap on document characters sent as context. MVP default is 10000.</summary>
    public int DocumentContextCharLimit { get; set; } = 10000;

    /// <summary>Upper bound on tokens the model may generate per reply.</summary>
    public int MaxOutputTokens { get; set; } = 4096;
}

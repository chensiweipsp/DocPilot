using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.Settings;

/// <summary>
/// Reads and writes the user's <see cref="AppSettings"/> to
/// <c>%AppData%\DocPilot\settings.json</c>. The API key is stored under Windows
/// DPAPI so it is never on disk in clear text.
/// </summary>
public interface ISettingsService
{
    /// <summary>Load settings from disk; returns defaults if no file exists.</summary>
    Task<AppSettings> LoadAsync();

    /// <summary>Save settings to disk, creating the config directory if needed.</summary>
    /// <param name="settings">Settings snapshot to persist.</param>
    Task SaveAsync(AppSettings settings);

    /// <summary>Decrypt the protected API key from the current settings snapshot.</summary>
    /// <param name="settings">Settings containing the protected blob.</param>
    string? GetApiKey(AppSettings settings);

    /// <summary>Encrypt and attach the given plaintext API key to the settings snapshot.</summary>
    /// <param name="settings">Settings to mutate.</param>
    /// <param name="plaintext">The API key in clear text.</param>
    void SetApiKey(AppSettings settings, string? plaintext);
}

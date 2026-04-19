using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DocPilot.Models;
using Microsoft.Extensions.Logging;

namespace DocPilot.Services.Settings;

/// <summary>
/// Default <see cref="ISettingsService"/>. Persists JSON to
/// <c>%AppData%\DocPilot\settings.json</c> and protects the API key with
/// DPAPI (per-user scope) before writing.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("DocPilot.v1.DPAPI.entropy");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;

    /// <summary>Create the settings service with the default storage location.</summary>
    /// <param name="logger">DI logger.</param>
    public SettingsService(ILogger<SettingsService> logger)
        : this(logger, GetDefaultPath())
    {
    }

    /// <summary>Test-only constructor that overrides the on-disk location.</summary>
    /// <param name="logger">DI logger.</param>
    /// <param name="settingsPath">Absolute path to the settings file.</param>
    public SettingsService(ILogger<SettingsService> logger, string settingsPath)
    {
        _logger = logger;
        _settingsPath = settingsPath;
    }

    /// <summary>The resolved settings file path (useful for diagnostics).</summary>
    public string SettingsPath => _settingsPath;

    /// <inheritdoc />
    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();

            await using var stream = File.OpenRead(_settingsPath);
            var loaded = await JsonSerializer
                .DeserializeAsync<AppSettings>(stream, JsonOptions)
                .ConfigureAwait(false);
            return loaded ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings at {Path}. Falling back to defaults.", _settingsPath);
            return new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var dir = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var stream = File.Create(_settingsPath);
        await JsonSerializer
            .SerializeAsync(stream, settings, JsonOptions)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public string? GetApiKey(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrEmpty(settings.ProtectedApiKey))
            return null;

        try
        {
            var blob = Convert.FromBase64String(settings.ProtectedApiKey);
            var plain = ProtectedData.Unprotect(blob, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt stored API key. Treating as absent.");
            return null;
        }
    }

    /// <inheritdoc />
    public void SetApiKey(AppSettings settings, string? plaintext)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrEmpty(plaintext))
        {
            settings.ProtectedApiKey = null;
            return;
        }

        var blob = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(plaintext),
            Entropy,
            DataProtectionScope.CurrentUser);
        settings.ProtectedApiKey = Convert.ToBase64String(blob);
    }

    private static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "DocPilot", "settings.json");
    }
}

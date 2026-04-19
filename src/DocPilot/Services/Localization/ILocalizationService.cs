using System.Collections.Generic;

namespace DocPilot.Services.Localization;

/// <summary>
/// Drives UI culture switching at runtime. Looks up resource strings and
/// raises <see cref="LanguageChanged"/> so bindings can refresh.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Current IETF language tag (e.g. <c>en</c>, <c>zh-CN</c>).</summary>
    string CurrentLanguage { get; }

    /// <summary>Supported languages as (tag, display-name) pairs.</summary>
    IReadOnlyList<(string Tag, string DisplayName)> AvailableLanguages { get; }

    /// <summary>Raised after a successful <see cref="Apply"/>.</summary>
    event System.EventHandler? LanguageChanged;

    /// <summary>Switch the process's UI culture to <paramref name="tag"/>.</summary>
    /// <param name="tag">IETF language tag.</param>
    void Apply(string tag);

    /// <summary>Look up a resource string by key in the current culture.</summary>
    /// <param name="key">Resource key (matches <c>Strings.resx</c>).</param>
    string Get(string key);
}

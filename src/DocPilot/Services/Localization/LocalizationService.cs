using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DocPilot.Resources;

namespace DocPilot.Services.Localization;

/// <summary>
/// Default <see cref="ILocalizationService"/>. Delegates lookups to the
/// generated <see cref="Strings"/> class and keeps <see cref="CultureInfo"/>
/// on the current thread in sync.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    /// <inheritdoc />
    public string CurrentLanguage { get; private set; } = "en";

    /// <inheritdoc />
    public IReadOnlyList<(string Tag, string DisplayName)> AvailableLanguages { get; } = new[]
    {
        ("en", "English"),
        ("zh-CN", "简体中文"),
    };

    /// <inheritdoc />
    public event EventHandler? LanguageChanged;

    /// <inheritdoc />
    public void Apply(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            tag = "en";

        var culture = CultureInfo.GetCultureInfo(tag);
        Strings.Culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        CurrentLanguage = tag;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public string Get(string key) =>
        Strings.ResourceManager.GetString(key, Strings.Culture) ?? key;
}

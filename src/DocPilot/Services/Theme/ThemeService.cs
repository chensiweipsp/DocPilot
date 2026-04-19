using System;
using System.Linq;
using System.Windows;
using DocPilot.Models;
using ModernWpf;

namespace DocPilot.Services.Theme;

/// <summary>
/// Default <see cref="IThemeService"/>. Keeps ModernWPF's global theme in sync
/// with the palette merged dictionary we ship in <c>Themes/</c>.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private const string PaletteKey = "DocPilot.ActivePalette";

    /// <inheritdoc />
    public ThemeMode Current { get; private set; } = ThemeMode.Dark;

    /// <inheritdoc />
    public void Apply(ThemeMode theme)
    {
        Current = theme;

        var app = Application.Current;
        if (app is null) return;

        // Toggle ModernWPF's global theme so built-in controls follow along.
        ThemeManager.Current.ApplicationTheme =
            theme == ThemeMode.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;

        // Swap our custom palette dictionary in App.xaml -> MergedDictionaries.
        var merged = app.Resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d =>
            d.Source is not null &&
            (d.Source.OriginalString.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
             d.Source.OriginalString.EndsWith("LightTheme.xaml", StringComparison.OrdinalIgnoreCase)));

        var newSource = theme == ThemeMode.Dark
            ? new Uri("/Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("/Themes/LightTheme.xaml", UriKind.Relative);

        var replacement = new ResourceDictionary { Source = newSource };

        if (existing is not null)
        {
            var index = merged.IndexOf(existing);
            merged[index] = replacement;
        }
        else
        {
            merged.Add(replacement);
        }
    }
}

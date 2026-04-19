using DocPilot.Models;

namespace DocPilot.Services.Theme;

/// <summary>
/// Swaps the active merged resource dictionary at runtime so the UI updates
/// instantly when the user picks a different theme.
/// </summary>
public interface IThemeService
{
    /// <summary>The currently active theme.</summary>
    ThemeMode Current { get; }

    /// <summary>Apply the given theme to the running application.</summary>
    /// <param name="theme">Target theme.</param>
    void Apply(ThemeMode theme);
}

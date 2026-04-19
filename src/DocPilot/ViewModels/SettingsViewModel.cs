using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocPilot.Models;
using DocPilot.Resources;
using DocPilot.Services.AI;
using DocPilot.Services.Dialog;
using DocPilot.Services.Localization;
using DocPilot.Services.Settings;
using DocPilot.Services.Theme;

namespace DocPilot.ViewModels;

/// <summary>
/// Backs the settings dialog: API key, model, theme, language.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly IAIService _ai;
    private readonly IThemeService _theme;
    private readonly ILocalizationService _locale;
    private readonly IDialogService _dialogs;

    private AppSettings _current = new();

    [ObservableProperty]
    private string? _apiKey;

    [ObservableProperty]
    private string _model = "claude-sonnet-4-5";

    [ObservableProperty]
    private ThemeMode _selectedTheme;

    [ObservableProperty]
    private string _language = "en";

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private string? _validationMessage;

    /// <summary>Available Claude model identifiers shown in the dropdown.</summary>
    public IReadOnlyList<string> AvailableModels { get; } = new[]
    {
        "claude-opus-4-5",
        "claude-sonnet-4-5",
        "claude-haiku-4-5",
    };

    /// <summary>Available (tag, display-name) language pairs.</summary>
    public IReadOnlyList<(string Tag, string DisplayName)> AvailableLanguages { get; }

    /// <summary>Available theme modes for the combo.</summary>
    public IReadOnlyList<ThemeMode> AvailableThemes { get; } = new[]
    {
        ThemeMode.Dark,
        ThemeMode.Light,
    };

    /// <summary>Create the settings view-model with injected services.</summary>
    public SettingsViewModel(
        ISettingsService settings,
        IAIService ai,
        IThemeService theme,
        ILocalizationService locale,
        IDialogService dialogs)
    {
        _settings = settings;
        _ai = ai;
        _theme = theme;
        _locale = locale;
        _dialogs = dialogs;
        AvailableLanguages = locale.AvailableLanguages;

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _current = await _settings.LoadAsync().ConfigureAwait(true);
        ApiKey = _settings.GetApiKey(_current);
        Model = _current.Model;
        SelectedTheme = _current.Theme;
        Language = _current.Language;
    }

    /// <summary>Persist edits; apply theme/language immediately.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        _current.Model = Model;
        _current.Theme = SelectedTheme;
        _current.Language = Language;
        _settings.SetApiKey(_current, ApiKey);

        await _settings.SaveAsync(_current).ConfigureAwait(true);
        _theme.Apply(SelectedTheme);
        _locale.Apply(Language);

        ClosingRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Validate the currently entered API key against the server.</summary>
    [RelayCommand(CanExecute = nameof(CanValidate))]
    private async Task ValidateAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey)) return;
        IsValidating = true;
        ValidationMessage = Strings.ValidatingApiKey;
        try
        {
            var ok = await _ai.ValidateApiKeyAsync(ApiKey).ConfigureAwait(true);
            ValidationMessage = ok ? Strings.ApiKeyValid : Strings.ApiKeyInvalid;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"{Strings.ApiKeyInvalid}: {ex.Message}";
        }
        finally
        {
            IsValidating = false;
        }
    }

    private bool CanValidate() => !IsValidating && !string.IsNullOrWhiteSpace(ApiKey);

    partial void OnApiKeyChanged(string? value) => ValidateCommand.NotifyCanExecuteChanged();

    partial void OnIsValidatingChanged(bool value) => ValidateCommand.NotifyCanExecuteChanged();

    /// <summary>Raised when the dialog should close (fired by <see cref="SaveAsync"/>).</summary>
    public event EventHandler? ClosingRequested;

    /// <summary>Close the dialog without saving.</summary>
    [RelayCommand]
    private void Cancel() => ClosingRequested?.Invoke(this, EventArgs.Empty);
}

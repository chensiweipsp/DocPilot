using System;
using System.Windows;
using System.Windows.Controls;
using DocPilot.ViewModels;

namespace DocPilot.Views.Dialogs;

/// <summary>
/// Settings modal. Uses a <see cref="PasswordBox"/> for the API key because
/// WPF does not expose a bindable <c>Password</c> property by default.
/// </summary>
public partial class SettingsDialog : Window
{
    /// <summary>Create the settings dialog.</summary>
    public SettingsDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SettingsViewModel oldVm)
            oldVm.ClosingRequested -= OnClosingRequested;
        if (e.NewValue is SettingsViewModel newVm)
        {
            newVm.ClosingRequested += OnClosingRequested;
            // Keep the PasswordBox in sync with the loaded value; since
            // PasswordBox.Password is not a DependencyProperty we do it by hand.
            ApiKeyBox.Password = newVm.ApiKey ?? string.Empty;
        }
    }

    private void OnClosingRequested(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ApiKeyBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.ApiKey = ApiKeyBox.Password;
    }
}

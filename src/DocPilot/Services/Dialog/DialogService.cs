using System.Windows;
using Microsoft.Win32;

namespace DocPilot.Services.Dialog;

/// <summary>
/// Default <see cref="IDialogService"/> that wraps WPF's built-in dialogs.
/// </summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc />
    public void ShowInfo(string title, string message) =>
        Show(title, message, MessageBoxImage.Information);

    /// <inheritdoc />
    public void ShowWarning(string title, string message) =>
        Show(title, message, MessageBoxImage.Warning);

    /// <inheritdoc />
    public void ShowError(string title, string message) =>
        Show(title, message, MessageBoxImage.Error);

    /// <inheritdoc />
    public bool Confirm(string title, string message)
    {
        var result = MessageBox.Show(
            GetOwner(),
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);
        return result == MessageBoxResult.Yes;
    }

    /// <inheritdoc />
    public string? PickOpenFile(string filter, string? title = null)
    {
        var dlg = new OpenFileDialog
        {
            Filter = filter,
            Title = title ?? "Open file",
            CheckFileExists = true,
            Multiselect = false,
        };
        return dlg.ShowDialog(GetOwner()) == true ? dlg.FileName : null;
    }

    /// <inheritdoc />
    public string? PickSaveFile(string filter, string defaultFileName, string? title = null)
    {
        var dlg = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName,
            Title = title ?? "Save as",
            OverwritePrompt = true,
        };
        return dlg.ShowDialog(GetOwner()) == true ? dlg.FileName : null;
    }

    private static Window? GetOwner() =>
        Application.Current?.Windows.Count > 0 ? Application.Current.MainWindow : null;

    private static void Show(string title, string message, MessageBoxImage icon) =>
        MessageBox.Show(GetOwner(), message, title, MessageBoxButton.OK, icon);
}

namespace DocPilot.Services.Dialog;

/// <summary>
/// UI-layer façade for message boxes and file pickers. Injecting this keeps
/// view-models free of WPF dialog calls and unit-testable.
/// </summary>
public interface IDialogService
{
    /// <summary>Show an information-style alert.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Body text.</param>
    void ShowInfo(string title, string message);

    /// <summary>Show a warning-style alert.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Body text.</param>
    void ShowWarning(string title, string message);

    /// <summary>Show an error-style alert.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Body text.</param>
    void ShowError(string title, string message);

    /// <summary>Ask the user to confirm a yes/no question.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Body text.</param>
    bool Confirm(string title, string message);

    /// <summary>Open an "Open file" dialog and return the chosen path or <c>null</c>.</summary>
    /// <param name="filter">Win32 filter string (e.g. <c>"Docs|*.pdf;*.docx"</c>).</param>
    /// <param name="title">Dialog title.</param>
    string? PickOpenFile(string filter, string? title = null);

    /// <summary>Open a "Save as" dialog and return the chosen path or <c>null</c>.</summary>
    /// <param name="filter">Win32 filter string.</param>
    /// <param name="defaultFileName">Initial file name (no directory).</param>
    /// <param name="title">Dialog title.</param>
    string? PickSaveFile(string filter, string defaultFileName, string? title = null);
}

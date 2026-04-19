using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocPilot.Models;

namespace DocPilot.ViewModels;

/// <summary>
/// Drives the left-hand document preview pane: current page text, pagination
/// controls, and the footer counter.
/// </summary>
public sealed partial class DocumentViewModel : ViewModelBase
{
    [ObservableProperty]
    private Document? _document;

    [ObservableProperty]
    private int _currentPageIndex;

    /// <summary>Create the view-model with no document loaded.</summary>
    public DocumentViewModel() { }

    /// <summary>Replace the currently displayed document and jump to page 1.</summary>
    /// <param name="document">New document, or <c>null</c> to clear the pane.</param>
    public void SetDocument(Document? document)
    {
        Document = document;
        CurrentPageIndex = 0;
        OnPropertyChanged(nameof(CurrentPageNumber));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(CurrentPageText));
        OnPropertyChanged(nameof(HasDocument));
        GoNextCommand.NotifyCanExecuteChanged();
        GoPreviousCommand.NotifyCanExecuteChanged();
    }

    /// <summary>True when a document is loaded and can be paginated.</summary>
    public bool HasDocument => Document is not null;

    /// <summary>Total number of pages (0 if no document).</summary>
    public int TotalPages => Document?.PageCount ?? 0;

    /// <summary>1-based page number shown in the footer.</summary>
    public int CurrentPageNumber => TotalPages == 0 ? 0 : CurrentPageIndex + 1;

    /// <summary>Text content of the current page.</summary>
    public string CurrentPageText =>
        Document is null || Document.Pages.Count == 0
            ? string.Empty
            : Document.Pages[Math.Clamp(CurrentPageIndex, 0, Document.Pages.Count - 1)].Text;

    partial void OnCurrentPageIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentPageNumber));
        OnPropertyChanged(nameof(CurrentPageText));
        GoNextCommand.NotifyCanExecuteChanged();
        GoPreviousCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Navigate to the previous page, if any.</summary>
    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void GoPrevious() => CurrentPageIndex--;

    private bool CanGoPrevious() => Document is not null && CurrentPageIndex > 0;

    /// <summary>Navigate to the next page, if any.</summary>
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext() => CurrentPageIndex++;

    private bool CanGoNext() => Document is not null && CurrentPageIndex < TotalPages - 1;
}

using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocPilot.Models;
using DocPilot.Resources;
using DocPilot.Services.AI;
using DocPilot.Services.Dialog;
using DocPilot.Services.Parsing;
using DocPilot.Services.Settings;
using DocPilot.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocPilot.ViewModels;

/// <summary>
/// Root view-model wiring the top-level shell together: exposes the child
/// document and chat view-models, coordinates file open / drop, and hosts the
/// Settings command.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase
{
    private const long MaxFileSizeBytes = 50L * 1024 * 1024;

    private readonly IDocumentParserFactory _parserFactory;
    private readonly ISampleDocumentProvider _sample;
    private readonly AIServiceRouter _aiRouter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;
    private readonly ILogger<MainViewModel> _logger;

    /// <summary>Document preview view-model (left pane).</summary>
    public DocumentViewModel DocumentVm { get; }

    /// <summary>Chat view-model (right pane).</summary>
    public ChatViewModel ChatVm { get; }

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _isParsing;

    [ObservableProperty]
    private bool _hasDocument;

    [ObservableProperty]
    private bool _isDemoMode;

    /// <summary>Create the shell view-model with injected dependencies.</summary>
    public MainViewModel(
        DocumentViewModel documentVm,
        ChatViewModel chatVm,
        IDocumentParserFactory parserFactory,
        ISampleDocumentProvider sample,
        AIServiceRouter aiRouter,
        IDialogService dialogs,
        IServiceProvider services,
        ILogger<MainViewModel> logger)
    {
        DocumentVm = documentVm;
        ChatVm = chatVm;
        _parserFactory = parserFactory;
        _sample = sample;
        _aiRouter = aiRouter;
        _dialogs = dialogs;
        _services = services;
        _logger = logger;
        StatusText = Strings.StatusReady;
        RefreshDemoMode();
    }

    /// <summary>Re-evaluate Demo Mode state (call after settings change).</summary>
    public void RefreshDemoMode() => IsDemoMode = _aiRouter.IsDemoMode;

    /// <summary>Opens a "pick document" dialog and loads the selection.</summary>
    [RelayCommand]
    private async Task OpenAsync()
    {
        var path = _dialogs.PickOpenFile(
            $"{Strings.SupportedDocumentsFilter}|*.pdf;*.docx;*.txt;*.md",
            Strings.OpenDocument);
        if (path is null) return;
        await LoadDocumentAsync(path).ConfigureAwait(true);
    }

    /// <summary>Loads a document from an arbitrary path (also used by drag-drop).</summary>
    /// <param name="path">Absolute path of the file to open.</param>
    public async Task LoadDocumentAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _dialogs.ShowWarning(Strings.ErrorTitle, Strings.ErrorFileNotFound);
            return;
        }

        var info = new FileInfo(path);
        if (info.Length > MaxFileSizeBytes)
        {
            _dialogs.ShowWarning(Strings.ErrorTitle, Strings.ErrorFileTooLarge);
            return;
        }

        var parser = _parserFactory.Resolve(path);
        if (parser is null)
        {
            _dialogs.ShowWarning(Strings.ErrorTitle, Strings.ErrorUnsupportedFormat);
            return;
        }

        try
        {
            IsParsing = true;
            StatusText = string.Format(Strings.StatusParsing, info.Name);
            var doc = await parser.ParseAsync(path).ConfigureAwait(true);
            DocumentVm.SetDocument(doc);
            await ChatVm.AttachDocumentAsync(doc).ConfigureAwait(true);
            StatusText = string.Format(Strings.StatusLoaded, doc.FileName, doc.PageCount);
            HasDocument = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse {Path}", path);
            _dialogs.ShowError(Strings.ErrorTitle, Strings.ErrorParseFailed);
            StatusText = Strings.StatusReady;
        }
        finally
        {
            IsParsing = false;
        }
    }

    /// <summary>Load the bundled demo document (useful when no files are on hand).</summary>
    [RelayCommand]
    private async Task TryDemoAsync()
    {
        var path = await _sample.EnsureAvailableAsync().ConfigureAwait(true);
        await LoadDocumentAsync(path).ConfigureAwait(true);
    }

    /// <summary>Opens the settings dialog as a modal.</summary>
    [RelayCommand]
    private void OpenSettings()
    {
        var dlg = _services.GetRequiredService<SettingsDialog>();
        dlg.Owner = System.Windows.Application.Current.MainWindow;
        dlg.DataContext = _services.GetRequiredService<SettingsViewModel>();
        dlg.ShowDialog();
        RefreshDemoMode();
    }
}

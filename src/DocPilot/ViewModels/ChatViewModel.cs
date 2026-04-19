using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocPilot.Models;
using DocPilot.Resources;
using DocPilot.Services.AI;
using DocPilot.Services.Dialog;
using DocPilot.Services.Export;
using DocPilot.Services.History;
using Microsoft.Extensions.Logging;

namespace DocPilot.ViewModels;

/// <summary>
/// Drives the right-hand chat pane: message list, input box, quick-action
/// buttons, streaming orchestration, and import/export of transcripts.
/// </summary>
public sealed partial class ChatViewModel : ViewModelBase
{
    private readonly IAIService _ai;
    private readonly IConversationHistoryService _history;
    private readonly IExportService _export;
    private readonly IDialogService _dialogs;
    private readonly ILogger<ChatViewModel> _logger;

    private Conversation _conversation = new();
    private CancellationTokenSource? _streamCts;

    /// <summary>Observable message list bound to the chat transcript.</summary>
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    /// <summary>Suggested open-ended prompts shown when the transcript is empty.</summary>
    public IReadOnlyList<string> SuggestedPrompts { get; } = new[]
    {
        "What is this document about in one paragraph?",
        "List the three most important points.",
        "Are there any open questions or gaps?",
        "Explain this to me like I'm a beginner.",
    };

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private Document? _document;

    [ObservableProperty]
    private bool _hasMessages;

    /// <summary>Create the view-model. All dependencies resolved from DI.</summary>
    public ChatViewModel(
        IAIService ai,
        IConversationHistoryService history,
        IExportService export,
        IDialogService dialogs,
        ILogger<ChatViewModel> logger)
    {
        _ai = ai;
        _history = history;
        _export = export;
        _dialogs = dialogs;
        _logger = logger;
        Messages.CollectionChanged += (_, _) => HasMessages = Messages.Count > 0;
    }

    /// <summary>Send a pre-written prompt as if the user had typed it.</summary>
    /// <param name="prompt">Prompt body.</param>
    [RelayCommand(CanExecute = nameof(CanSuggestion))]
    private Task SuggestionAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return Task.CompletedTask;
        return SendUserMessageAsync(prompt);
    }

    private bool CanSuggestion() => !IsBusy;

    /// <summary>
    /// Switch to a different document. Loads (or creates) the persisted
    /// conversation for that document so history survives app restarts.
    /// </summary>
    /// <param name="document">Newly opened document, or <c>null</c> to detach.</param>
    public async Task AttachDocumentAsync(Document? document)
    {
        Document = document;
        Messages.Clear();

        if (document is null)
        {
            _conversation = new Conversation();
            return;
        }

        var existing = await _history.FindByDocumentAsync(document.FilePath).ConfigureAwait(true);
        if (existing is not null)
        {
            _conversation = existing;
            foreach (var m in existing.Messages)
                Messages.Add(m);
        }
        else
        {
            _conversation = new Conversation
            {
                DocumentPath = document.FilePath,
                Title = document.FileName,
            };
        }
    }

    /// <summary>Send the composed input text as a new user message.</summary>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var text = InputText.Trim();
        if (string.IsNullOrEmpty(text)) return;
        InputText = string.Empty;
        await SendUserMessageAsync(text).ConfigureAwait(true);
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

    /// <summary>Cancel the in-flight streaming response, if any.</summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _streamCts?.Cancel();

    private bool CanStop() => IsBusy;

    /// <summary>Clear the current transcript (does not delete from disk).</summary>
    [RelayCommand]
    private void Clear()
    {
        if (Messages.Count == 0) return;
        if (!_dialogs.Confirm(Strings.ConfirmClearTitle, Strings.ConfirmClearBody)) return;
        Messages.Clear();
        _conversation.Messages.Clear();
        _ = PersistAsync();
    }

    /// <summary>Run one of the preset quick-action prompts.</summary>
    /// <param name="action">Which preset to run.</param>
    [RelayCommand(CanExecute = nameof(CanQuickAction))]
    private async Task QuickActionAsync(QuickAction action)
    {
        var prompt = action switch
        {
            QuickAction.Summarize =>
                "Summarise the document in a clear, structured way. Include key points and any open questions.",
            QuickAction.Translate =>
                "Translate the document into fluent English, preserving structure. If it is already in English, offer a Chinese translation instead.",
            QuickAction.AskQuestions =>
                "Suggest 5 insightful questions I could ask about this document, as a numbered Markdown list.",
            _ => string.Empty,
        };
        if (string.IsNullOrEmpty(prompt)) return;
        await SendUserMessageAsync(prompt).ConfigureAwait(true);
    }

    private bool CanQuickAction() => !IsBusy && Document is not null;

    /// <summary>Save transcript as Markdown via Save-As dialog.</summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportMarkdownAsync()
    {
        var path = _dialogs.PickSaveFile(
            $"{Strings.MarkdownFilter}|*.md",
            _export.SuggestDefaultName() + ".md",
            Strings.ExportAsMarkdown);
        if (path is null) return;
        await _export.ExportMarkdownAsync(path, Messages, Document?.FileName).ConfigureAwait(true);
    }

    /// <summary>Save transcript as plain text via Save-As dialog.</summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportTextAsync()
    {
        var path = _dialogs.PickSaveFile(
            $"{Strings.TextFilter}|*.txt",
            _export.SuggestDefaultName() + ".txt",
            Strings.ExportAsText);
        if (path is null) return;
        await _export.ExportTextAsync(path, Messages, Document?.FileName).ConfigureAwait(true);
    }

    private bool CanExport() => Messages.Count > 0;

    partial void OnInputTextChanged(string value) => SendCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
    {
        SendCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        QuickActionCommand.NotifyCanExecuteChanged();
        SuggestionCommand.NotifyCanExecuteChanged();
    }

    partial void OnDocumentChanged(Document? value) =>
        QuickActionCommand.NotifyCanExecuteChanged();

    private async Task SendUserMessageAsync(string text)
    {
        var userMsg = ChatMessage.FromUser(text);
        Messages.Add(userMsg);
        _conversation.Messages.Add(userMsg);

        var assistantMsg = ChatMessage.CreateAssistantPlaceholder();
        Messages.Add(assistantMsg);

        IsBusy = true;
        _streamCts = new CancellationTokenSource();

        try
        {
            var history = Messages
                .Where(m => m.Role is MessageRole.User or MessageRole.Assistant)
                .Where(m => m.Id != userMsg.Id && m.Id != assistantMsg.Id)
                .ToList();

            var docContext = Document?.FullText ?? string.Empty;

            await foreach (var chunk in _ai
                .SendMessageStreamAsync(docContext, history, text, _streamCts.Token)
                .ConfigureAwait(true))
            {
                assistantMsg.Content += chunk;
            }

            assistantMsg.IsStreaming = false;

            if (!string.IsNullOrEmpty(assistantMsg.Content))
                _conversation.Messages.Add(assistantMsg);
            else
                Messages.Remove(assistantMsg);
        }
        catch (OperationCanceledException)
        {
            assistantMsg.IsStreaming = false;
            if (string.IsNullOrEmpty(assistantMsg.Content))
                Messages.Remove(assistantMsg);
        }
        catch (InvalidOperationException ex)
        {
            Messages.Remove(assistantMsg);
            Messages.Add(ChatMessage.FromError(ex.Message));
            _dialogs.ShowWarning(Strings.ErrorTitle, Strings.ErrorApiKeyMissing);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network failure during chat.");
            Messages.Remove(assistantMsg);
            Messages.Add(ChatMessage.FromError(Strings.ErrorNetwork));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during chat.");
            Messages.Remove(assistantMsg);
            Messages.Add(ChatMessage.FromError($"{Strings.ErrorUnexpected}: {ex.Message}"));
        }
        finally
        {
            IsBusy = false;
            _streamCts?.Dispose();
            _streamCts = null;
            ExportMarkdownCommand.NotifyCanExecuteChanged();
            ExportTextCommand.NotifyCanExecuteChanged();
            await PersistAsync().ConfigureAwait(true);
        }
    }

    private async Task PersistAsync()
    {
        if (string.IsNullOrEmpty(_conversation.DocumentPath)) return;
        try
        {
            await _history.SaveAsync(_conversation).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist conversation.");
        }
    }
}

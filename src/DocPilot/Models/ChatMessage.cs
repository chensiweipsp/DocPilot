using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DocPilot.Models;

/// <summary>
/// A single message inside a conversation transcript.
/// </summary>
/// <remarks>
/// Inherits <see cref="ObservableObject"/> so the UI can observe incremental
/// updates while the assistant streams a response token-by-token.
/// </remarks>
public partial class ChatMessage : ObservableObject
{
    /// <summary>Stable identifier for persistence and diffing.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Role of the author (user, assistant, system, error).</summary>
    public MessageRole Role { get; init; }

    /// <summary>UTC timestamp when the message was created.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Message body. Backed by an observable property so the chat view can
    /// re-render as tokens arrive from the streaming API.
    /// </summary>
    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>
    /// True while the assistant is still streaming tokens into this message.
    /// Drives the "thinking" indicator animation in the UI.
    /// </summary>
    [ObservableProperty]
    private bool _isStreaming;

    /// <summary>
    /// True if this message carried an error (network failure, API rejection…).
    /// Renders in the error colour.
    /// </summary>
    public bool IsError => Role == MessageRole.Error;

    /// <summary>Create a new user message with the given text.</summary>
    /// <param name="content">Plain-text user input.</param>
    public static ChatMessage FromUser(string content) =>
        new() { Role = MessageRole.User, Content = content };

    /// <summary>Create a new assistant message that will be streamed into.</summary>
    public static ChatMessage CreateAssistantPlaceholder() =>
        new() { Role = MessageRole.Assistant, Content = string.Empty, IsStreaming = true };

    /// <summary>Create a new inline error message.</summary>
    /// <param name="content">Error text to show to the user.</param>
    public static ChatMessage FromError(string content) =>
        new() { Role = MessageRole.Error, Content = content };
}

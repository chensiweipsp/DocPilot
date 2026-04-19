using System;
using System.Collections.Generic;

namespace DocPilot.Models;

/// <summary>
/// A persisted chat transcript tied to a document.
/// </summary>
public sealed class Conversation
{
    /// <summary>Stable identifier (used as the on-disk filename).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Absolute path of the document this conversation is about.</summary>
    public string DocumentPath { get; set; } = string.Empty;

    /// <summary>Display name shown in the history list (defaults to file name).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>UTC time the conversation was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>UTC time of the most recent message.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Ordered list of messages in the transcript.</summary>
    public List<ChatMessage> Messages { get; set; } = new();
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.History;

/// <summary>
/// Persists and retrieves saved <see cref="Conversation"/> transcripts under
/// <c>%AppData%\DocPilot\conversations\</c>.
/// </summary>
public interface IConversationHistoryService
{
    /// <summary>Enumerate all stored conversations, newest first.</summary>
    Task<IReadOnlyList<Conversation>> ListAsync();

    /// <summary>
    /// Look up the most recent conversation associated with the given document
    /// path. Returns <c>null</c> if no transcript has been saved for it yet.
    /// </summary>
    /// <param name="documentPath">Absolute path of the document.</param>
    Task<Conversation?> FindByDocumentAsync(string documentPath);

    /// <summary>Insert or overwrite a conversation on disk.</summary>
    /// <param name="conversation">Transcript to save.</param>
    Task SaveAsync(Conversation conversation);

    /// <summary>Remove a saved conversation.</summary>
    /// <param name="id">Conversation identifier.</param>
    Task DeleteAsync(Guid id);
}

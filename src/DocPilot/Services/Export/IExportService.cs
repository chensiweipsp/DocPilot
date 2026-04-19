using System.Collections.Generic;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.Export;

/// <summary>
/// Writes a conversation transcript to disk as Markdown or plain text.
/// </summary>
public interface IExportService
{
    /// <summary>Render the transcript as Markdown and save to <paramref name="path"/>.</summary>
    /// <param name="path">Destination file path.</param>
    /// <param name="messages">Transcript to export.</param>
    /// <param name="documentName">Optional document name for the header.</param>
    Task ExportMarkdownAsync(string path, IEnumerable<ChatMessage> messages, string? documentName = null);

    /// <summary>Render the transcript as plain text and save to <paramref name="path"/>.</summary>
    /// <param name="path">Destination file path.</param>
    /// <param name="messages">Transcript to export.</param>
    /// <param name="documentName">Optional document name for the header.</param>
    Task ExportTextAsync(string path, IEnumerable<ChatMessage> messages, string? documentName = null);

    /// <summary>Return the suggested default file name (no extension).</summary>
    string SuggestDefaultName();
}

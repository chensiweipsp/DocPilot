using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.Export;

/// <summary>
/// Default <see cref="IExportService"/>. Stateless, writes UTF-8 files.
/// </summary>
public sealed class ExportService : IExportService
{
    /// <inheritdoc />
    public Task ExportMarkdownAsync(string path, IEnumerable<ChatMessage> messages, string? documentName = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(messages);

        var sb = new StringBuilder();
        sb.AppendLine("# DocPilot Conversation");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(documentName))
        {
            sb.Append("**Document:** ").AppendLine(documentName);
        }
        sb.Append("**Exported:** ").AppendLine(DateTimeOffset.Now.ToString("u"));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var m in messages)
        {
            if (m.Role == MessageRole.Error)
                sb.Append("> [!WARNING] ").AppendLine(m.Content);
            else
                sb.Append("### ").AppendLine(RoleLabel(m.Role));
            sb.AppendLine();
            sb.AppendLine(m.Content);
            sb.AppendLine();
        }

        return File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
    }

    /// <inheritdoc />
    public Task ExportTextAsync(string path, IEnumerable<ChatMessage> messages, string? documentName = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(messages);

        var sb = new StringBuilder();
        sb.AppendLine("DocPilot Conversation");
        if (!string.IsNullOrEmpty(documentName))
            sb.Append("Document: ").AppendLine(documentName);
        sb.Append("Exported: ").AppendLine(DateTimeOffset.Now.ToString("u"));
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();

        foreach (var m in messages)
        {
            sb.Append('[').Append(RoleLabel(m.Role)).Append(']').Append(' ')
              .AppendLine(m.Timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine(m.Content);
            sb.AppendLine();
            sb.AppendLine(new string('-', 60));
            sb.AppendLine();
        }

        return File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
    }

    /// <inheritdoc />
    public string SuggestDefaultName() =>
        $"DocPilot_Conversation_{DateTime.Now:yyyyMMdd_HHmmss}";

    private static string RoleLabel(MessageRole role) => role switch
    {
        MessageRole.User => "User",
        MessageRole.Assistant => "Assistant",
        MessageRole.System => "System",
        MessageRole.Error => "Error",
        _ => role.ToString(),
    };
}

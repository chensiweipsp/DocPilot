using System;
using System.Collections.Generic;
using System.Linq;

namespace DocPilot.Models;

/// <summary>
/// A parsed document loaded into DocPilot.
/// </summary>
public sealed class Document
{
    /// <summary>Absolute filesystem path of the source file.</summary>
    public required string FilePath { get; init; }

    /// <summary>User-visible file name (including extension).</summary>
    public required string FileName { get; init; }

    /// <summary>Resolved document format.</summary>
    public required DocumentType Type { get; init; }

    /// <summary>Ordered pages of extracted text.</summary>
    public required IReadOnlyList<DocumentPage> Pages { get; init; }

    /// <summary>File size in bytes as reported by the filesystem.</summary>
    public long SizeBytes { get; init; }

    /// <summary>UTC timestamp of when this document was loaded into the app.</summary>
    public DateTimeOffset LoadedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Concatenated text of every page, separated by blank lines.</summary>
    public string FullText => string.Join("\n\n", Pages.Select(p => p.Text));

    /// <summary>Total number of pages in the document.</summary>
    public int PageCount => Pages.Count;
}

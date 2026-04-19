using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Plain-text parser. Splits the file into <c>~4000-character</c> virtual pages
/// so the preview pane has sane pagination regardless of the source file.
/// </summary>
public sealed class TxtParser : IDocumentParser
{
    private const int VirtualPageSize = 4000;

    private static readonly string[] Extensions =
        new[] { ".txt", ".md", ".markdown", ".log", ".csv", ".json", ".xml" };

    /// <inheritdoc />
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        var ext = Path.GetExtension(filePath);
        return Array.Exists(Extensions, e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<Document> ParseAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        var info = new FileInfo(filePath);
        var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct).ConfigureAwait(false);

        var pages = Paginate(text);

        return new Document
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            Type = DocumentType.Text,
            SizeBytes = info.Length,
            Pages = pages,
        };
    }

    private static DocumentPage[] Paginate(string text)
    {
        if (text.Length == 0)
            return new[] { new DocumentPage(1, string.Empty) };

        var count = (text.Length + VirtualPageSize - 1) / VirtualPageSize;
        var pages = new DocumentPage[count];
        for (var i = 0; i < count; i++)
        {
            var start = i * VirtualPageSize;
            var len = Math.Min(VirtualPageSize, text.Length - start);
            pages[i] = new DocumentPage(i + 1, text.Substring(start, len));
        }
        return pages;
    }
}

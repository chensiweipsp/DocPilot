using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Extracts text from PDF files using <see href="https://github.com/UglyToad/PdfPig">PdfPig</see>.
/// </summary>
public sealed class PdfParser : IDocumentParser
{
    /// <inheritdoc />
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        return string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<Document> ParseAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        return Task.Run(() => ExtractPages(filePath, ct), ct);
    }

    private static Document ExtractPages(string filePath, CancellationToken ct)
    {
        var info = new FileInfo(filePath);
        var pages = new List<DocumentPage>();

        using var pdf = PdfDocument.Open(filePath);
        foreach (Page page in pdf.GetPages())
        {
            ct.ThrowIfCancellationRequested();
            var text = page.Text ?? string.Empty;
            pages.Add(new DocumentPage(page.Number, text));
        }

        if (pages.Count == 0)
            pages.Add(new DocumentPage(1, string.Empty));

        return new Document
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            Type = DocumentType.Pdf,
            SizeBytes = info.Length,
            Pages = pages,
        };
    }
}

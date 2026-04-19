using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DpDocument = DocPilot.Models.Document;
using DpDocumentPage = DocPilot.Models.DocumentPage;
using DpDocumentType = DocPilot.Models.DocumentType;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Extracts text from .docx files using the OpenXML SDK. Paragraph breaks are
/// preserved; hard page breaks inside the document body start a new page.
/// </summary>
public sealed class DocxParser : IDocumentParser
{
    /// <inheritdoc />
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        return string.Equals(Path.GetExtension(filePath), ".docx", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<DpDocument> ParseAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        return Task.Run(() => ExtractPages(filePath, ct), ct);
    }

    private static DpDocument ExtractPages(string filePath, CancellationToken ct)
    {
        var info = new FileInfo(filePath);
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document.Body;

        var pages = new List<DpDocumentPage>();
        var currentPage = new StringBuilder();

        if (body is not null)
        {
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                ct.ThrowIfCancellationRequested();

                var startsNewPage = paragraph
                    .Descendants<Break>()
                    .Any(b => b.Type is not null && b.Type.Value == BreakValues.Page);

                if (startsNewPage && currentPage.Length > 0)
                {
                    pages.Add(new DpDocumentPage(pages.Count + 1, currentPage.ToString().TrimEnd()));
                    currentPage.Clear();
                }

                var text = paragraph.InnerText;
                if (!string.IsNullOrEmpty(text))
                    currentPage.AppendLine(text);
                else
                    currentPage.AppendLine();
            }
        }

        if (currentPage.Length > 0)
            pages.Add(new DpDocumentPage(pages.Count + 1, currentPage.ToString().TrimEnd()));

        if (pages.Count == 0)
            pages.Add(new DpDocumentPage(1, string.Empty));

        return new DpDocument
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            Type = DpDocumentType.Docx,
            SizeBytes = info.Length,
            Pages = pages,
        };
    }
}

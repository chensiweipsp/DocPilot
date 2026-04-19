using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocPilot.Models;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Resolves the first registered <see cref="IDocumentParser"/> that claims a
/// given file. Parsers are injected by the DI container.
/// </summary>
public sealed class DocumentParserFactory : IDocumentParserFactory
{
    private readonly IReadOnlyList<IDocumentParser> _parsers;

    /// <summary>Create the factory with a collection of registered parsers.</summary>
    /// <param name="parsers">All parser implementations registered in DI.</param>
    public DocumentParserFactory(IEnumerable<IDocumentParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    /// <inheritdoc />
    public IDocumentParser? Resolve(string filePath) =>
        _parsers.FirstOrDefault(p => p.CanParse(filePath));

    /// <inheritdoc />
    public DocumentType DetectType(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return DocumentType.Unknown;
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => DocumentType.Pdf,
            ".docx" => DocumentType.Docx,
            ".txt" or ".md" or ".markdown" or ".log" or ".csv" or ".json" or ".xml"
                => DocumentType.Text,
            _ => DocumentType.Unknown,
        };
    }
}

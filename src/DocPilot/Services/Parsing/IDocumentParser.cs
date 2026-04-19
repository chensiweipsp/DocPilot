using System.Threading;
using System.Threading.Tasks;
using DocPilot.Models;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Parses a file on disk into a <see cref="Document"/>. One implementation per
/// supported format; selected by <see cref="IDocumentParserFactory"/>.
/// </summary>
public interface IDocumentParser
{
    /// <summary>Returns true if this parser can handle the given file.</summary>
    /// <param name="filePath">Absolute path to the candidate file.</param>
    bool CanParse(string filePath);

    /// <summary>Parse the file and extract text page-by-page.</summary>
    /// <param name="filePath">Absolute path to the candidate file.</param>
    /// <param name="ct">Token to cancel the parse.</param>
    Task<Document> ParseAsync(string filePath, CancellationToken ct = default);
}

/// <summary>
/// Resolves the correct <see cref="IDocumentParser"/> for a given file extension.
/// </summary>
public interface IDocumentParserFactory
{
    /// <summary>Return a parser for <paramref name="filePath"/> or <c>null</c> if unsupported.</summary>
    /// <param name="filePath">File whose extension determines the parser.</param>
    IDocumentParser? Resolve(string filePath);

    /// <summary>Infer the <see cref="DocumentType"/> purely from extension.</summary>
    /// <param name="filePath">Path whose extension is inspected.</param>
    DocumentType DetectType(string filePath);
}

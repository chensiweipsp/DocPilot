using DocPilot.Models;
using DocPilot.Services.Parsing;
using FluentAssertions;
using Xunit;

namespace DocPilot.Tests.Parsing;

public sealed class DocumentParserFactoryTests
{
    private static DocumentParserFactory CreateSut() => new(new IDocumentParser[]
    {
        new PdfParser(),
        new DocxParser(),
        new TxtParser(),
    });

    [Theory]
    [InlineData("a.pdf", typeof(PdfParser))]
    [InlineData("A.PDF", typeof(PdfParser))]
    [InlineData("a.docx", typeof(DocxParser))]
    [InlineData("a.txt", typeof(TxtParser))]
    [InlineData("a.md", typeof(TxtParser))]
    public void Resolve_returns_matching_parser(string path, System.Type expected)
    {
        var parser = CreateSut().Resolve(path);
        parser.Should().NotBeNull().And.BeOfType(expected);
    }

    [Fact]
    public void Resolve_returns_null_for_unsupported()
    {
        CreateSut().Resolve("a.xyz").Should().BeNull();
    }

    [Theory]
    [InlineData("a.pdf", DocumentType.Pdf)]
    [InlineData("a.docx", DocumentType.Docx)]
    [InlineData("a.txt", DocumentType.Text)]
    [InlineData("a.md", DocumentType.Text)]
    [InlineData("a.unknown", DocumentType.Unknown)]
    [InlineData("", DocumentType.Unknown)]
    public void DetectType_matches_extension(string path, DocumentType expected)
    {
        CreateSut().DetectType(path).Should().Be(expected);
    }
}

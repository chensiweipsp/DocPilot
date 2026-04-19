using System.IO;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.Parsing;
using FluentAssertions;
using Xunit;

namespace DocPilot.Tests.Parsing;

public sealed class TxtParserTests
{
    [Theory]
    [InlineData("file.txt", true)]
    [InlineData("file.md", true)]
    [InlineData("file.log", true)]
    [InlineData("file.pdf", false)]
    [InlineData("file.docx", false)]
    [InlineData("", false)]
    public void CanParse_recognises_extension(string path, bool expected)
    {
        new TxtParser().CanParse(path).Should().Be(expected);
    }

    [Fact]
    public async Task ParseAsync_extracts_full_text()
    {
        var path = Path.GetTempFileName();
        const string content = "Hello\nWorld";
        await File.WriteAllTextAsync(path, content);
        try
        {
            var doc = await new TxtParser().ParseAsync(path);

            doc.Type.Should().Be(DocumentType.Text);
            doc.FileName.Should().Be(Path.GetFileName(path));
            doc.FullText.Should().Contain("Hello");
            doc.FullText.Should().Contain("World");
            doc.PageCount.Should().BeGreaterThan(0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ParseAsync_paginates_long_text()
    {
        var path = Path.GetTempFileName();
        var content = new string('a', 10_000);
        await File.WriteAllTextAsync(path, content);
        try
        {
            var doc = await new TxtParser().ParseAsync(path);
            doc.PageCount.Should().BeGreaterOrEqualTo(2);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

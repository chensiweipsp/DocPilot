using System;
using System.IO;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.Export;
using FluentAssertions;
using Xunit;

namespace DocPilot.Tests.Services;

public sealed class ExportServiceTests
{
    [Fact]
    public async Task ExportMarkdownAsync_writes_expected_sections()
    {
        var path = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}.md");
        try
        {
            var messages = new[]
            {
                ChatMessage.FromUser("Hi"),
                new ChatMessage { Role = MessageRole.Assistant, Content = "Hello there" },
            };
            await new ExportService().ExportMarkdownAsync(path, messages, "doc.pdf");

            var text = await File.ReadAllTextAsync(path);
            text.Should().Contain("# DocPilot Conversation");
            text.Should().Contain("**Document:** doc.pdf");
            text.Should().Contain("### User");
            text.Should().Contain("### Assistant");
            text.Should().Contain("Hi");
            text.Should().Contain("Hello there");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExportTextAsync_writes_plain_sections()
    {
        var path = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}.txt");
        try
        {
            var messages = new[] { ChatMessage.FromUser("Hi") };
            await new ExportService().ExportTextAsync(path, messages);
            (await File.ReadAllTextAsync(path)).Should().Contain("[User]").And.Contain("Hi");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void SuggestDefaultName_has_date_prefix()
    {
        new ExportService().SuggestDefaultName().Should().StartWith("DocPilot_Conversation_");
    }
}

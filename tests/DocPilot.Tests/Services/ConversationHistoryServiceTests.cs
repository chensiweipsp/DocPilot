using System;
using System.IO;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.History;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DocPilot.Tests.Services;

public sealed class ConversationHistoryServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ConversationHistoryServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docpilot-hist-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private ConversationHistoryService CreateSut() =>
        new(NullLogger<ConversationHistoryService>.Instance, _tempDir);

    [Fact]
    public async Task SaveAsync_then_FindByDocumentAsync_round_trips()
    {
        var sut = CreateSut();
        var convo = new Conversation
        {
            DocumentPath = @"C:\docs\a.pdf",
            Title = "a.pdf",
        };
        convo.Messages.Add(ChatMessage.FromUser("hi"));

        await sut.SaveAsync(convo);
        var found = await sut.FindByDocumentAsync(@"C:\docs\a.pdf");

        found.Should().NotBeNull();
        found!.Id.Should().Be(convo.Id);
        found.Messages.Should().ContainSingle().Which.Content.Should().Be("hi");
    }

    [Fact]
    public async Task ListAsync_orders_by_updated_desc()
    {
        var sut = CreateSut();
        var older = new Conversation { DocumentPath = "a", Title = "A" };
        var newer = new Conversation { DocumentPath = "b", Title = "B" };
        await sut.SaveAsync(older);
        await Task.Delay(50);
        await sut.SaveAsync(newer);

        var all = await sut.ListAsync();
        all.Should().HaveCount(2);
        all[0].Title.Should().Be("B");
    }

    [Fact]
    public async Task DeleteAsync_removes_file()
    {
        var sut = CreateSut();
        var convo = new Conversation { DocumentPath = "x", Title = "X" };
        await sut.SaveAsync(convo);
        await sut.DeleteAsync(convo.Id);
        (await sut.ListAsync()).Should().BeEmpty();
    }
}

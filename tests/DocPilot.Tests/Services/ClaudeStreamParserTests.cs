using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocPilot.Services.AI;
using FluentAssertions;
using Xunit;

namespace DocPilot.Tests.Services;

public sealed class ClaudeStreamParserTests
{
    [Fact]
    public async Task Parses_content_block_delta_events()
    {
        var sse = string.Join("\n", new[]
        {
            "event: message_start",
            "data: {\"type\":\"message_start\"}",
            "",
            "event: content_block_delta",
            "data: {\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}",
            "",
            "event: content_block_delta",
            "data: {\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\", world!\"}}",
            "",
            "event: message_stop",
            "data: {\"type\":\"message_stop\"}",
            "",
        });

        using var reader = new StringReader(sse);
        var chunks = new List<string>();
        await foreach (var chunk in ClaudeStreamParser.ReadDeltasAsync(reader))
            chunks.Add(chunk);

        chunks.Should().Equal("Hello", ", world!");
    }

    [Fact]
    public async Task Ignores_non_delta_events()
    {
        const string sse =
            "event: ping\ndata: {}\n\nevent: content_block_delta\ndata: {\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"x\"}}\n\n";

        using var reader = new StringReader(sse);
        var chunks = new List<string>();
        await foreach (var chunk in ClaudeStreamParser.ReadDeltasAsync(reader))
            chunks.Add(chunk);

        chunks.Should().Equal("x");
    }

    [Fact]
    public async Task Skips_malformed_json()
    {
        const string sse =
            "event: content_block_delta\ndata: {not-json\n\nevent: content_block_delta\ndata: {\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"ok\"}}\n\n";

        using var reader = new StringReader(sse);
        var chunks = new List<string>();
        await foreach (var chunk in ClaudeStreamParser.ReadDeltasAsync(reader))
            chunks.Add(chunk);

        chunks.Should().Equal("ok");
    }
}

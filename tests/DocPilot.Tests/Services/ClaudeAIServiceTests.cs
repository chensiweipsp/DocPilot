using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DocPilot.Models;
using DocPilot.Services.AI;
using FluentAssertions;
using Xunit;

namespace DocPilot.Tests.Services;

public sealed class ClaudeAIServiceTests
{
    [Fact]
    public void BuildRequestBody_includes_model_and_messages()
    {
        var settings = new AppSettings
        {
            Model = "claude-sonnet-4-5",
            MaxOutputTokens = 2048,
            DocumentContextCharLimit = 1000,
        };
        var history = new List<ChatMessage>
        {
            ChatMessage.FromUser("Hi"),
            new ChatMessage { Role = MessageRole.Assistant, Content = "Hello!" },
        };

        var json = ClaudeAIService.BuildRequestBody(
            settings,
            documentContext: "Some doc",
            history: history,
            userMessage: "What is this?",
            stream: true);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("model").GetString().Should().Be("claude-sonnet-4-5");
        root.GetProperty("max_tokens").GetInt32().Should().Be(2048);
        root.GetProperty("stream").GetBoolean().Should().BeTrue();
        root.GetProperty("system").GetString().Should().Contain("Some doc");

        var messages = root.GetProperty("messages").EnumerateArray().ToList();
        messages.Should().HaveCount(3);
        messages[0].GetProperty("role").GetString().Should().Be("user");
        messages[0].GetProperty("content").GetString().Should().Be("Hi");
        messages[1].GetProperty("role").GetString().Should().Be("assistant");
        messages[2].GetProperty("content").GetString().Should().Be("What is this?");
    }

    [Fact]
    public void BuildRequestBody_truncates_document_context()
    {
        var settings = new AppSettings { DocumentContextCharLimit = 100 };
        var big = new string('x', 500);

        var json = ClaudeAIService.BuildRequestBody(
            settings,
            documentContext: big,
            history: new List<ChatMessage>(),
            userMessage: "?",
            stream: false);

        using var doc = JsonDocument.Parse(json);
        var system = doc.RootElement.GetProperty("system").GetString()!;
        system.Should().Contain("truncated");
        system.Should().NotContain(new string('x', 200));
    }

    [Fact]
    public void BuildRequestBody_omits_stream_when_false()
    {
        var settings = new AppSettings();
        var json = ClaudeAIService.BuildRequestBody(
            settings,
            documentContext: "",
            history: new List<ChatMessage>(),
            userMessage: "ping",
            stream: false);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("stream", out _).Should().BeFalse();
    }
}

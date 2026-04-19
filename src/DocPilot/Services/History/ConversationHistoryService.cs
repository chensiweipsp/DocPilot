using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DocPilot.Models;
using Microsoft.Extensions.Logging;

namespace DocPilot.Services.History;

/// <summary>
/// File-backed conversation store. Each conversation lives in its own JSON file
/// named by <see cref="Conversation.Id"/>; discovery is a directory scan.
/// </summary>
public sealed class ConversationHistoryService : IConversationHistoryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ILogger<ConversationHistoryService> _logger;
    private readonly string _directory;

    /// <summary>Create the service using the default <c>%AppData%</c> directory.</summary>
    /// <param name="logger">DI logger.</param>
    public ConversationHistoryService(ILogger<ConversationHistoryService> logger)
        : this(logger, GetDefaultDirectory())
    {
    }

    /// <summary>Test-only constructor exposing the storage directory.</summary>
    /// <param name="logger">DI logger.</param>
    /// <param name="directory">Absolute storage directory.</param>
    public ConversationHistoryService(ILogger<ConversationHistoryService> logger, string directory)
    {
        _logger = logger;
        _directory = directory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Conversation>> ListAsync()
    {
        if (!Directory.Exists(_directory))
            return Array.Empty<Conversation>();

        var results = new List<Conversation>();
        foreach (var file in Directory.EnumerateFiles(_directory, "*.json"))
        {
            var convo = await TryReadAsync(file).ConfigureAwait(false);
            if (convo is not null)
                results.Add(convo);
        }

        return results.OrderByDescending(c => c.UpdatedAt).ToList();
    }

    /// <inheritdoc />
    public async Task<Conversation?> FindByDocumentAsync(string documentPath)
    {
        if (string.IsNullOrEmpty(documentPath))
            return null;

        var all = await ListAsync().ConfigureAwait(false);
        return all.FirstOrDefault(c =>
            string.Equals(c.DocumentPath, documentPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task SaveAsync(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);

        conversation.UpdatedAt = DateTimeOffset.UtcNow;

        var path = PathFor(conversation.Id);
        await using var stream = File.Create(path);
        await JsonSerializer
            .SerializeAsync(stream, conversation, JsonOptions)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid id)
    {
        var path = PathFor(id);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string PathFor(Guid id) => Path.Combine(_directory, id + ".json");

    private async Task<Conversation?> TryReadAsync(string path)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer
                .DeserializeAsync<Conversation>(stream, JsonOptions)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read conversation {Path}. Skipping.", path);
            return null;
        }
    }

    private static string GetDefaultDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "DocPilot", "conversations");
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DocPilot.Services.Parsing;

/// <summary>
/// Extracts the bundled sample document to a temp file so DocPilot can open
/// it through the regular parsing pipeline (great for screenshots and demos).
/// </summary>
public interface ISampleDocumentProvider
{
    /// <summary>
    /// Materialise the sample document on disk and return its absolute path.
    /// Subsequent calls reuse the same temp file.
    /// </summary>
    Task<string> EnsureAvailableAsync();
}

/// <summary>Default <see cref="ISampleDocumentProvider"/>.</summary>
public sealed class SampleDocumentProvider : ISampleDocumentProvider
{
    private const string ResourceName = "DocPilot.Resources.SampleDocument.md";
    private const string FileName = "DocPilot_Sample.md";

    private string? _cachedPath;

    /// <inheritdoc />
    public async Task<string> EnsureAvailableAsync()
    {
        if (_cachedPath is not null && File.Exists(_cachedPath))
            return _cachedPath;

        var dir = Path.Combine(Path.GetTempPath(), "DocPilot");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, FileName);

        var asm = Assembly.GetExecutingAssembly();
        await using var stream = asm.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} not found.");
        await using var fs = File.Create(path);
        await stream.CopyToAsync(fs).ConfigureAwait(false);

        _cachedPath = path;
        return path;
    }
}

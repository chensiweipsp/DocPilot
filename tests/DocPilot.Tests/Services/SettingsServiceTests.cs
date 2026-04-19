using System;
using System.IO;
using System.Threading.Tasks;
using DocPilot.Models;
using DocPilot.Services.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DocPilot.Tests.Services;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _tempPath;

    public SettingsServiceTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"docpilot-settings-{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    private SettingsService CreateSut() =>
        new(NullLogger<SettingsService>.Instance, _tempPath);

    [Fact]
    public async Task LoadAsync_returns_defaults_when_missing()
    {
        var sut = CreateSut();
        var loaded = await sut.LoadAsync();
        loaded.Should().NotBeNull();
        loaded.Model.Should().Be("claude-sonnet-4-5");
        loaded.Theme.Should().Be(ThemeMode.Dark);
    }

    [Fact]
    public async Task SaveAsync_then_LoadAsync_round_trips_values()
    {
        var sut = CreateSut();
        var settings = new AppSettings { Model = "claude-opus-4-5", Theme = ThemeMode.Light, Language = "zh-CN" };
        await sut.SaveAsync(settings);

        var loaded = await sut.LoadAsync();
        loaded.Model.Should().Be("claude-opus-4-5");
        loaded.Theme.Should().Be(ThemeMode.Light);
        loaded.Language.Should().Be("zh-CN");
    }

    [Fact]
    public async Task ApiKey_is_protected_on_disk()
    {
        var sut = CreateSut();
        var settings = new AppSettings();
        sut.SetApiKey(settings, "sk-secret-value-123");
        await sut.SaveAsync(settings);

        var raw = await File.ReadAllTextAsync(_tempPath);
        raw.Should().NotContain("sk-secret-value-123", "DPAPI must hide the plaintext");

        var loaded = await sut.LoadAsync();
        sut.GetApiKey(loaded).Should().Be("sk-secret-value-123");
    }

    [Fact]
    public void SetApiKey_null_clears_field()
    {
        var sut = CreateSut();
        var settings = new AppSettings();
        sut.SetApiKey(settings, "abc");
        sut.SetApiKey(settings, null);
        settings.ProtectedApiKey.Should().BeNull();
    }
}

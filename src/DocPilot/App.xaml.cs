using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using DocPilot.Services.AI;
using DocPilot.Services.Dialog;
using DocPilot.Services.Export;
using DocPilot.Services.History;
using DocPilot.Services.Localization;
using DocPilot.Services.Parsing;
using DocPilot.Services.Settings;
using DocPilot.Services.Theme;
using DocPilot.ViewModels;
using DocPilot.Views;
using DocPilot.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocPilot;

/// <summary>
/// Application entry point. Owns the dependency-injection host and routes
/// startup/shutdown lifecycle events into the container.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Gets the current application instance cast to <see cref="App"/>.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the root service provider. Throws if the host has not started yet.
    /// </summary>
    public IServiceProvider Services =>
        _host?.Services ?? throw new InvalidOperationException("Host has not been started.");

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => ConfigureServices(services))
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .Build();

        await _host.StartAsync().ConfigureAwait(true);

        // Apply the saved theme before we show any window.
        var settings = Services.GetRequiredService<ISettingsService>();
        var theme = Services.GetRequiredService<IThemeService>();
        var locale = Services.GetRequiredService<ILocalizationService>();
        var current = await settings.LoadAsync().ConfigureAwait(true);
        theme.Apply(current.Theme);
        locale.Apply(current.Language);

        var main = Services.GetRequiredService<MainWindow>();
        main.DataContext = Services.GetRequiredService<MainViewModel>();
        main.Show();
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Typed HttpClient for the live Claude client.
        services.AddHttpClient<ClaudeAIService>(client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // Demo service and the router picking between live / demo at runtime.
        services.AddSingleton<DemoAIService>();
        services.AddSingleton<AIServiceRouter>();
        services.AddSingleton<IAIService>(sp => sp.GetRequiredService<AIServiceRouter>());

        // Document parsers
        services.AddSingleton<IDocumentParser, PdfParser>();
        services.AddSingleton<IDocumentParser, DocxParser>();
        services.AddSingleton<IDocumentParser, TxtParser>();
        services.AddSingleton<IDocumentParserFactory, DocumentParserFactory>();
        services.AddSingleton<ISampleDocumentProvider, SampleDocumentProvider>();

        // Infrastructure services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IConversationHistoryService, ConversationHistoryService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels - transient so dialogs get fresh state
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddSingleton<DocumentViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Windows / Dialogs
        services.AddSingleton<MainWindow>();
        services.AddTransient<SettingsDialog>();
    }
}

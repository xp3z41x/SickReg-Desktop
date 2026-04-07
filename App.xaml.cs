using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SickReg.Desktop.Services;
using SickReg.Desktop.Services.Scanners;
using SickReg.Desktop.ViewModels;
using SickReg.Desktop.Views;
using SickReg.Desktop.Views.Pages;
using Wpf.Ui;

namespace SickReg.Desktop;

public partial class App : Application
{
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .UseSerilog((context, configuration) =>
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SickReg", "Logs", "sickreg-.log");
            configuration
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();

            // Scanner modules
            services.AddSingleton<IScannerModule, ComClsidScanner>();
            services.AddSingleton<IScannerModule, FileAssociationScanner>();
            services.AddSingleton<IScannerModule, SharedDllScanner>();
            services.AddSingleton<IScannerModule, ShellExtensionScanner>();
            services.AddSingleton<IScannerModule, StartupEntryScanner>();
            services.AddSingleton<IScannerModule, UninstallEntryScanner>();
            services.AddSingleton<IScannerModule, EmptyKeyScanner>();
            services.AddSingleton<IScannerModule, MuiCacheScanner>();
            services.AddSingleton<IScannerModule, AppPathScanner>();
            services.AddSingleton<IScannerModule, FontReferenceScanner>();
            services.AddSingleton<IScannerModule, HelpFileScanner>();
            services.AddSingleton<IScannerModule, FileTypeScanner>();

            // Services
            services.AddSingleton<IRegistryScannerService, RegistryScannerService>();
            services.AddSingleton<IRegistryBackupService, RegistryBackupService>();
            services.AddSingleton<IRegistryCleanerService, RegistryCleanerService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<ScanViewModel>();
            services.AddSingleton<ResultsViewModel>();
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<SettingsViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<DashboardPage>();
            services.AddSingleton<ScanPage>();
            services.AddSingleton<ResultsPage>();
            services.AddSingleton<BackupPage>();
            services.AddSingleton<SettingsPage>();
        })
        .Build();

    public static IServiceProvider Services => _host.Services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        await _host.StopAsync();
        _host.Dispose();
    }
}

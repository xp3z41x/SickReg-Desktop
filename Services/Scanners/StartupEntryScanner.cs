using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class StartupEntryScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.StartupEntry;
    public string DisplayName => "Entradas de Inicializacao";

    private static readonly string[] RunKeyPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\RunOnce",
    ];

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();

        foreach (var subPath in RunKeyPaths)
        {
            ct.ThrowIfCancellationRequested();
            ScanRunKey(Registry.LocalMachine, "HKEY_LOCAL_MACHINE", subPath, issues, ct);
            ScanRunKey(Registry.CurrentUser, "HKEY_CURRENT_USER", subPath, issues, ct);
        }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }

    private static void ScanRunKey(RegistryKey hive, string hiveName, string subPath, List<RegistryIssue> issues, CancellationToken ct)
    {
        try
        {
            using var key = hive.OpenSubKey(subPath);
            if (key == null) return;

            foreach (var valueName in key.GetValueNames())
            {
                ct.ThrowIfCancellationRequested();

                var value = key.GetValue(valueName)?.ToString();
                var exePath = RegistryPathValidator.ExtractFilePath(value);

                if (!string.IsNullOrEmpty(exePath) && !RegistryPathValidator.FileExistsAtPath(exePath))
                {
                    var fullPath = $@"{hiveName}\{subPath}";
                    if (RegistryPermissionHelper.CanWriteKey(fullPath))
                    {
                        issues.Add(new RegistryIssue
                        {
                            FullKeyPath = fullPath,
                            ValueName = valueName,
                            ValueData = value,
                            Description = $"Startup entry aponta para executavel inexistente: {exePath}",
                            Category = RegistryCategory.StartupEntry
                        });
                    }
                }
            }
        }
        catch (System.Security.SecurityException) { }
        catch (UnauthorizedAccessException) { }
    }
}

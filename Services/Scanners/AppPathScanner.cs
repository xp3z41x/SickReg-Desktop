using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class AppPathScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.AppPath;
    public string DisplayName => "Caminhos de Aplicativos";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        try
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(keyPath);
            if (baseKey == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            foreach (var subKeyName in baseKey.GetSubKeyNames())
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    using var appKey = baseKey.OpenSubKey(subKeyName);
                    if (appKey == null) continue;

                    var defaultValue = appKey.GetValue(null)?.ToString();
                    var exePath = RegistryPathValidator.ExtractFilePath(defaultValue);

                    if (!string.IsNullOrEmpty(exePath) && !RegistryPathValidator.FileExistsAtPath(exePath))
                    {
                        var fullPath = $@"HKEY_LOCAL_MACHINE\{keyPath}\{subKeyName}";
                        if (!SafetyList.IsProtectedPath(fullPath) && RegistryPermissionHelper.CanWriteKey(fullPath))
                        {
                            issues.Add(new RegistryIssue
                            {
                                FullKeyPath = fullPath,
                                ValueName = "",
                                ValueData = defaultValue,
                                Description = $"App Path '{subKeyName}' aponta para executavel inexistente: {exePath}",
                                Category = RegistryCategory.AppPath
                            });
                        }
                    }
                }
                catch (System.Security.SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

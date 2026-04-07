using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class UninstallEntryScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.UninstallEntry;
    public string DisplayName => "Entradas de Desinstalacao";

    private static readonly string[] UninstallPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    ];

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();

        foreach (var basePath in UninstallPaths)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using var baseKey = Registry.LocalMachine.OpenSubKey(basePath);
                if (baseKey == null) continue;

                foreach (var subKeyName in baseKey.GetSubKeyNames())
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        using var appKey = baseKey.OpenSubKey(subKeyName);
                        if (appKey == null) continue;

                        var displayName = appKey.GetValue("DisplayName")?.ToString() ?? subKeyName;
                        var installLocation = appKey.GetValue("InstallLocation")?.ToString();
                        var uninstallString = appKey.GetValue("UninstallString")?.ToString();

                        bool isOrphaned = false;
                        string reason = "";

                        if (!string.IsNullOrEmpty(installLocation))
                        {
                            var expanded = Environment.ExpandEnvironmentVariables(installLocation);
                            if (!RegistryPathValidator.DirectoryExistsAtPath(expanded))
                            {
                                isOrphaned = true;
                                reason = $"Diretorio de instalacao nao existe: {expanded}";
                            }
                        }

                        if (!isOrphaned && !string.IsNullOrEmpty(uninstallString))
                        {
                            var exePath = RegistryPathValidator.ExtractFilePath(uninstallString);
                            if (!string.IsNullOrEmpty(exePath) && !RegistryPathValidator.FileExistsAtPath(exePath))
                            {
                                isOrphaned = true;
                                reason = $"Desinstalador nao encontrado: {exePath}";
                            }
                        }

                        if (isOrphaned)
                        {
                            var fullPath = $@"HKEY_LOCAL_MACHINE\{basePath}\{subKeyName}";
                            if (!SafetyList.IsProtectedPath(fullPath) && RegistryPermissionHelper.CanWriteParentKey(fullPath))
                            {
                                issues.Add(new RegistryIssue
                                {
                                    FullKeyPath = fullPath,
                                    ValueData = displayName,
                                    Description = $"'{displayName}': {reason}",
                                    Category = RegistryCategory.UninstallEntry
                                });
                            }
                        }
                    }
                    catch (System.Security.SecurityException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (System.Security.SecurityException) { }
        }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

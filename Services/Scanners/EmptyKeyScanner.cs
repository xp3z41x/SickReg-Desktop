using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class EmptyKeyScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.EmptyKey;
    public string DisplayName => "Chaves Vazias";

    private static readonly (RegistryKey Hive, string HiveName, string SubPath)[] ScanTargets =
    [
        (Registry.CurrentUser, "HKEY_CURRENT_USER", "Software"),
        (Registry.LocalMachine, "HKEY_LOCAL_MACHINE", @"SOFTWARE\Classes"),
    ];

    private const int MaxDepth = 5;

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();

        foreach (var (hive, hiveName, subPath) in ScanTargets)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var rootKey = hive.OpenSubKey(subPath);
                if (rootKey == null) continue;
                ScanForEmptyKeys(rootKey, $@"{hiveName}\{subPath}", issues, ct, 0);
            }
            catch (System.Security.SecurityException) { }
        }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }

    private static void ScanForEmptyKeys(RegistryKey key, string currentPath, List<RegistryIssue> issues, CancellationToken ct, int depth)
    {
        if (depth > MaxDepth) return;

        try
        {
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                ct.ThrowIfCancellationRequested();

                var fullPath = $@"{currentPath}\{subKeyName}";
                if (SafetyList.IsProtectedPath(fullPath)) continue;

                try
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    if (subKey.SubKeyCount == 0 && subKey.ValueCount == 0)
                    {
                        if (RegistryPermissionHelper.CanWriteParentKey(fullPath))
                        {
                            issues.Add(new RegistryIssue
                            {
                                FullKeyPath = fullPath,
                                Description = "Chave de registro vazia (sem valores e sem subchaves)",
                                Category = RegistryCategory.EmptyKey
                            });
                        }
                    }
                    else
                    {
                        ScanForEmptyKeys(subKey, fullPath, issues, ct, depth + 1);
                    }
                }
                catch (System.Security.SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        catch (System.Security.SecurityException) { }
    }
}

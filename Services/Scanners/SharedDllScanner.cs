using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class SharedDllScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.SharedDll;
    public string DisplayName => "DLLs Compartilhadas";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs";
        var fullKeyPath = $@"HKEY_LOCAL_MACHINE\{keyPath}";

        try
        {
            if (!RegistryPermissionHelper.CanWriteKey(fullKeyPath))
                return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            foreach (var valueName in key.GetValueNames())
            {
                ct.ThrowIfCancellationRequested();

                var expandedPath = Environment.ExpandEnvironmentVariables(valueName);
                if (!RegistryPathValidator.FileExistsAtPath(expandedPath))
                {
                    if (!SafetyList.IsProtectedPath(fullKeyPath))
                    {
                        issues.Add(new RegistryIssue
                        {
                            FullKeyPath = fullKeyPath,
                            ValueName = valueName,
                            ValueData = key.GetValue(valueName)?.ToString(),
                            Description = $"DLL compartilhada nao encontrada: {expandedPath}",
                            Category = RegistryCategory.SharedDll
                        });
                    }
                }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

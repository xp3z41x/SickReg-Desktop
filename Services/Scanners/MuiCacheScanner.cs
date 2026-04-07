using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class MuiCacheScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.MuiCache;
    public string DisplayName => "MUI Cache";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            if (key == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            foreach (var valueName in key.GetValueNames())
            {
                ct.ThrowIfCancellationRequested();

                // MUI cache value names contain the executable path
                // Format: "C:\path\app.exe.FriendlyAppName" or just the path
                var dotExeIdx = valueName.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (dotExeIdx < 0) continue;

                var exePath = valueName[..(dotExeIdx + 4)];
                exePath = Environment.ExpandEnvironmentVariables(exePath);

                if (!RegistryPathValidator.FileExistsAtPath(exePath))
                {
                    issues.Add(new RegistryIssue
                    {
                        FullKeyPath = $@"HKEY_CURRENT_USER\{keyPath}",
                        ValueName = valueName,
                        ValueData = key.GetValue(valueName)?.ToString(),
                        Description = $"MUI cache referencia executavel removido: {exePath}",
                        Category = RegistryCategory.MuiCache
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

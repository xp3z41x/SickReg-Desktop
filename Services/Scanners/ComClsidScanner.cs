using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class ComClsidScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.ComClsid;
    public string DisplayName => "COM / ActiveX / CLSID";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();

        try
        {
            using var clsidKey = Registry.ClassesRoot.OpenSubKey("CLSID");
            if (clsidKey == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            var subKeyNames = clsidKey.GetSubKeyNames();
            for (int i = 0; i < subKeyNames.Length; i++)
            {
                ct.ThrowIfCancellationRequested();
                var guid = subKeyNames[i];

                try
                {
                    using var guidKey = clsidKey.OpenSubKey(guid);
                    if (guidKey == null) continue;

                    CheckServerPath(guidKey, guid, "InprocServer32", issues);
                    CheckServerPath(guidKey, guid, "LocalServer32", issues);
                }
                catch (System.Security.SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }

    private static void CheckServerPath(RegistryKey guidKey, string guid, string serverType, List<RegistryIssue> issues)
    {
        try
        {
            using var serverKey = guidKey.OpenSubKey(serverType);
            if (serverKey == null) return;

            var value = serverKey.GetValue(null)?.ToString();
            var path = RegistryPathValidator.ExtractFilePath(value);

            if (!string.IsNullOrEmpty(path) && !RegistryPathValidator.FileExistsAtPath(path))
            {
                // The issue is the default value pointing to a missing file.
                // Set ValueName = "" to delete just the default value, not the whole key.
                var fullPath = $@"HKEY_CLASSES_ROOT\CLSID\{guid}\{serverType}";
                if (!SafetyList.IsProtectedPath(fullPath) && RegistryPermissionHelper.CanWriteKey(fullPath))
                {
                    issues.Add(new RegistryIssue
                    {
                        FullKeyPath = fullPath,
                        ValueName = "",
                        ValueData = value,
                        Description = $"COM {serverType} referencia arquivo inexistente: {path}",
                        Category = RegistryCategory.ComClsid
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }
        catch (UnauthorizedAccessException) { }
    }
}

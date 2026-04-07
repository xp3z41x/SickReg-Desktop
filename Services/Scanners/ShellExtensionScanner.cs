using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class ShellExtensionScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.ShellExtension;
    public string DisplayName => "Extensoes de Shell";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved";
        var fullKeyPath = $@"HKEY_LOCAL_MACHINE\{keyPath}";

        try
        {
            if (!RegistryPermissionHelper.CanWriteKey(fullKeyPath))
                return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            foreach (var clsid in key.GetValueNames())
            {
                ct.ThrowIfCancellationRequested();

                if (!clsid.StartsWith('{')) continue;

                try
                {
                    using var clsidKey = Registry.ClassesRoot.OpenSubKey($@"CLSID\{clsid}\InprocServer32");
                    if (clsidKey == null)
                    {
                        issues.Add(new RegistryIssue
                        {
                            FullKeyPath = fullKeyPath,
                            ValueName = clsid,
                            ValueData = key.GetValue(clsid)?.ToString(),
                            Description = $"Extensao de shell referencia CLSID inexistente: {clsid}",
                            Category = RegistryCategory.ShellExtension
                        });
                        continue;
                    }

                    var dllPath = RegistryPathValidator.ExtractFilePath(clsidKey.GetValue(null)?.ToString());
                    if (!string.IsNullOrEmpty(dllPath) && !RegistryPathValidator.FileExistsAtPath(dllPath))
                    {
                        issues.Add(new RegistryIssue
                        {
                            FullKeyPath = fullKeyPath,
                            ValueName = clsid,
                            ValueData = dllPath,
                            Description = $"Extensao de shell aponta para DLL inexistente: {dllPath}",
                            Category = RegistryCategory.ShellExtension
                        });
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

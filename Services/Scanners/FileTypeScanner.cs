using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class FileTypeScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.FileType;
    public string DisplayName => "Tipos de Arquivo";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();

        try
        {
            var subKeyNames = Registry.ClassesRoot.GetSubKeyNames();
            foreach (var name in subKeyNames)
            {
                ct.ThrowIfCancellationRequested();

                if (!name.StartsWith('.') || SafetyList.IsProtectedExtension(name))
                    continue;

                try
                {
                    using var extKey = Registry.ClassesRoot.OpenSubKey(name);
                    if (extKey == null) continue;

                    using var openWithKey = extKey.OpenSubKey("OpenWithProgids");
                    if (openWithKey != null)
                    {
                        var fullPath = $@"HKEY_CLASSES_ROOT\{name}\OpenWithProgids";
                        if (!RegistryPermissionHelper.CanWriteKey(fullPath))
                            continue;

                        foreach (var progId in openWithKey.GetValueNames())
                        {
                            if (string.IsNullOrEmpty(progId)) continue;

                            using var progIdKey = Registry.ClassesRoot.OpenSubKey(progId);
                            if (progIdKey == null)
                            {
                                issues.Add(new RegistryIssue
                                {
                                    FullKeyPath = fullPath,
                                    ValueName = progId,
                                    Description = $"OpenWithProgids referencia ProgID inexistente: {progId}",
                                    Category = RegistryCategory.FileType
                                });
                            }
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

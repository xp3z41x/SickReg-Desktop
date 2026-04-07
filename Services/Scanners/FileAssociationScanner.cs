using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class FileAssociationScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.FileAssociation;
    public string DisplayName => "Associacoes de Arquivo";

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

                    var progId = extKey.GetValue(null)?.ToString();
                    if (string.IsNullOrEmpty(progId)) continue;

                    using var progIdKey = Registry.ClassesRoot.OpenSubKey(progId);
                    if (progIdKey == null)
                    {
                        var fullPath = $@"HKEY_CLASSES_ROOT\{name}";
                        if (RegistryPermissionHelper.CanWriteKey(fullPath))
                        {
                            issues.Add(new RegistryIssue
                            {
                                FullKeyPath = fullPath,
                                ValueName = "",
                                ValueData = progId,
                                Description = $"Extensao '{name}' referencia ProgID inexistente: {progId}",
                                Category = RegistryCategory.FileAssociation
                            });
                        }
                        continue;
                    }

                    using var cmdKey = progIdKey.OpenSubKey(@"shell\open\command");
                    if (cmdKey != null)
                    {
                        var cmdValue = cmdKey.GetValue(null)?.ToString();
                        var exePath = RegistryPathValidator.ExtractFilePath(cmdValue);
                        if (!string.IsNullOrEmpty(exePath) && !RegistryPathValidator.FileExistsAtPath(exePath))
                        {
                            var fullPath = $@"HKEY_CLASSES_ROOT\{progId}\shell\open\command";
                            if (RegistryPermissionHelper.CanWriteKey(fullPath))
                            {
                                issues.Add(new RegistryIssue
                                {
                                    FullKeyPath = fullPath,
                                    ValueName = "",
                                    ValueData = cmdValue,
                                    Description = $"Comando de '{name}' aponta para executavel inexistente: {exePath}",
                                    Category = RegistryCategory.FileAssociation
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

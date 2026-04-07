using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class HelpFileScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.HelpFile;
    public string DisplayName => "Arquivos de Ajuda";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"SOFTWARE\Microsoft\Windows\Help";
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

                var helpPath = key.GetValue(valueName)?.ToString();
                if (string.IsNullOrEmpty(helpPath)) continue;

                helpPath = Environment.ExpandEnvironmentVariables(helpPath);

                if (!RegistryPathValidator.DirectoryExistsAtPath(helpPath) &&
                    !RegistryPathValidator.FileExistsAtPath(helpPath))
                {
                    issues.Add(new RegistryIssue
                    {
                        FullKeyPath = fullKeyPath,
                        ValueName = valueName,
                        ValueData = helpPath,
                        Description = $"Arquivo de ajuda nao encontrado: {helpPath}",
                        Category = RegistryCategory.HelpFile
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

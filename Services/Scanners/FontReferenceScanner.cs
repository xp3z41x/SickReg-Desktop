using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public class FontReferenceScanner : IScannerModule
{
    public RegistryCategory Category => RegistryCategory.FontReference;
    public string DisplayName => "Referencias de Fontes";

    public Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var issues = new List<RegistryIssue>();
        const string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
        var fullKeyPath = $@"HKEY_LOCAL_MACHINE\{keyPath}";

        try
        {
            if (!RegistryPermissionHelper.CanWriteKey(fullKeyPath))
                return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) return Task.FromResult<IEnumerable<RegistryIssue>>(issues);

            var fontsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            var userFontsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "Windows", "Fonts");

            foreach (var valueName in key.GetValueNames())
            {
                ct.ThrowIfCancellationRequested();

                var fontFile = key.GetValue(valueName)?.ToString();
                if (string.IsNullOrEmpty(fontFile)) continue;

                string resolvedPath;
                if (Path.IsPathRooted(fontFile))
                    resolvedPath = Environment.ExpandEnvironmentVariables(fontFile);
                else
                    resolvedPath = Path.Combine(fontsDir, fontFile);

                if (!RegistryPathValidator.FileExistsAtPath(resolvedPath))
                {
                    var userFontPath = Path.Combine(userFontsDir, fontFile);
                    if (!RegistryPathValidator.FileExistsAtPath(userFontPath))
                    {
                        issues.Add(new RegistryIssue
                        {
                            FullKeyPath = fullKeyPath,
                            ValueName = valueName,
                            ValueData = fontFile,
                            Description = $"Fonte registrada nao encontrada: {fontFile}",
                            Category = RegistryCategory.FontReference
                        });
                    }
                }
            }
        }
        catch (System.Security.SecurityException) { }

        return Task.FromResult<IEnumerable<RegistryIssue>>(issues);
    }
}

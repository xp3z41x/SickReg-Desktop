using Microsoft.Extensions.Logging;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services;

public class RegistryBackupService(ILogger<RegistryBackupService> logger) : IRegistryBackupService
{
    public string BackupDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SickReg", "Backups");

    public async Task<string> BackupAsync(IEnumerable<RegistryIssue> issues)
    {
        Directory.CreateDirectory(BackupDirectory);

        var fileName = $"SickReg_Backup_{DateTime.Now:yyyy-MM-dd_HHmmss}.reg";
        var filePath = Path.Combine(BackupDirectory, fileName);

        var entries = issues.Select(i => (i.FullKeyPath, i.ValueName)).ToList();
        var content = RegFileExporter.GenerateRegFileContent(entries);

        await File.WriteAllTextAsync(filePath, content, System.Text.Encoding.Unicode);
        logger.LogInformation("Backup created at {Path} with {Count} entries", filePath, entries.Count);

        return filePath;
    }

    public IEnumerable<BackupEntry> GetBackups()
    {
        if (!Directory.Exists(BackupDirectory))
            return [];

        return Directory.GetFiles(BackupDirectory, "*.reg")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Select(f => new BackupEntry(f.FullName, f.Name, f.CreationTime, f.Length));
    }

    public void DeleteBackup(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            logger.LogInformation("Backup deleted: {Path}", filePath);
        }
    }
}

using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services;

public interface IRegistryBackupService
{
    Task<string> BackupAsync(IEnumerable<RegistryIssue> issues);
    IEnumerable<BackupEntry> GetBackups();
    void DeleteBackup(string filePath);
    string BackupDirectory { get; }
}

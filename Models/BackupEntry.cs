namespace SickReg.Desktop.Models;

public record BackupEntry(
    string FilePath,
    string FileName,
    DateTime CreatedAt,
    long FileSizeBytes)
{
    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1048576 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / 1048576.0:F1} MB"
    };
}

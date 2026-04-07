namespace SickReg.Desktop.Models;

public class ScanResult
{
    public List<RegistryIssue> Issues { get; init; } = [];
    public TimeSpan Duration { get; init; }
    public int TotalKeysScanned { get; init; }
    public DateTime ScanDate { get; init; } = DateTime.Now;

    public Dictionary<RegistryCategory, int> CategoryCounts =>
        Issues.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.Count());
}

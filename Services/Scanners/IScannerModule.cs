using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services.Scanners;

public interface IScannerModule
{
    RegistryCategory Category { get; }
    string DisplayName { get; }
    Task<IEnumerable<RegistryIssue>> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct);
}

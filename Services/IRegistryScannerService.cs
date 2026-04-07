using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services;

public interface IRegistryScannerService
{
    Task<ScanResult> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct);
}

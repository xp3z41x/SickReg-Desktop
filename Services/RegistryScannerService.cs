using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SickReg.Desktop.Models;
using SickReg.Desktop.Services.Scanners;

namespace SickReg.Desktop.Services;

public class RegistryScannerService(
    IEnumerable<IScannerModule> scanners,
    ILogger<RegistryScannerService> logger) : IRegistryScannerService
{
    public async Task<ScanResult> ScanAsync(IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var allIssues = new List<RegistryIssue>();
        var scannerList = scanners.ToList();
        var sw = Stopwatch.StartNew();
        var totalKeysScanned = 0;

        for (int i = 0; i < scannerList.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var scanner = scannerList[i];
            var overallPct = (double)i / scannerList.Count * 100;

            progress.Report(new ScanProgress(
                scanner.DisplayName,
                overallPct,
                allIssues.Count,
                $"Escaneando {scanner.DisplayName}..."));

            try
            {
                logger.LogInformation("Starting scanner: {Category}", scanner.Category);
                var issues = await Task.Run(() => scanner.ScanAsync(progress, ct), ct);
                var issueList = issues.ToList();
                allIssues.AddRange(issueList);
                logger.LogInformation("Scanner {Category} found {Count} issues", scanner.Category, issueList.Count);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scanner {Category} failed", scanner.Category);
            }
        }

        sw.Stop();

        progress.Report(new ScanProgress(
            "Concluido",
            100,
            allIssues.Count,
            $"Scan concluido. {allIssues.Count} problemas encontrados."));

        return new ScanResult
        {
            Issues = allIssues,
            Duration = sw.Elapsed,
            TotalKeysScanned = totalKeysScanned,
            ScanDate = DateTime.Now
        };
    }
}

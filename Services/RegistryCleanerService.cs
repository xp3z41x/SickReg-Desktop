using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SickReg.Desktop.Helpers;
using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services;

public class RegistryCleanerService(ILogger<RegistryCleanerService> logger) : IRegistryCleanerService
{
    public Task<(int succeeded, int failed)> CleanAsync(IEnumerable<RegistryIssue> issues)
    {
        int succeeded = 0, failed = 0;

        foreach (var issue in issues)
        {
            if (SafetyList.IsProtectedPath(issue.FullKeyPath))
            {
                logger.LogWarning("Skipped protected path: {Path}", issue.FullKeyPath);
                failed++;
                continue;
            }

            try
            {
                if (issue.ValueName != null)
                {
                    // ValueName is set: delete the specific value
                    using var key = RegFileExporter.OpenKeyFromFullPath(issue.FullKeyPath, writable: true);
                    if (key == null)
                    {
                        // Key no longer exists or inaccessible — already cleaned or never writable
                        logger.LogDebug("Key not found or not writable: {Path}", issue.FullKeyPath);
                        failed++;
                        continue;
                    }
                    key.DeleteValue(issue.ValueName, throwOnMissingValue: false);
                }
                else
                {
                    // No ValueName: delete the entire key
                    var lastSep = issue.FullKeyPath.LastIndexOf('\\');
                    if (lastSep < 0)
                    {
                        failed++;
                        continue;
                    }

                    var parentPath = issue.FullKeyPath[..lastSep];
                    var subKeyName = issue.FullKeyPath[(lastSep + 1)..];

                    using var parentKey = RegFileExporter.OpenKeyFromFullPath(parentPath, writable: true);
                    if (parentKey == null)
                    {
                        logger.LogDebug("Parent key not found or not writable: {Path}", parentPath);
                        failed++;
                        continue;
                    }
                    parentKey.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
                }
                succeeded++;
                logger.LogInformation("Cleaned: {Path}\\{Value}", issue.FullKeyPath, issue.ValueName ?? "(key)");
            }
            catch (UnauthorizedAccessException)
            {
                failed++;
                logger.LogWarning("Access denied: {Path}", issue.FullKeyPath);
            }
            catch (System.Security.SecurityException)
            {
                failed++;
                logger.LogWarning("Security exception: {Path}", issue.FullKeyPath);
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex, "Failed to clean: {Path}", issue.FullKeyPath);
            }
        }

        return Task.FromResult((succeeded, failed));
    }
}

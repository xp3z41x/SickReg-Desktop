using SickReg.Desktop.Models;

namespace SickReg.Desktop.Services;

public interface IRegistryCleanerService
{
    Task<(int succeeded, int failed)> CleanAsync(IEnumerable<RegistryIssue> issues);
}

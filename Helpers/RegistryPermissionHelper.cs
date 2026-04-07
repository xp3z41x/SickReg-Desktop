using Microsoft.Win32;

namespace SickReg.Desktop.Helpers;

public static class RegistryPermissionHelper
{
    public static bool CanWriteKey(string fullKeyPath)
    {
        try
        {
            using var key = RegFileExporter.OpenKeyFromFullPath(fullKeyPath, writable: true);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool CanWriteParentKey(string fullKeyPath)
    {
        var lastSep = fullKeyPath.LastIndexOf('\\');
        if (lastSep < 0) return false;

        var parentPath = fullKeyPath[..lastSep];
        return CanWriteKey(parentPath);
    }
}

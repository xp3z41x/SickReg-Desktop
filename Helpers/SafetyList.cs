namespace SickReg.Desktop.Helpers;

public static class SafetyList
{
    private static readonly HashSet<string> ProtectedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        @"HKEY_LOCAL_MACHINE\SYSTEM",
        @"HKEY_LOCAL_MACHINE\SAM",
        @"HKEY_LOCAL_MACHINE\SECURITY",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing",
        @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet",
    };

    private static readonly HashSet<string> ProtectedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".com", ".bat", ".cmd", ".msi", ".reg", ".ps1", ".sys", ".drv"
    };

    public static bool IsProtectedPath(string fullKeyPath)
    {
        foreach (var protectedPath in ProtectedPaths)
        {
            if (fullKeyPath.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public static bool IsProtectedExtension(string extension)
    {
        return ProtectedExtensions.Contains(extension);
    }
}

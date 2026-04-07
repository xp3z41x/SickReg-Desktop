namespace SickReg.Desktop.Helpers;

public static class RegistryPathValidator
{
    public static string? ExtractFilePath(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var value = rawValue.Trim();

        // Expand environment variables
        value = Environment.ExpandEnvironmentVariables(value);

        // Handle rundll32.exe: target is the second token
        if (value.StartsWith("rundll32", StringComparison.OrdinalIgnoreCase))
        {
            var parts = value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var dllPath = parts[1].Trim('"', ',');
                return NormalizePath(dllPath);
            }
            return null;
        }

        // Handle quoted paths: "C:\path\file.exe" /args
        if (value.StartsWith('"'))
        {
            var endQuote = value.IndexOf('"', 1);
            if (endQuote > 1)
            {
                return NormalizePath(value[1..endQuote]);
            }
        }

        // Handle unquoted paths with arguments: C:\path\file.exe /args
        // Try to find .exe, .dll, .com etc in the string
        string[] extensions = [".exe", ".dll", ".com", ".bat", ".cmd", ".sys"];
        foreach (var ext in extensions)
        {
            var idx = value.IndexOf(ext, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                var path = value[..(idx + ext.Length)].Trim('"');
                return NormalizePath(path);
            }
        }

        // If nothing matched, treat the whole value as a path
        return NormalizePath(value);
    }

    public static bool FileExistsAtPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public static bool DirectoryExistsAtPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizePath(string path)
    {
        path = path.Trim().Trim('"');
        return Environment.ExpandEnvironmentVariables(path);
    }
}

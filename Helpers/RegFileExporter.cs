using System.Text;
using Microsoft.Win32;

namespace SickReg.Desktop.Helpers;

public static class RegFileExporter
{
    public static string ExportKey(string fullKeyPath, string? valueName)
    {
        var sb = new StringBuilder();
        try
        {
            using var key = OpenKeyFromFullPath(fullKeyPath, writable: false);
            if (key == null) return string.Empty;

            sb.AppendLine($"[{fullKeyPath}]");

            if (valueName != null)
            {
                ExportValue(sb, key, valueName);
            }
            else
            {
                foreach (var name in key.GetValueNames())
                {
                    ExportValue(sb, key, name);
                }
            }
            sb.AppendLine();
        }
        catch
        {
            // Key may no longer exist or be inaccessible
        }
        return sb.ToString();
    }

    public static string GenerateRegFileContent(IEnumerable<(string KeyPath, string? ValueName)> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Windows Registry Editor Version 5.00");
        sb.AppendLine();

        foreach (var (keyPath, valueName) in entries)
        {
            sb.Append(ExportKey(keyPath, valueName));
        }

        return sb.ToString();
    }

    private static void ExportValue(StringBuilder sb, RegistryKey key, string valueName)
    {
        try
        {
            var value = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            var kind = key.GetValueKind(valueName);

            var nameStr = string.IsNullOrEmpty(valueName) ? "@" : $"\"{EscapeRegString(valueName)}\"";

            switch (kind)
            {
                case RegistryValueKind.String:
                    sb.AppendLine($"{nameStr}=\"{EscapeRegString(value?.ToString() ?? "")}\"");
                    break;
                case RegistryValueKind.DWord:
                    sb.AppendLine($"{nameStr}=dword:{(int)(value ?? 0):x8}");
                    break;
                case RegistryValueKind.QWord:
                    sb.AppendLine($"{nameStr}=hex(b):{FormatQWord((long)(value ?? 0L))}");
                    break;
                case RegistryValueKind.Binary:
                    var bytes = (byte[])(value ?? Array.Empty<byte>());
                    sb.AppendLine($"{nameStr}=hex:{FormatBytes(bytes)}");
                    break;
                case RegistryValueKind.MultiString:
                    var strings = (string[])(value ?? Array.Empty<string>());
                    var multiBytes = EncodeMultiString(strings);
                    sb.AppendLine($"{nameStr}=hex(7):{FormatBytes(multiBytes)}");
                    break;
                case RegistryValueKind.ExpandString:
                    var expBytes = Encoding.Unicode.GetBytes((value?.ToString() ?? "") + "\0");
                    sb.AppendLine($"{nameStr}=hex(2):{FormatBytes(expBytes)}");
                    break;
                default:
                    sb.AppendLine($"{nameStr}=\"{EscapeRegString(value?.ToString() ?? "")}\"");
                    break;
            }
        }
        catch
        {
            // Skip values that can't be read
        }
    }

    private static string EscapeRegString(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string FormatBytes(byte[] bytes) =>
        string.Join(",", bytes.Select(b => b.ToString("x2")));

    private static string FormatQWord(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        return FormatBytes(bytes);
    }

    private static byte[] EncodeMultiString(string[] strings)
    {
        var sb = new StringBuilder();
        foreach (var s in strings)
        {
            sb.Append(s);
            sb.Append('\0');
        }
        sb.Append('\0');
        return Encoding.Unicode.GetBytes(sb.ToString());
    }

    public static RegistryKey? OpenKeyFromFullPath(string fullPath, bool writable)
    {
        var separatorIndex = fullPath.IndexOf('\\');
        if (separatorIndex < 0) return null;

        var hiveName = fullPath[..separatorIndex];
        var subPath = fullPath[(separatorIndex + 1)..];

        var hive = hiveName.ToUpperInvariant() switch
        {
            "HKEY_LOCAL_MACHINE" or "HKLM" => Registry.LocalMachine,
            "HKEY_CURRENT_USER" or "HKCU" => Registry.CurrentUser,
            "HKEY_CLASSES_ROOT" or "HKCR" => Registry.ClassesRoot,
            "HKEY_USERS" or "HKU" => Registry.Users,
            "HKEY_CURRENT_CONFIG" or "HKCC" => Registry.CurrentConfig,
            _ => null
        };

        return hive?.OpenSubKey(subPath, writable);
    }
}

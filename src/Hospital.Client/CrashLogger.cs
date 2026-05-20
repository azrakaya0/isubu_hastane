using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Storage;

namespace Hospital.Client;

internal static class CrashLogger
{
    private const string LogFileName = "client-crash-log.txt";

    private static string LogFilePath
    {
        get
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, LogFileName);
                EnsureDirectoryExists(path);
                return path;
            }
            catch
            {
                var fallbackDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hospital.Client");
                var fallbackPath = Path.Combine(fallbackDirectory, LogFileName);
                EnsureDirectoryExists(fallbackPath);
                return fallbackPath;
            }
        }
    }

    internal static void Log(Exception exception, string source)
    {
        try
        {
            var text = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}] {source}: {exception}\n";
            File.AppendAllText(LogFilePath, text);
            Debug.WriteLine($"CrashLogger ({LogFilePath}): {text}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CrashLogger failed to write log file: {ex}");
        }
    }

    internal static void Log(string message, string source)
    {
        try
        {
            var text = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}] {source}: {message}\n";
            File.AppendAllText(LogFilePath, text);
            Debug.WriteLine($"CrashLogger ({LogFilePath}): {text}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CrashLogger failed to write log file: {ex}");
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

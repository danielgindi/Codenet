using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Codenet.IO;

public static class FolderHelper
{
    /// <summary>
    /// Remove invalid or unsafe characters from folder name
    /// </summary>
    /// <param name="folderName">Raw folder name</param>
    /// <returns>Cleaned up folder name</returns>
    internal static string SafeName(string folderName)
    {
        // Remove \ / : * ? " < > |
        return Regex.Replace(folderName, @"[\\/:*?""<>|\p{C}]", "_", RegexOptions.None);
    }

    public static bool VerifyDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    public static string GetTempDir()
    {
        string path = null;
        try
        {
            path = Path.GetTempPath();
        }
        catch
        {
            try
            {
                // Fallback 1
                path = Environment.GetEnvironmentVariable("TEMP");
                if (path == null || path.Length == 0)
                {
                    // Fallback 2
                    path = Environment.GetEnvironmentVariable("TMP");
                }
                if (path == null || path.Length == 0)
                {
                    // Fallback 3
                    path = Environment.GetEnvironmentVariable("WINDIR");
                    if (path != null && path.Length > 0)
                    {
                        path = Path.Combine(path, "TEMP");
                    }
                }
            }
            catch
            {
            }
        }

        if (!path.EndsWith(@"/") && !path.EndsWith(@"\"))
        {
            if (path.IndexOf('/') > -1) path += '/';
            else path += '\\';
        }

        if (VerifyDirectoryExists(path)) return path;

        throw new UnauthorizedAccessException("Cannot access TEMP folder!");
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace Codenet.IO;

public static class FileHelper
{
    public static readonly string[] DANGEROUS_EXTENSIONS = new string[]
    { 
        // Possibly malicious files, they can execute server side.
        ".aspx", ".asp", ".ascx", // ASP/ASP.NET
        ".php", ".php3", ".php4", ".phtml", // PHP
        ".jsp", // Java
        ".py", ".pyc", ".pyd", ".pyw", // Python
        ".pl", ".cgi", // Perl

        // Possibly malicious files, they can be viruses.
        ".exe", ".vbs", ".js", ".com", ".bat"
    };

    /// <summary>
    /// Remove invalid or unsafe characters from file name
    /// </summary>
    /// <param name="fileName">Raw file name</param>
    /// <returns>Cleaned up file name</returns>
    internal static string SafeName(string fileName)
    {
        // Replace dots with underscores, except the extension
        fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

        // Remove \ / : * ? " < > |
        return Regex.Replace(fileName, @"[\\/:*?""<>|\p{C}]", "_", RegexOptions.None);
    }

    public static string AquireUploadFileName(
        string fileName,
        string folder, string subFolder,
        bool appendDateTime,
        bool stripUnicodeFileNames,
        bool renameDangerousExtensions
        )
    {
        fileName = SafeName(Path.GetFileName(fileName));
        if (renameDangerousExtensions)
        {
            foreach (string ext in DANGEROUS_EXTENSIONS)
            {
                if (fileName.EndsWith(ext.StartsWith(".") ? ext : ('.' + ext), StringComparison.OrdinalIgnoreCase))
                {
                    fileName += @".unsafe";
                    break;
                }
            }
        }

        if (subFolder != null)
        {
            if (subFolder.Length == 0) subFolder = null;
            else subFolder = subFolder.Trim('/', '\\');
        }
        folder = folder.Trim('/', '\\');

        if (folder.Contains(@"/"))
        {
            folder += @"/";
            if (subFolder != null) subFolder += @"/";
        }
        else
        {
            folder += @"\";
            if (subFolder != null) subFolder += @"\";
        }

        if (subFolder != null) folder += subFolder;

        if (stripUnicodeFileNames)
        {
            char[] chars = fileName.ToCharArray();
            fileName = string.Empty;
            foreach (char c in chars)
            {
                if (c <= 127 && c != 37/*%*/ && c != 38/*&*/) fileName += ((c == 32/* */ || c == 39/*'*/) ? ((char)95/*_*/) : c);
            }
        }
        else fileName = fileName.Replace("'", "_").Replace("%", "").Replace("&", "");
        fileName = fileName.Trim();

        if (fileName.Length == 0 || fileName.StartsWith(".")) appendDateTime = true;

        if (appendDateTime)
        {
            fileName = DateTime.UtcNow.ToString("yyyy_MM_dd_hh_mm_ss", DateTimeFormatInfo.InvariantInfo) + ((fileName.Length > 0 && !fileName.StartsWith(".")) ? "_" + fileName : fileName);
        }

        int iTries = 0;
        string strFile = Path.GetFileNameWithoutExtension(fileName);
        string strFileExt = Path.GetExtension(fileName);
        fileName = Path.GetFileName(fileName);
        string strFilePath = folder + fileName;
        while (File.Exists(strFilePath))
        {
            if (!strFile.EndsWith("_")) strFile += "_";
            fileName = strFile + iTries.ToString() + strFileExt;
            strFilePath = folder + fileName;
            iTries++;
        }
        return strFilePath;
    }

    /// <summary>
    /// Creates an empty file in the TEMP folder.
    /// Note that you might want to reset the file's permissions after moving, because it has the permissions of the TEMP folder.
    /// </summary>
    /// <returns>Path to the temp file that was created</returns>
    public static string CreateEmptyTempFile()
    {
        string tempFilePath = FolderHelper.GetTempDir() + Guid.NewGuid().ToString() + ".tmp";
        FileStream fs = null;
        while (true)
        {
            try
            {
                fs = new FileStream(tempFilePath, FileMode.CreateNew);
                break;
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"Codenet.IO.FileHelper.CreateEmptyTempFile - Error: {ioex}");
                if (File.Exists(tempFilePath))
                { // File exists, make up another name
                    tempFilePath = FolderHelper.GetTempDir() + Guid.NewGuid().ToString() + ".tmp";
                }
                else
                { // Another error, throw it back up
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codenet.IO.FileHelper.CreateEmptyTempFile - Error: {ex}");
                break;
            }
        }
        if (fs != null)
        {
            fs.Dispose();
            return tempFilePath;
        }
        return null;
    }

    /// <summary>
    /// Reset the file's permissions to it's parent folder's permissions
    /// </summary>
    /// <param name="filePath">Path to the target file</param>
    public static void ResetFilePermissionsToInherited(string filePath)
    {
        FileSecurity fileSecurity = new FileSecurity(filePath, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        fileSecurity.SetAccessRuleProtection(false, true);
        foreach (FileSystemAccessRule rule in fileSecurity.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount)))
        {
            fileSecurity.RemoveAccessRule(rule);
        }
        new FileInfo(filePath).SetAccessControl(fileSecurity);
    }
}

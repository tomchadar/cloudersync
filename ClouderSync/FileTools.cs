using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClouderSync
{
    public static class FileTools
    {
        private static readonly char[] DirectorySeparators = new[] {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        public static string NormalizeDirName(string remoteDir, bool bPrepend = true)
        {
            remoteDir = remoteDir.Replace("\\", "/");
            if (remoteDir == string.Empty)
            {
                remoteDir = "/";
                return remoteDir;
            }
            if (bPrepend)
            {
                if (!remoteDir.StartsWith("/"))
                {
                    remoteDir = ("/" + remoteDir);
                }
            }
            if (!remoteDir.EndsWith("/"))
            {
                remoteDir += "/";
            }
            return remoteDir;
        }
        public static string CombinePaths(string remoteRootDir, string remoteRelativeDir)
        {
            if (remoteRootDir == string.Empty)
            {
                remoteRootDir = "/";
            }

            if (!remoteRootDir.EndsWith("/"))
            {
                remoteRootDir += "/";
            }
            if(remoteRelativeDir.StartsWith("/"))
            {
                remoteRelativeDir = remoteRelativeDir.TrimStart('/');
            }
            return (remoteRootDir + remoteRelativeDir);
        }
        public static bool CanMapRoots(string localPath, string projectDirectory)
        {

            string pathTrail = string.Empty;
            try
            {
                pathTrail = localPath.Substring(projectDirectory.Length);
            }
            catch(Exception eSubstring)
            {
                Debug.Write(eSubstring.Message);
                return false;
            }
            //pathTrail = pathTrail.TrimEnd(Path.DirectorySeparatorChar);
            if (!localPath.StartsWith(projectDirectory, true, null))
            {
                return false;
            }
            return true;
        }

        public static string MapRoots(string localPath, string projectDirectory)
        {
            int iMinLen = projectDirectory.Length;
            if (localPath.Length < iMinLen)
            {
                iMinLen = localPath.Length;
            }

            string pathTrail = localPath.Substring(iMinLen);
            pathTrail = pathTrail.TrimStart(Path.DirectorySeparatorChar);
            return pathTrail;

        }
        public static bool FileExists(string name)
        {
            if (File.Exists(name))
            {
                return true;
            }
            return (Directory.Exists(name));
        }
        /// <summary>
        /// Returns true if the path has a directory separator character at the end.
        /// </summary>
        public static bool HasEndSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[path.Length - 1]);
        }
        public static Uri MakeUri(string path, bool isDirectory, UriKind kind, string throwParameterName = "path")
        {
            try
            {
                if (isDirectory && !string.IsNullOrEmpty(path) && !HasEndSeparator(path))
                {
                    path += Path.DirectorySeparatorChar;
                }

                return new Uri(path, kind);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
        }
        /// <summary>
        /// Normalizes and returns the provided directory path, always
        /// ending with '/'.
        /// </summary>
        public static string NormalizeDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var uri = MakeUri(path, true, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                if (uri.IsFile)
                {
                    return uri.LocalPath;
                }
                else
                {
                    return uri.AbsoluteUri.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }
            }
            else
            {
                return Uri.UnescapeDataString(uri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

    }
}

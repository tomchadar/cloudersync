using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace ClouderSync
{
    public static class FileTools
    {
        public static readonly char[] DirectorySeparators = new[] {
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
            if (remoteRelativeDir.StartsWith("/"))
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
            catch (Exception eSubstring)
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
            try
            {
                if (File.Exists(name))
                {
                    return true;
                }
                if (Directory.Exists(name))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
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
        //since 2.1.2
        public static bool QuoteString(string fileName, ref string quotedString, byte quote=0x22)
        {
            byte[] byteArray = Encoding.Default.GetBytes("@" + fileName + "@");
            byteArray[0] = quote;
            int iLast = byteArray.Length - 1;
            if (iLast > 0)
            {
                byteArray[iLast] = quote;
            }
            string quotedName=byteArray.ToString();
            quotedName = @Encoding.Default.GetString(byteArray);
            if(quotedString!=null)
            {
                quotedString = quotedName;
            }
            return true;

        }
        public static bool DeleteFile(string archiveName, ref string errorMessage)
        {
            string fileName = archiveName;
            fileName.TrimStart('"');
            fileName.TrimEnd('"');
            string quotedName = "";
            quotedName = Path.GetPathRoot("K:\\codercrest\\New folder\\New Text Document.txt");
//            FileInfo fi = new FileInfo("K:\\codercrest\\New folder\\New Text Document.txt");
//            quotedName = fi.FullName;

            QuoteString(fileName, ref quotedName);
            FileTools.FileExists(quotedName);
            try
            {
                if (!File.Exists(fileName))
                {
                    return true;
                }
            }
            catch
            {

            }
            try
            {
                
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                ProcessStartInfo StartInfo = new ProcessStartInfo("cmd.exe");
                StartInfo.Arguments = String.Format("/C del /F /Q {0}", fileName);
                StartInfo.CreateNoWindow = true;
                //p.StartInfo.FileName = "cmd.exe";
                //p.StartInfo.Arguments = String.Format(" del /F /Q {0}", fileName);
                //p.StartInfo.WorkingDirectory = @Path.GetDirectoryName(archiveName);
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //StartInfo.UseShellExecute = false;
                /*
                StartInfo.RedirectStandardOutput = true;
                StartInfo.RedirectStandardInput = true;
                */
                p.StartInfo = StartInfo;
                p.Start();
                p.WaitForExit();
            }
            catch(Exception eProcess)
            {
                if(errorMessage!=null)
                {
                    errorMessage = eProcess.Message;
                }
                return false;
            }
            return true;
        }
        //since 1.9.7
        private static string ToCaseSensitive(string sDirOrFile)
        {
            string sTmp = String.Empty;
            string pathSep = Path.DirectorySeparatorChar.ToString();
            //   string rootPath = Path.GetPathRoot(sDirOrFile);
            string[] dirs = sDirOrFile.Split(Path.DirectorySeparatorChar);//FileTools.DirectorySeparators);
            string[] fsEntries = null;// new[] { string.Empty };
                                      // fsEntries = Directory.GetFileSystemEntries(rootPath);

            foreach (string sPth in dirs)
            {
                if (string.IsNullOrEmpty(sTmp))
                {
                    sTmp = sPth + Path.DirectorySeparatorChar;
                    continue;
                }
                //sTmp = Directory.GetFileSystemEntries(sTmp, sPth)[0];
                fsEntries = Directory.GetFileSystemEntries(sTmp, sPth);
                if (fsEntries != null && fsEntries.Length > 0)
                {
                    sTmp = fsEntries[0];
                }
            }

            if (sTmp.EndsWith(pathSep) && !sDirOrFile.EndsWith(pathSep))
            {
                return sTmp.TrimSuffix(pathSep);
            }

            return sTmp;
            /*
                        if (fsEntries.Length>0)
                        {
                            if (fsEntries[0].EndsWith(pathSep) && !sDirOrFile.EndsWith(pathSep))
                            {
                                return fsEntries[0].TrimSuffix(pathSep);
                            }
                        }
                        return string.Empty;
            */
            /*
            */
        }
        //since 1.9.7
        public static bool ConvertToCaseSensitive(string pathIn, ref string pathOut)
        {
            if (string.IsNullOrEmpty(pathIn))
            {
                return false;
            }
            //pathOut = pathIn;
            try
            {
                pathOut = ToCaseSensitive(pathIn);
            }
            catch (Exception eConvert)
            {
                pathOut = pathIn;
                Debug.Write(eConvert.Message);
                return false;
            }
            return true;
        }
        public static string NormalizeFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var uri = MakeUri(path, false, UriKind.RelativeOrAbsolute);
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

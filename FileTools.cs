using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSync
{
    public static class FileTools
    {
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
            
            string pathTrail = localPath.Substring(projectDirectory.Length);
            //pathTrail = pathTrail.TrimEnd(Path.DirectorySeparatorChar);
            if (!localPath.StartsWith(projectDirectory, true, null))
            {
                return false;
            }
            return true;
        }

        public static string MapRoots(string localPath, string projectDirectory)
        {
            string pathTrail = localPath.Substring(projectDirectory.Length);
            pathTrail = pathTrail.TrimStart(Path.DirectorySeparatorChar);
            return pathTrail;

        }

    }
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ClouderSync.Data;
using ClouderSync.SFTPClient;

namespace ClouderSync
{
    public class CompressTask: CoreTask
    {
        public static CancellationTokenSource _cancelTokenSrc = null;
        public static CancellationToken _cancelToken = new CancellationToken();
        protected static long iRunning = 0;

        private static readonly char[] dirSeparators = new[] {
            '/','\\'
        };
        public CompressTask(CommandCore commandCore, CancellationTokenSource cancelTokenSrc = null)
        : base(commandCore, cancelTokenSrc)
        {
            if(!FileTools.FileExists(selectedFullFilePath))
            {
                selectedFullFilePath= commandCore.GetTopmostRoot();
            }
            arrItems = _commandCore.GetSelectedItems(true);
        }

        protected override bool ExecuteInternal()
        {
            PreProcessTask();
            bool bRet = ProcessTask();
            PostProcessTask();
            return bRet;
        }
        protected override bool ProcessTask()
        {
            

            ci.ReadEntry();

            
            ZipArchive archive = null;
            string archiveFile = string.Empty;
            if (!InitArchive(ref archive,ref ci, ref archiveFile))
            {
                return false;
            }
          
            Array arrInfo = arrItems;
            ProcessCompression(archive,arrInfo, ci);
            ClouderSyncPackage.WriteToOutputWindow(string.Format("Compression complete: file {0}\n", archiveFile));
            try
            {
                archive.Dispose();
            }
            catch { }
            archive = null;
            return true;
        }
        protected bool NeedsNewArchiveEntry(ZipArchiveEntry archiveEntry, string remoteRelativeDir)
        {
            if (archiveEntry == null)
            {
                return true;
            }
            if (!remoteRelativeDir.Equals(archiveEntry.FullName))
            {
                return true;
            }
            return false;
        }
        protected bool ProcessCompression(ZipArchive archive, Array arrInfo, ConnectEntryData ci)
        {
            if ((arrInfo == null) || (arrInfo.Length < 1))
            {
                ClouderSyncPackage.WriteToOutputWindow("Nothing selected, nothing to do \n");
                return false;
            }

            int iItem = 0;
            int iItemCount = arrInfo.Length;
            if ((_cancelToken != null) && _cancelToken.IsCancellationRequested)
            {
                _cancelToken.ThrowIfCancellationRequested();
            }

            foreach (ItemInfo item in arrInfo)
            {
                iItem++;
                if (!item.HasPath)
                {
                    continue;
                }
                if (ci.ce.ignoreexcludedfiles)
                {
                    if (!item.IncludedInProject)
                    {
                        ClouderSyncPackage.WriteToOutputWindow("Skipping item excluded from project " + item.FilePath + "...\n");
                        continue;
                    }
                }
                string pathTrail = FileTools.MapRoots(item.FilePath, projectDirectory);
                string szFileName = Path.Combine(projectDirectory, pathTrail);
                if (!File.Exists(szFileName))
                {
                    ClouderSyncPackage.WriteToOutputWindow("Entering local directory " + szFileName + "\n");
                    continue;
                }
                string fileName = Path.GetFileName(item.FilePath);
                string localDir = Path.GetDirectoryName(pathTrail);
                string remoteRelativeDir = FileTools.NormalizeDirName(localDir);
                string archiveRoot = remoteRelativeDir;
                ZipArchiveEntry archiveEntry = null;

                remoteRelativeDir = remoteRelativeDir.TrimStart(FileTools.DirectorySeparators);
                if(NeedsNewArchiveEntry(archiveEntry,remoteRelativeDir))
                { 
                    ClouderSyncPackage.WriteToOutputWindow("Entering local directory " + Path.GetDirectoryName(item.FilePath) + "\n");
                    archiveRoot = remoteRelativeDir;
                    if (string.IsNullOrEmpty(archiveRoot))
                    {
                        archiveRoot = "/";
                    }
                    if (!string.IsNullOrEmpty(archiveRoot))
                    {
                        try
                        {
                            archiveEntry = archive.CreateEntry(archiveRoot);
                        }
                        catch (Exception eFile)
                        {
                            ClouderSyncPackage.WriteToOutputWindow(string.Format(CultureInfo.CurrentCulture, "Failed to create archive entry {0}: {1}\n", remoteRelativeDir, eFile.Message));
                        }

                    }
                    if (fileName == string.Empty)
                    {
                        continue;
                    }
                }
                string message = string.Format(CultureInfo.CurrentCulture, "{0}/{1} {2}=>{3}", iItem, iItemCount, pathTrail, remoteRelativeDir + fileName);
                ClouderSyncPackage.WriteToOutputWindow(message + "\n");
#if !TEST_MODE
                try
                {
                    string remoteFile = Path.Combine(remoteRelativeDir, fileName);
                    if (archiveEntry != null)
                    {
//                        remoteFile = archiveEntry.FullName + "/" + fileName;
                    }
                    ZipArchiveEntry fileZipEntry=archive.CreateEntryFromFile(szFileName, remoteFile, CompressionLevel.Optimal);
                }
                catch(Exception eAddFile)
                {
                    ClouderSyncPackage.WriteToOutputWindow(string.Format(CultureInfo.CurrentCulture, "Skipping file {0}: {1}", szFileName,eAddFile.Message));
                }
#endif
                if ((_cancelToken != null) && _cancelToken.IsCancellationRequested)
                {
                    _cancelToken.ThrowIfCancellationRequested();
                }
            }
            return true;
        }
    }
}

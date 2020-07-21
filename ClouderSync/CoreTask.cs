using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ClouderSync.Data;
using ClouderSync.SFTPClient;

namespace ClouderSync
{
    public partial class CoreTask : TaskControl
    {

        /*
        public new static CancellationTokenSource _cancelTokenSrc = null;
        public new static CancellationToken _cancelToken = new CancellationToken();
        protected new static long iRunning = 0;
        */
        protected string projectDirectory = string.Empty;
        protected string selectedDir = string.Empty;
        protected string selectedFullFilePath = string.Empty;
        protected Array arrItems = null;
        protected ConnectEntryData ci = new ConnectEntryData();
        protected CommandCore _commandCore = null;

        public CoreTask(CommandCore commandCore, CancellationTokenSource cancelTokenSrc = null)
        {
            _commandCore = commandCore;
            _cancelTokenSrc = cancelTokenSrc;
            projectDirectory = commandCore.GetTopmostRoot();
            selectedDir = commandCore.GetSelectedItemDirectory(ref selectedFullFilePath);
            FileTools.ConvertToCaseSensitive(selectedDir, ref selectedDir);

        }
        public static void SetIsRunning(bool bRunning)
        {
            Interlocked.Exchange(ref iRunning, (bRunning ? 1 : 0));
        }
        public static bool GetIsRunning()
        {
            long iRes = Interlocked.Read(ref iRunning);
            if (iRes == 1)
            {
                return true;
            }
            return false;
        }
        public static void Cancel()
        {
            SetIsRunning(false);
            if (_cancelTokenSrc != null)
            {
                _cancelTokenSrc.Cancel(true);
            }
        }
        public virtual void Execute(CancellationTokenSource cancellationTokenSource = null)
        {
            _cancelTokenSrc = cancellationTokenSource;
            try
            {
                ExecuteInternal();
            }
            catch (OperationCanceledException eCxl)
            {
                SetIsRunning(false);
                ClouderSyncPackage.WriteToOutputWindow("Transfer cancelled, " + eCxl.Message + "\n");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

        }
        protected virtual bool ExecuteInternal()
        {
            PreProcessTask();
            bool bRet=ProcessTask();
            PostProcessTask();
            return bRet;
        }
        protected virtual bool PreProcessTask()
        {
            SetIsRunning(true);
            if (_cancelTokenSrc != null)
            {
                _cancelToken = _cancelTokenSrc.Token;
            }
            if (_cancelToken.IsCancellationRequested)
            {
                _cancelToken.ThrowIfCancellationRequested();
            }
            return true;

        }
        protected virtual bool PostProcessTask()
        {
            SetIsRunning(false);
            return true;
        }

        protected virtual bool ProcessTask()
        {
            return false;
        }
        /*
        protected virtual void ExecuteInternal()
        {
            SetIsRunning(true);
            if (_cancelTokenSrc != null)
            {
                _cancelToken = _cancelTokenSrc.Token;
            }
            if (_cancelToken.IsCancellationRequested)
            {
                _cancelToken.ThrowIfCancellationRequested();
            }
            SFTPSyncClient client = null;

            ci.ReadEntry();

            Array arrInfo = arrItems;

            ProcessTask(arrInfo, ref client, ref ci);
            ClouderSyncPackage.WriteToOutputWindow("Transfer complete" + "\n");
            SetIsRunning(false);
        }
        */
        protected virtual bool InitArchive(ref ZipArchive archive, ref ConnectEntryData ci, ref string _archiveFileName)
        {
            //string archiveName = Path.ChangeExtension(selectedDir, "zip");
            string archiveName = selectedFullFilePath + ".zip";
            string errorMessage = string.Empty;
            if(!FileTools.DeleteFile(archiveName,ref errorMessage))
            {
                try
                {
                    if (File.Exists(archiveName))
                    {
                        File.Delete(archiveName);
                    }
                }
                catch(Exception eDelete)
                {
                    ClouderSyncPackage.WriteToOutputWindow(String.Format("Failed to delete {0}:{1}\n", archiveName, eDelete.Message));
                    return false;
                }
            }
            try
            {
                archive = ZipFile.Open(archiveName, ZipArchiveMode.Create);
            }
            catch (Exception eCreate)
            {
                ClouderSyncPackage.WriteToOutputWindow(String.Format("Failed to create {0}:{1}\n", archiveName, eCreate.Message));
                return false;
            }
            if(_archiveFileName!=null)
            {
                _archiveFileName = archiveName;
            }
            //archive.CreateEntryFromFile()
            return true;
        }


        protected virtual bool InitClient(ConnectEntryData ci, ref SFTPSyncClient client)
        {
            try
            {
                client = SFTPSyncClient.Create(ci.ce);
            }
            catch (Exception eClient)
            {
                ClouderSyncPackage.Log(eClient.Message);
                return false;
            }
            try
            {
                if (!client.Connect())
                {
                    client.Log("Connection has failed" + '\n');
                    return false;
                }
            }
            catch (Exception eConnect)
            {
                ClouderSyncPackage.WriteToOutputWindow(eConnect.Message + '\n');
                string testResult = client.getLogMessages();
                ClouderSyncPackage.WriteToOutputWindow(testResult);
                client.clearLogMessages();
                return false;
            }
            return true;
        }
#if false
        protected virtual bool ProcessTask(Array arrInfo, ref SFTPSyncClient client, ref ConnectEntryData ci)
        {
            return false;
        }

        protected virtual bool ProcessTransfer(Array arrInfo, ref SFTPSyncClient client, ref ConnectEntryData ci)
        {

            if ((arrInfo == null) || (arrInfo.Length < 1))
            {
                ClouderSyncPackage.WriteToOutputWindow("Nothing selected, nothing to do \n");
                return false;
            }
            if (!InitClient(ci, ref client))
            {
                return false;
            }
            string remoteSrcPath = FileTools.NormalizeDirName(ci.ce.remotesrcpath);
            int iItem = 0;
            int iItemCount = arrInfo.Length;
            if ((_cancelToken != null) && _cancelToken.IsCancellationRequested)
            {
                _cancelToken.ThrowIfCancellationRequested();
            }

#if !TEST_MODE
            if (!client.EnterDirectory(remoteSrcPath))
            {
                ClouderSyncPackage.WriteToOutputWindow("Failed to enter remote directory " + remoteSrcPath + "\n");
            }
            else
#endif
            {
                ClouderSyncPackage.WriteToOutputWindow("Entering remote directory " + remoteSrcPath + "...\n");
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
                if (!FileTools.CanMapRoots(item.FilePath, projectDirectory))
                {
                    int iMinLen = projectDirectory.Length;
                    if (item.FilePath.Length < iMinLen)
                    {
                        iMinLen = item.FilePath.Length;
                    }
                    ClouderSyncPackage.WriteToOutputWindow("Can not map " + item.FilePath.Substring(0, iMinLen) + " to " + projectDirectory + "\n");
                    continue;
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

                if (fileName == string.Empty)
                {
                    ClouderSyncPackage.WriteToOutputWindow("Entering local directory " + item.FilePath + "\n");
                    client.EnterDirectory(FileTools.CombinePaths(remoteSrcPath, remoteRelativeDir));
                    continue;
                }


                string message = string.Format(CultureInfo.CurrentCulture, "{0}/{1} {2}=>{3}", iItem, iItemCount, pathTrail, remoteRelativeDir + fileName);
                ClouderSyncPackage.WriteToOutputWindow(message + "\n");
#if TEST_MODE
                FileStream fs = null;
                try
                {
                    fs = new FileStream(szFileName, FileMode.Open, FileAccess.Read);
                }
                catch (Exception eFile)
                {
                    ClouderSyncPackage.WriteToOutputWindow(string.Format(CultureInfo.CurrentCulture, "Failed to open local file {0}: {1}\n",szFileName,eFile.Message ));
                }
                if (fs != null)
                {
                    client.BufferSize = 1024;
                    client.UploadAndCreateFile(fs, remoteSrcPath, remoteRelativeDir, fileName);
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
#endif
                if ((_cancelToken != null) && _cancelToken.IsCancellationRequested)
                {
                    _cancelToken.ThrowIfCancellationRequested();
                }
            }
            if (client != null)
            {
                client.Disconnect();
                client.Dispose();
            }

            return true;
        }
#endif
    }

}

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClouderSync.Data;
using ClouderSync.SFTPClient;

namespace ClouderSync
{
    public class DeploymentTask
    {
        string projectDirectory = string.Empty;
        Array arrItems = null;
        ConnectEntryData ci = new ConnectEntryData();
        public static CancellationTokenSource _cancelTokenSrc=null;
        public static CancellationToken _cancelToken = new CancellationToken();
        private static long iRunning = 0;

        public DeploymentTask(CommandCore commandCore, CancellationTokenSource cancelTokenSrc =null)
        {
            _cancelTokenSrc = cancelTokenSrc;
            projectDirectory = commandCore.GetTopmostRoot();
            arrItems = commandCore.GetSelectedItems(true);
        }
        public static void SetIsRunning(bool bRunning)
        {
            Interlocked.Exchange(ref iRunning, (bRunning ? 1 : 0));
        }
        public static bool GetIsRunning()
        {
            long iRes= Interlocked.Read(ref iRunning);
            if(iRes==1)
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
        public void Execute(CancellationTokenSource cancellationTokenSource = null)
        {
            _cancelTokenSrc = cancellationTokenSource;
            try
            {
                ExecuteInternal();
            }
            catch(OperationCanceledException eCxl)
            {
                SetIsRunning(false);
                ClouderSyncPackage.WriteToOutputWindow("Transfer cancelled, " + eCxl.Message  + "\n");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

        }
        protected void ExecuteInternal()
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

            ProcessTransfer(arrInfo, ref client, ref ci);
            ClouderSyncPackage.WriteToOutputWindow("Transfer complete" + "\n");
            SetIsRunning(false);
        }
        protected bool InitClient(ConnectEntryData ci, ref SFTPSyncClient client)
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
                if(!client.Connect())
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

        protected bool ProcessTransfer(Array arrInfo, ref SFTPSyncClient client, ref ConnectEntryData ci)
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
            if ((_cancelToken!=null) && _cancelToken.IsCancellationRequested)
            {
                _cancelToken.ThrowIfCancellationRequested();
            }

#if !TEST_XFER
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
                if ((_cancelToken != null) && _cancelToken.IsCancellationRequested)
                {
                    _cancelToken.ThrowIfCancellationRequested();
                    break;
                }

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
                    if(item.FilePath.Length<iMinLen)
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

                if (fileName==string.Empty)
                {
                    ClouderSyncPackage.WriteToOutputWindow("Entering local directory " + item.FilePath + "\n");
                    client.EnterDirectory(FileTools.CombinePaths(remoteSrcPath,remoteRelativeDir));
                    continue;
                }


                string message = string.Format(CultureInfo.CurrentCulture, "{0}/{1} {2}=>{3}", iItem,iItemCount, pathTrail,remoteRelativeDir + fileName);
                ClouderSyncPackage.WriteToOutputWindow(message + "\n");
#if !TEST_XFER
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
                    client.UploadAndCreateFile(fs, remoteSrcPath, remoteRelativeDir, fileName,_cancelToken);
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

    }
}

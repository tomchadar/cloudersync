using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudSync.Data;
using CloudSync.SFTPClient;

namespace CloudSync
{
    public class DeploymentTask
    {
        string projectDirectory = string.Empty;
        Array arrItems = null;

        public DeploymentTask(CommandCore commandCore)
        {
            projectDirectory = commandCore.GetProjectRoot();
            arrItems = commandCore.GetSelectedItems(true);
        }
        public void Execute()
        {
            ConnectEntryData ci = new ConnectEntryData();
            SFTPSyncClient client = null;

            ci.ReadEntry();

            Array arrInfo = arrItems;

            ProcessTransfer(arrInfo, ref client, ref ci);
            CloudSyncPackage.WriteToOutputWindow("Transfer complete" + "\n");
        }
        protected bool InitClient(ConnectEntryData ci, ref SFTPSyncClient client)
        {
            try
            {
                client = SFTPSyncClient.Create(ci.ce);
            }
            catch (Exception eClient)
            {
                CloudSyncPackage.Log(eClient.Message);
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
                CloudSyncPackage.WriteToOutputWindow(eConnect.Message + '\n');
                string testResult = client.getLogMessages();
                CloudSyncPackage.WriteToOutputWindow(testResult);
                client.clearLogMessages();
                return false;
            }
            return true;
        }

        protected bool ProcessTransfer(Array arrInfo, ref SFTPSyncClient client, ref ConnectEntryData ci)
        {

            if ((arrInfo == null) || (arrInfo.Length < 1))
            {
                CloudSyncPackage.WriteToOutputWindow("Nothing selected, nothing to do \n");
                return false;
            }
            if (!InitClient(ci, ref client))
            {
                return false;
            }
           // string projectDirectory = GetProjectRoot();
            string remoteSrcPath = FileTools.NormalizeDirName(ci.ce.remotesrcpath);
            int iItem = 0;
            int iItemCount = arrInfo.Length;
            if (!client.EnterDirectory(remoteSrcPath))
            {
                CloudSyncPackage.WriteToOutputWindow("Failed to enter remote directory " + remoteSrcPath + "\n");
            }

            foreach (ItemInfo item in arrInfo)
            {
                iItem++;
                if (!item.HasPath)
                {
                    continue;
                }
                if (!FileTools.CanMapRoots(item.FilePath, projectDirectory))
                {
                    CloudSyncPackage.WriteToOutputWindow("Can not map " + item.FilePath.Substring(0, projectDirectory.Length) + " to " + projectDirectory + "\n");
                    continue;
                }
                string pathTrail = FileTools.MapRoots(item.FilePath, projectDirectory);
                string szFileName = Path.Combine(projectDirectory, pathTrail);
                if (!File.Exists(szFileName))
                {
                    CloudSyncPackage.WriteToOutputWindow("Entering local directory " + szFileName + "\n");
                    continue;
                }
                string fileName = Path.GetFileName(item.FilePath);
                string localDir = Path.GetDirectoryName(pathTrail);
                string remoteRelativeDir = FileTools.NormalizeDirName(localDir);

                if (fileName==string.Empty)
                {
                    CloudSyncPackage.WriteToOutputWindow("Entering local directory " + item.FilePath + "\n");
                    client.EnterDirectory(FileTools.CombinePaths(remoteSrcPath,remoteRelativeDir));
                    continue;
                }


                string message = string.Format(CultureInfo.CurrentCulture, "{0}/{1} {2}=>{3}", iItem,iItemCount, pathTrail,remoteRelativeDir + fileName);
                CloudSyncPackage.WriteToOutputWindow(message + "\n");
                FileStream fs = null;
                try
                {
                    fs = new FileStream(szFileName, FileMode.Open, FileAccess.Read);
                }
                catch (Exception eFile)
                {
                    CloudSyncPackage.WriteToOutputWindow(string.Format(CultureInfo.CurrentCulture, "Failed to open local file {0}: {1}\n",szFileName,eFile.Message ));
                }
                if (fs != null)
                {
                    client.BufferSize = 1024;
                    client.UploadAndCreateFile(fs, remoteSrcPath, remoteRelativeDir, fileName);
                    fs.Close();
                    fs.Dispose();
                    fs = null;
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

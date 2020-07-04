using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using ClouderSync.Data;
using Renci.SshNet.Common;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace ClouderSync.SFTPClient
{
    public class SFTPSyncClient : SftpClient
    {
        public ConnectionInfo connectionInfo = null;
        public StringBuilder resultMessages;
        public StringBuilder progressMessages;
        IVsOutputWindowPane pane=null;

        public SFTPSyncClient(ConnectionInfo connInfo)
            :base(connInfo)
        {
            KeepAliveInterval = new TimeSpan(-1);
            connectionInfo = connInfo;
            resultMessages = new StringBuilder();
            progressMessages = new StringBuilder();
            InitOutputPane();
            
        }
        protected void InitOutputPane()
        {
            pane = ClouderSyncPackage.GetWindowPane(Guid.Empty, "ClouderSync", true, true);
        }
        public static SFTPSyncClient Create(CONNECTENTRY ci)
        {
            SFTPSyncClient objClient = null;
            ConnectionInfo connectionInfo = InitConnection(ci);
            if(connectionInfo==null)
            {
                return null;
            }
            try
            {
                connectionInfo.Timeout = TimeSpan.FromSeconds(2);
                objClient = new SFTPSyncClient(connectionInfo);
            }
            catch(Exception eCreate)
            {
                Debug.Write(eCreate.Message);
                return null;
            }
            return objClient;
        }
        public static ConnectionInfo InitConnection(CONNECTENTRY ci)
        {
            ConnectionInfo connectionInfo = null;
            if (!ci.usekeyfile)
            {
                connectionInfo = getSftpConnection(ci.hostname, ci.port, ci.username, ci.plainpass);
            }
            else
            {
                connectionInfo = getSftpConnection(ci.hostname, ci.port, ci.username, privateKeyObject(ci.username, ci.keyfile));
            }
            return connectionInfo;
        }
        public static ConnectionInfo getSftpConnection(string host, int port, string username, string password)
        {
            return new ConnectionInfo(host, port, username, new AuthenticationMethod[]
            {
                 new PasswordAuthenticationMethod(username, password),
            });

        }
        public static ConnectionInfo getSftpConnection(string host, int port, string username, AuthenticationMethod[] privateKeyObject)
        {
            return new ConnectionInfo(host, port, username, privateKeyObject);
        }

        private static AuthenticationMethod[] privateKeyObject(string username, string publicKeyPath)
        {
            PrivateKeyFile privateKeyFile = new PrivateKeyFile(publicKeyPath);
            PrivateKeyAuthenticationMethod privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod(username, privateKeyFile);
            return new AuthenticationMethod[] { privateKeyAuthenticationMethod };
        }

        private static AuthenticationMethod[] passwordObject(string username, string password)
        {
            //  PrivateKeyFile privateKeyFile = new PrivateKeyFile(publicKeyPath);
            PasswordAuthenticationMethod passwordAuthenticationMethod = new PasswordAuthenticationMethod(username, password);
            return new AuthenticationMethod[] { passwordAuthenticationMethod };
        }
        public static string GetRemoteDirectoryName(string remotePath)
        {
            string dir = Path.GetDirectoryName(remotePath);

            return dir.Replace("\\", "/");
        }
        public new bool Connect()
        {
            //ConnectionInfo.Timeout = TimeSpan.FromSeconds(2);
            try
            {
                base.Connect();
            }
            catch(System.Net.Sockets.SocketException eConnect)
            {
                Log(eConnect.Message+'\n');
                return false;
            }
            return true;
        }
        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool Disconnect()
        {

            try
            {
                base.Disconnect();
            }
            catch(Exception eDisconnect)
            {
                Log(eDisconnect.Message + '\n');
                return false;
            }
            return true;
        }

        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public new bool UploadFile(Stream input, string path, bool canOverride=true, Action<ulong,ulong> uploadCallback = null)
        {
            try
            {
                base.UploadFile(input,path, canOverride, uploadCallback);
            }
            catch (Exception eUpload)
            {
                Log(path+":"+eUpload.Message+"\n");
                return false;
            }
            return true;
        }
        protected bool RecursiveCreateDirectory(string remoteAbsoulteDir, string remoteRelativeDir="")
        {
            string absolutePath = "";// remoteAbsoulteDir;
            string[] arrDirs = remoteAbsoulteDir.Split('/');
            foreach (string dir in arrDirs)
            {
                if ((dir == string.Empty))// || dir.Equals('/'))
                {
//                    absolutePath += ("/");
                    continue;
                }
                //absolutePath += (dir + "/");
                absolutePath += ("/"+dir);
                bool bExists = false;
                if (!Exists(absolutePath,ref bExists))
                {
                    return false;
                }
                if(bExists)
                {
                    //    absolutePath += (dir + "/");
                    if(!ChangeDirectory(absolutePath))
                    {
                        return false;
                    }
                    //continue;
                }
                else
                {
                    if (!this.CreateDirectory(dir))
                    {
                        return false;
                    }
                    //absolutePath += (dir+ "/");
                    if(!ChangeDirectory(absolutePath))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public void OnUploadProgress(ulong ul, ulong ulSize)
        {
            
            if (ul > 0)
            {
                decimal bufSize = Math.Round((decimal)(ulSize / 1024));
                if ((ul % 1024) == 0)
                {
                    decimal kb=Math.Round((decimal)(ul / 1024));

                    //ulong kb = ul / 1024;
                    LogProgress(kb.ToString()+"kb / "+bufSize.ToString()+"kb" );// string.Format("{0}", ul));   
                }
            }
        }
        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public bool UploadAndCreateFile(Stream input, string remoteProjectDir, string remoteRelativeDir, string fileName, bool canOverride = true, Action<ulong> uploadCallback = null)
        {
            remoteRelativeDir = FileTools.NormalizeDirName(remoteRelativeDir);
            remoteProjectDir = FileTools.NormalizeDirName(remoteProjectDir);
            string absoluteDir = FileTools.CombinePaths(remoteProjectDir,remoteRelativeDir);

            //string dir = remoteRelativeDir;//GetRemoteDirectoryName(path);
            try
            {
                if (!EnterDirectory(absoluteDir))
                {
                    return false;
                }
                if (!ChangeDirectory(absoluteDir))
                {
                    return false;
                }
                if (!UploadFile(input, fileName, canOverride, OnUploadProgress))
                {
                    return false;
                }
            }
            catch(ObjectDisposedException eDisposed)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// EnterDirectorys remote directory and optionally creates it
        /// </summary>
        /// <param name="path">New directory path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change directory denied by remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool EnterDirectory(string path, bool bCreateIfNotExist=true, bool bForce=false)
        {
            string workingDirectory = base.WorkingDirectory;

            if(ChangeDirectory(path))
            {
                return true;
            }
            if(!bCreateIfNotExist)
            {
                return false;
            }
            if(!RecursiveCreateDirectory(path,""))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Changes remote directory to path.
        /// </summary>
        /// <param name="path">New directory path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change directory denied by remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public new bool ChangeDirectory(string path, bool bIgnoreNotFoundLog=true)
        {
            try
            {
                base.ChangeDirectory(path);
            }
            catch(SftpPathNotFoundException eNotFound)
            {
                if (!bIgnoreNotFoundLog)
                {
                    Log("ChangeDirectory:" + path + ":" + eNotFound.Message + "\n");
                }
                return false;
            }
            catch (Exception eChange)
            {
                Log("ChangeDirectory:"+ path + ":" + eChange.Message + "\n");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to create the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public new bool CreateDirectory(string path)
        {
            
            try
            {
//                string dir = GetRemoteDirectoryName(path);
                base.CreateDirectory(path);
            }
            catch (Exception eChange)
            {
                Log("CreateDirectory:" + path + ":" + eChange.Message + "\n");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks whether file or directory exists;
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// <c>true</c> if directory or file exists; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool Exists(string path, ref bool bExists)
        {
            bExists = false;
            try
            {
                bExists=base.Exists(path);
            }
            catch (Exception eChange)
            {
                Log("Exists:"+path + ":" + eChange.Message + "\n");
                return false;
            }
            return true;
        }

        public static void Log(string message,StringBuilder resultMessages)
        {
            //CloudSyncPackage.WriteToOutputWindow(message, "ClouderSync", null, true);
            resultMessages.Append(message);
        }
        public void LogProgress(string message)
        {
            //LogToStringBuilder(progressMessages, message);
            if (pane != null)
            {
                pane.Activate();
                pane.OutputStringThreadSafe(message+" "+ "\r");
            }
            //CloudSyncPackage.WriteToOutputWindow(message, "ClouderSync", null, false);
        }
        public static void LogProgressToStringBuilder(StringBuilder sb, string message)
        {
            sb.Append(message);
            sb.AppendLine(" \n");
        }

        public void Log(string message)
        {
            LogToStringBuilder(resultMessages, message);
            ClouderSyncPackage.WriteToOutputWindow(message, "ClouderSync", null, false);
        }
        public static void LogToStringBuilder(StringBuilder sb,string message)
        {
            sb.Append(message);
            sb.AppendLine(" \n");
        }
        public string getLogMessages()
        {
            return resultMessages.ToString();
        }
        public void clearLogMessages()
        {
            resultMessages.Clear();
        }


        protected override void OnConnecting()
        {
            base.OnConnecting();
            Log("SFTPClient is connecting\n");
        }

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>

        protected override void OnConnected()
        {
            base.OnConnected();
            Log("SFTPClient has connected\n");
        }

    }
}

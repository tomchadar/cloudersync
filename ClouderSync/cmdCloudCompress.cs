using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using ClouderSync.Data;
using ClouderSync.SFTPClient;
using ClouderSync.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClouderSync
{
    internal sealed class cmdCloudCompress : CommandCore
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public static int _commandId = ClouderSyncIDs.cmdidCompress;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        //  public static readonly Guid CommandSet = ClouderSyncIDs.guidSSHDeployCmdSet;//new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929d");
        public static Guid _commandSet = ClouderSyncIDs.guidCloudSyncCmdSet;// new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

        private System.ComponentModel.BackgroundWorker backgroundWorker;
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        //private readonly CloudSyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="cmdCloudDeploy"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private cmdCloudCompress(ClouderSyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService, _commandSet, _commandId)
        {
            CommandID commandID0 = CreateCommandID(CommandSet, CommandId);
            OleMenuCommand menuCommand0 = CreateMenuCommand(commandID0, this.Execute);
            CommandID commandID1 = CreateCommandID(ClouderSyncIDs.guidSSHDeployCmdSet, CommandId);
            OleMenuCommand menuCommand1 = CreateMenuCommand(commandID1, this.Execute);
#if USE_BKG_WORKER
            InitBackgroundWorker();
#endif            
        }
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static cmdCloudCompress Instance
        {
            get;
            private set;
        }
#if USE_BKG_WORKER
        private void InitBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(this.backgroundWorker_DoWork);
        }
#endif
        protected override void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (myCommand != null)
            {
                if (myCommand.CommandID.ID == ClouderSyncIDs.cmdidCloudDeploy || myCommand.CommandID.ID == ClouderSyncIDs.cmdidCancelDeploy)
                {
                    if (CompressTask.GetIsRunning())
                    {
                        myCommand.Text = @"Cancel file compression";
                    }
                    else
                    {
                        myCommand.Text = @"Compress files to archive";
                    }
                }
            }

        }


        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(ClouderSyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in cmdCloudDeploy's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new cmdCloudCompress(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            if (!CompressTask.GetIsRunning())
            {
                Launch();
            }
            else
            {
                Cancel();
            }
        }
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteAsync(object sender, EventArgs e)
        {
            Launch();
        }
        private async void Cancel()
        {
            ClouderSyncPackage.WriteToOutputWindow("\nCancellation requested \n", "ClouderSync", null, false);
            CompressTask.Cancel();
        }
        private async void Launch()
        {
            //CompressTask task = null;

            ClouderSyncPackage.WriteToOutputWindow("-----\n", "ClouderSync", null, true);

            if (!ConnectEntryData.SettingsExist())
            {
                ShowMessageBox(@"It does not appear that SFTP connection has been configured. Please configure in the next step", "ClouderSync SSH");
                cmdConfigureSSH.ConfigureSSH(this);
                return;
            }
            if (CompressTask.GetIsRunning())
            {
                ShowMessageBox(@"It appears that a compression task is running. Please cancel, or wait, or restart Visual Studio", "ClouderSync Compress");
                return;
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            CompressTask task = new CompressTask(this);
            await Task.Run(() =>
            {
                task.Execute(tokenSource);
            });


        }
#if USE_BKG_WORKER
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs eArgs)
        {
            CompressTask task = null;
            if(eArgs.Argument is CompressTask)
            {
                task = eArgs.Argument as CompressTask;
            }
            if(task!=null)
            {
                task.Execute();
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, "Failed to pass SFTP CompressTask {0}", this.GetType().FullName);
                ShowMessageBox(message, "ClouderSync Deploy");
            }
        }
#endif
    }
}

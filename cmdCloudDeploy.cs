using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using CloudSync.Data;
using CloudSync.SFTPClient;
using CloudSync.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CloudSync
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class cmdCloudDeploy : CommandCore
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = CloudSyncIDs.guidCloudSyncCmdSet;//new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

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
        private cmdCloudDeploy(CloudSyncPackage package, OleMenuCommandService commandService)
            : base(package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            //    var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var menuItem = new MenuCommand(this.ExecuteAsync, menuCommandID);
            commandService.AddCommand(menuItem);
            InitBackgroundWorker();
            
        }
        private void InitBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(this.backgroundWorker_DoWork);
        }
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static cmdCloudDeploy Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private AsyncPackage ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(CloudSyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in cmdCloudDeploy's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new cmdCloudDeploy(package, commandService);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);

            CloudSyncPackage.WriteToOutputWindow("-----\n", "CloudSync", null, true);
            DeploymentTask task = new DeploymentTask(this);
            //Array arrInfo = GetSelectedItems(true);
           // backgroundWorker.RunWorkerAsync(task);
        }
        private async void ExecuteAsync(object sender, EventArgs e)
        {
            CloudSyncPackage.WriteToOutputWindow("-----\n", "CloudSync", null, true);
            DeploymentTask task = new DeploymentTask(this);
            await Task.Run(() =>
            {
                task.Execute();
            });
            
        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs eArgs)
        {
            DeploymentTask task = null;
            if(eArgs.Argument is DeploymentTask)
            {
                task = eArgs.Argument as DeploymentTask;
            }
            if(task!=null)
            {
                task.Execute();
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, "Failed to pass SFTP DeploymentTask {0}", this.GetType().FullName);
                ShowMessageBox(message, "CloudSync Deploy");
            }
        }
    }
}

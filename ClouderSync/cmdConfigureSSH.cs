using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using ClouderSync.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClouderSync
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class cmdConfigureSSH : CommandCore
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public static int _commandId = 0x0102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static Guid _commandSet = ClouderSyncIDs.guidCloudSyncConfigureSSHSet;//new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        //private readonly AsyncPackage package;
        //private CloudSyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="cmdConfigureSSH"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private cmdConfigureSSH(ClouderSyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService, _commandSet, _commandId)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static cmdConfigureSSH Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(ClouderSyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in cmdConfigureSSH's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new cmdConfigureSSH(package, commandService);
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
            ConfigureSSH(this);
            return;
        }
        public static void ConfigureSSH(CommandCore cc)
        {
            string projectRootPath = string.Empty;

            /*
            ConnectionList listForm = new ConnectionList(package);
            listForm.ShowDialog();
            */
            projectRootPath = cc.GetProjectRoot();

            ConnectionEntryForm dlgSettings = new ConnectionEntryForm(cc.Package, projectRootPath);
            dlgSettings.ProjectDirectory = projectRootPath;

            DialogResult res = dlgSettings.ShowDialog();


        }
    }
}

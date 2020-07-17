using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CloudSync
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class cmdCommandWindow : CommandCore
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0104;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = CloudSyncIDs.guidCloudSyncCmdSet;// new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        //private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="cmdCommandWindow"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private cmdCommandWindow(CloudSyncPackage package, OleMenuCommandService commandService)
            : base(package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static cmdCommandWindow Instance
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
            // Switch to the main thread - the call to AddCommand in cmdCommandWindow's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new cmdCommandWindow(package, commandService);
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
            string title = "cmdCommandWindow";

            Array arrInfo = GetSelectedItemArray(title);
            if (arrInfo != null)
            {
                ItemInfo pSelected = (ItemInfo)arrInfo.GetValue(0);
                if (!pSelected.HasPath)
                {
                    ShowMessageBox("The selected item \"" + pSelected.Name + "\" has no valid file path", title);
                    return;
                }
                OpenCommandPrompt(Path.GetDirectoryName(pSelected.FilePath));
            }

            // Show a message box to prove we were here
            /*
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                */
        }
    }
}

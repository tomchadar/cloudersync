﻿using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClouderSync
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class cmdCommandWindow : CommandCore
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        static int _commandId = 0x0104;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        static Guid _commandSet = ClouderSyncIDs.guidCloudSyncCmdSet;// new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

        /// <summary>
        /// Initializes a new instance of the <see cref="cmdCommandWindow"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private cmdCommandWindow(ClouderSyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService,_commandSet, _commandId)
        {

            OleMenuCommand menuCommand= CreateMenuCommand(CommandSet, CommandId, this.ExecuteCommand);

/*

            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, base.CommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
*/
              /*
             * var menuItem = new OleMenuCommand
             * menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
             */
  
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static cmdCommandWindow Instance
        {
            get;
            private set;
        }

        //public static int CommandId1 => commandId1;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(ClouderSyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in cmdCommandWindow's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new cmdCommandWindow(package, commandService);
        }

        protected override bool RunCommand()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "Command Window";

            string selectedDir = GetSelectedItemDirectory(title);
            if (!string.IsNullOrEmpty(selectedDir))
            {
                OpenCommandPrompt(selectedDir);
            }
            return true;
        }
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected override void ExecuteCommand(object sender, EventArgs e)
        {
            RunCommand();
        }
    }
}

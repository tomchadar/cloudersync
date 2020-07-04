using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace ClouderSync
{
    public class BaseCommand : CommandAbstraction
    {
        protected static ClouderSyncPackage _package = null;

        
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int _commandId = 0x0000;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        //  public static readonly Guid CommandSet = ClouderSyncIDs.guidSSHDeployCmdSet;//new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929d");
        public Guid _commandSet = Guid.Empty;// new Guid("d10c84e7-2237-4b60-85eb-e0e1864f929e");

        protected OleMenuCommandService _commandService = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="cmdCloudDeploy"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        protected BaseCommand(ClouderSyncPackage package, OleMenuCommandService commandService, Guid commandSet, int commandId)
        {
            if (_package == null)
            {
                _package = package ?? throw new ArgumentNullException(nameof(package));
            }
            CommandSet=commandSet;
            CommandId=commandId;
            _commandService = commandService;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        protected AsyncPackage ServiceProvider
        {
            get
            {
                return _package;
            }
        }
        public virtual int CommandId
        {
            get; protected set;
        }
        public virtual Guid CommandSet
        {
            get; protected set;
        }
        public static CommandID CreateCommandID(Guid commandGuid, int commandId)
        {
            CommandID menuCommandID = null;
            try
            {
                menuCommandID = new CommandID(commandGuid, commandId);
            }
            catch(Exception eCmd)
            {
                Debug.Write(eCmd.Message);
            }
            return menuCommandID;
        }
        protected OleMenuCommand CreateMenuCommand(Guid commandGuid, int commandId = 0, EventHandler invokeHandler = null, EventHandler beforeQueryStatus = null)
        {
            if (commandGuid == Guid.Empty)
            {
                commandGuid = CommandSet;
            }
            if (commandId == 0)
            {
                commandId = CommandId;
            }
            CommandID menuCommandID = CreateCommandID(commandGuid, commandId);
            if(menuCommandID!=null)
            {
                return CreateMenuCommand(menuCommandID, invokeHandler, beforeQueryStatus);
            }
            return null;

        }

        protected OleMenuCommand CreateMenuCommand(CommandID menuCommandID, EventHandler invokeHandler=null, EventHandler beforeQueryStatus=null)
        {
            OleMenuCommand menuItem = null;
            if (menuCommandID != null)
            {
                menuItem = new OleMenuCommand(invokeHandler, OnMenuChange, (EventHandler)OnBeforeQueryStatus, menuCommandID);

                if (_commandService != null)
                {
                    _commandService.AddCommand(menuItem);
                }
            }
            /*
             * menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
             */
            return menuItem;

        }
        protected virtual void ExecuteCommand(object sender, EventArgs e)
        {

        }
        protected virtual void OnMenuChange(object sender, EventArgs e)
        {

        }
        protected virtual void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null != myCommand)
            {
                //myCommand.Text = "New Command Window Text";
            }

        }

        protected virtual bool RunCommand()
        {
            return false;
        }
        public virtual  async Task<bool> RunCommandAsync()
        {
            return false;
        }
        public static void SetMenuText(IntPtr pCmdTextInt, string text)
        {
            try
            {
                var pCmdText = (OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(OLECMDTEXT));
                char[] menuText = text.ToCharArray();

                // Get the offset to the rgsz param.  This is where we will stuff our text
                IntPtr offset = Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz");
                IntPtr offsetToCwActual = Marshal.OffsetOf(typeof(OLECMDTEXT), "cwActual");

                // The max chars we copy is our string, or one less than the buffer size,
                // since we need a null at the end.
                int maxChars = Math.Min((int)pCmdText.cwBuf - 1, menuText.Length);

                Marshal.Copy(menuText, 0, (IntPtr)((long)pCmdTextInt + (long)offset), maxChars);

                // append a null character
                Marshal.WriteInt16((IntPtr)((long)pCmdTextInt + (long)offset + maxChars * 2), 0);

                // write out the length +1 for the null char
                Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + (long)offsetToCwActual), maxChars + 1);
            }
            catch (Exception ex)
            {
                //   Logger.Log(ex);
            }
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (pguidCmdGroup == CommandSet && prgCmds[0].cmdID == CommandId)
            {
                prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                return VSConstants.S_OK;
            }
            return VSConstants.S_OK;
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (pguidCmdGroup == CommandSet && nCmdID == CommandId)
            {
                RunCommand();
//                ThreadHelper.JoinableTaskFactory.RunAsync(MakePrettierAsync);
                return VSConstants.S_OK;
            }
            return (int)(-2147221248);// OleConstants.OLECMDERR_E_NOTSUPPORTED;
            return Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

    }
}

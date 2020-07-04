using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClouderSync
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", LanguageIndependentName="ClouderSync", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
   // [ProvideMenuResource("Menus.ctmenu", ClouderSyncIDs.cmdidCloudDeploy)]
    [Guid(ClouderSyncIDs.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    //[ProvideAutoLoad(cmdUiContextGuid: VSConstants.UICONTEXT.SolutionExists_string, flags: PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class ClouderSyncPackage : AsyncPackage
    {
        /// <summary>
        /// cmdCommandWindowPackage GUID string.
        /// </summary>
       // public const string PackageGuidString = "2cdac2c7-3110-48ec-8d71-f028f5c612ac";
        private static string vsAppFolder = string.Empty;
        private MenuCommandService _menuCommandService = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="ClouderSyncPackage"/> class.
        /// </summary>
        public ClouderSyncPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            InitAppSettings();
        }
        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        public AsyncPackage ServiceProvider
        {
            get
            {
                return this;
            }
        }
        public MenuCommandService MenuCommandService
        {
            get
            {
                return _menuCommandService;
            }
        }
        public void InitAppSettings()
        {
//            SettingsManagerCreator.GetCurrentVSVersion();

            SettingsManager sm=CloudSyncSettings.GetAppSettings(this);

            string vsFolder = sm.GetApplicationDataFolder(ApplicationDataFolder.LocalSettings);
            if(vsFolder.EndsWith("\\"))
            {
                vsFolder=vsFolder.TrimEnd(Path.DirectorySeparatorChar);
            }
            vsAppFolder = Path.GetDirectoryName(vsFolder);
             
//            var store = sm.GetWritableSettingsStore(SettingsScope.UserSettings);

        }
        public static string GetVSAppFolder()
        {
            return vsAppFolder;
        }
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.

            var commandService = await GetServiceAsync(typeof(System.ComponentModel.Design.IMenuCommandService)) as OleMenuCommandService;

            

            if (commandService is MenuCommandService)
            {
                _menuCommandService = commandService;
            }
            await cmdCommandWindow.InitializeAsync(this);
            await cmdCloudDeploy.InitializeAsync(this);
            await cmdConfigureSSH.InitializeAsync(this);
//            await cmdDeployWindow.InitializeAsync(this);
        }

        #endregion
        public object GetServiceOfType(Type serviceType)
        {
            return GetService(serviceType);
        }
        public async Task<object> GetServiceOfTypeAsync(Type serviceType)
        {
            object objService = await GetServiceAsync(serviceType);
            return objService;
        }
        public static IVsOutputWindowPane GetWindowPane(Guid paneGuid, string title, bool visible, bool clearWithSolution)
        {
            if(paneGuid==Guid.Empty)
            {
                paneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            }
            IVsOutputWindow outputWindow = (IVsOutputWindow)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsOutputWindow));
            IVsOutputWindowPane pane;

            outputWindow.GetPane(ref paneGuid, out pane);
            if (pane == null)
            {
                // Retrieve the new pane.
                // Create a new pane.
                outputWindow.CreatePane(ref paneGuid,
                    title,
                    Convert.ToInt32(visible),
                    Convert.ToInt32(clearWithSolution));
                outputWindow.GetPane(ref paneGuid, out pane);
            }
            return pane;
        }

        public static void WriteToOutputWindow(string Message, string szCaption = "ClouderSync", string szPaneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_string, bool bClear = false)
        {
            if(szPaneGuid==null || szPaneGuid==string.Empty)
            {
                szPaneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_string;
            }
            Guid guidPane = new Guid(szPaneGuid);

            IVsOutputWindowPane pane;
            pane = GetWindowPane(guidPane, szCaption, true, true);

            if (pane != null)
            {
                if (bClear)
                {
                    pane.Clear();
                }
                pane.Activate();
                pane.OutputStringThreadSafe(Message);
            }

        }
        public static void Log(string message)
        {
            WriteToOutputWindow(message, "ClouderSync", null, true);
        }

    }
}

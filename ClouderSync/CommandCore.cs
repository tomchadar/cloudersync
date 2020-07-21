using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using Microsoft.VisualStudio;
using System.Reflection;
using static Microsoft.VisualStudio.VSConstants;

namespace ClouderSync
{
    public class CommandCore : BaseCommand
    {
        protected static IVsSolution _vsSolution = null;

        protected CommandCore(ClouderSyncPackage package, OleMenuCommandService commandService, Guid commandSet, int commandId = _commandId)
            : base(package, commandService, commandSet, commandId)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            //_package = package;
            if (_vsSolution == null)
            {
                _vsSolution = _package.GetServiceOfType(typeof(SVsSolution)) as IVsSolution;
            }

        }

        public ClouderSyncPackage Package
        {
            get
            {
                return _package;
            }
        }
        public IVsSolution VSSolution
        {
            get
            {
                if (_vsSolution == null)
                {
                    _vsSolution = _package.GetServiceOfType(typeof(SVsSolution)) as IVsSolution;
                }
                return _vsSolution;
            }
        }

        [DllImport("User32.dll")]
        public static extern bool SetWindowText(IntPtr hwnd, string title);

        public static void OpenCommandPrompt(String directoryName)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = @directoryName;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
            //p.StartInfo.RedirectStandardInput = true;
            p.Start();
            SetWindowText(p.MainWindowHandle, directoryName.ToString());
        }
        protected void ShowMessageBox(string message, string title = "Message")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Enumerates files in the solution projects.
        /// </summary>
        /// <returns>File names collection.</returns>
        private List<VSPROJECTEX> ListSolutionProjects(IVsSolution vsSolution, bool bExcludeNonPhysical = true)
        {
            List<VSPROJECTEX> result = new List<VSPROJECTEX>();
            foreach (IVsProject project in EnumerateSolutionProjects(vsSolution))
            {
                IVsHierarchy vsHierarchy = project as IVsHierarchy;
                string canonicalProjectName = string.Empty;
                Guid typeGUID = Guid.Empty;
                if (bExcludeNonPhysical)
                {
                    if (vsHierarchy != null)
                    {
                        if (ErrorHandler.Succeeded(vsHierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGUID)))
                        {
                            if (typeGUID == Guid.Empty)
                            {
                                continue;
                            }
                            if (typeGUID == VSConstants.GUID_ItemType_VirtualFolder)
                            {
                                continue;
                            }
                        }
                        if (!ErrorHandler.Succeeded(vsHierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out canonicalProjectName)))
                        {
                            continue;
                        }
                        if (!FileTools.FileExists(canonicalProjectName))
                        {
                            continue;
                        }
                    }
                }

                VSPROJECTEX vsp = new VSPROJECTEX(project, (uint)VSConstants.VSITEMID.Nil, canonicalProjectName);
                vsp.projectDirectory = canonicalProjectName;
                //vsp.canonicalName = canonicalProjectName;
                result.Add(vsp);

            }
            return result;
        }
        /// <summary>
        /// Enumerates files in the solution projects.
        /// </summary>
        /// <returns>File names collection.</returns>
        private List<VSITEMEX> ListProjectsAsVSITEMEXs(IList<VSPROJECTEX> projectList, bool bExcludeSolution = true)
        {
            List<VSITEMEX> result = new List<VSITEMEX>();
            foreach (VSPROJECTEX project in projectList)
            {
                if (bExcludeSolution)
                {
                    if (project.IsRootSolution())
                    {
                        continue;
                    }
                }
                foreach (VSITEMSELECTION vsSelItem in EnumerateProjectItems(project.vsProject))
                {
                    VSITEMEX vsi = new VSITEMEX(vsSelItem.pHier, vsSelItem.itemid, project.canonicalName, VSSolution);
                    result.Add(vsi);
                }

            }
            return result;
        }

        private List<VSITEMEX> ProjectsToVSITEMEXs(IList<VSPROJECTEX> projectList)
        {
            List<VSITEMEX> result = new List<VSITEMEX>();
            foreach (VSPROJECTEX project in projectList)
            {
                VSITEMEX vsi = project as VSITEMEX;
                /*
                vsi.pHier = project.pHier;
                vsi.itemid = project.itemid;
                vsi.canonicalName = project.canonicalName;
                vsi.vsParentProject = new VSITEMEX() { pHier = vsSelItem.pHier, itemid = vsSelItem.itemid, canonicalName = project.canonicalName };
                */
                result.Add(vsi);
            }
            return result;
        }
        protected IList<VSITEMEX> ListProjectItems(IVsProject vsProject, uint childID = VSConstants.VSITEMID_NIL)
        {
            IList<VSITEMEX> result = new List<VSITEMEX>();
            uint startID = VSConstants.VSITEMID_ROOT;
            if (VSITEMEX.IsItemIDReal(childID))
            {
                startID = childID;
            }
            foreach (VSITEMSELECTION vsSelItem in EnumerateProjectItems(vsProject, startID, (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_Branch)))
            {
                result.Add(new VSITEMEX(vsSelItem.pHier, vsSelItem.itemid, null, VSSolution));
            }
            return result;
        }

        internal static IEnumerable<VSITEMSELECTION> EnumerateProjectItems(IVsProject project, uint startID = (uint)VSConstants.VSITEMID_ROOT, uint flags = (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest))
        {
            var enumHierarchyItemsFactory = ClouderSyncPackage.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;
            var hierarchy = (IVsHierarchy)project;
            if (enumHierarchyItemsFactory != null && project != null)
            {
                IEnumHierarchyItems enumHierarchyItems;
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        flags,
                        startID,
                        out enumHierarchyItems)))
                {
                    if (enumHierarchyItems != null)
                    {
                        VSITEMSELECTION[] rgelt = new VSITEMSELECTION[1];
                        uint fetched;
                        while (VSConstants.S_OK == enumHierarchyItems.Next(1, rgelt, out fetched) && fetched == 1)
                        {
                            yield return rgelt[0];
                        }
                    }
                }
            }
        }

        public static IEnumerable<IVsProject> EnumerateSolutionProjects(IVsSolution vsSolution)
        {
            if (vsSolution == null)
            {
                Debug.Fail("Failed to get SVsSolution service.");
                yield break;
            }
            var flags = (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);

            Guid guid = Guid.Empty;
            IEnumHierarchies hierarchies;
            vsSolution.GetProjectEnum(flags, ref guid, out hierarchies);
            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1)
            {

                yield return (IVsProject)hierarchy[0];
            }
        }
        /// <summary>
        /// Get reference to IVsUIHierarchyWindow interface from guid persistence slot.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="persistenceSlot">Unique identifier for a tool window created using IVsUIShell::CreateToolWindow. 
        /// The caller of this method can use predefined identifiers that map to tool windows if those tool windows 
        /// are known to the caller. </param>
        /// <returns>A reference to an IVsUIHierarchyWindow interface.</returns>
        public static IVsUIHierarchyWindow GetUIHierarchyWindow(IServiceProvider serviceProvider, Guid persistenceSlot)
        {
            if (serviceProvider == null)
            {
                Debug.Fail("serviceProvider");
            }

#pragma warning disable IDE0019 // Use pattern matching
            IVsUIShell shell = serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
#pragma warning restore IDE0019 // Use pattern matching
            if (shell != null)
            {
                object pvar;
                IVsWindowFrame frame;

                if (ErrorHandler.Succeeded(shell.FindToolWindow(0, ref persistenceSlot, out frame)) &&
                    ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar)))
                {
                    return pvar as IVsUIHierarchyWindow;
                }
            }
            return null;
        }
        public List<VSITEMEX> GetSolutionExplorerHierarchy(ref IVsHierarchy rootHierarchy)
        {
            List<VSITEMEX> results = new List<VSITEMEX>();
            IVsUIHierarchyWindow vsUIHierarchyWindow = null;
            IVsMultiItemSelect ppMIS = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;
            uint itemID = 0;


            var shell = (IVsUIShell)ClouderSyncPackage.GetGlobalService(typeof(SVsUIShell));
            var solutionExplorerToolWindowGuid = new Guid(ToolWindowGuids.SolutionExplorer);
            IVsWindowFrame solutionExplorerFrame;
            object vshw = null;

            shell.FindToolWindow(0, ref solutionExplorerToolWindowGuid, out solutionExplorerFrame);
            if (solutionExplorerFrame != null)
            {
                if (!ErrorHandler.Succeeded(solutionExplorerFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out vshw)))
                {
                    return results;
                }
            }
            vsUIHierarchyWindow = vshw as IVsUIHierarchyWindow;
            if (vsUIHierarchyWindow == null)
            {
                return results;
            }
            vsUIHierarchyWindow.GetCurrentSelection(out hierarchyPtr, out itemID, out ppMIS);
            if (hierarchyPtr != IntPtr.Zero)
            {
                IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                rootHierarchy = hierarchy;
                ParseHierarchyPtr(hierarchyPtr, itemID, true, ref results);
                //results.Add(new VSITEMEX(hierarchy, itemID, null, VSSolution));
            }
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            return results;
        }
        protected bool ParseHierarchyPtr(IntPtr hierarchyPtr, uint itemID, bool bMarkSelected, ref List<VSITEMEX> results)
        {
            if (hierarchyPtr == IntPtr.Zero)
            {
                return false;
            }
            IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
            IVsProject iVsProject = hierarchy as IVsProject;
            VSITEMEX itemex = null;
            if (iVsProject != null)
            {
                itemex = new VSPROJECTEX(iVsProject, itemID, null, VSSolution);
            }
            else
            {
                itemex = new VSITEMEX(hierarchy, itemID, null, VSSolution);
            }
            if (itemex == null)
            {
                return false;
            }

            itemex.isItemSelected = bMarkSelected;
            itemex.GetCanonicalName();
            if (!VSITEMEX.Exists(results, itemex.FullFilePath))
            {
                results.Add(itemex);
            }
            return true;
        }
        public List<VSITEMEX> GetSelectedHierarchyItems(IVsMonitorSelection selectionMonitor)
        {
            List<VSITEMEX> results = new List<VSITEMEX>();

            int hr = VSConstants.S_OK;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;

            uint itemCount = 0;
            int fSingleHierarchy = 0;

            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);

            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");

            if (itemID == (uint)VSConstants.VSITEMID.Root)
            {
                IVsHierarchy rootHierarchy = null;
                VSITEMEX itemex = null;

                if (hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                    rootHierarchy = hierarchy;

                    IVsProject iVsProject = hierarchy as IVsProject;
                    if (iVsProject != null)
                    {
                        itemex = new VSPROJECTEX(iVsProject, itemID, null, VSSolution);
                    }
                    else
                    {
                        itemex = new VSITEMEX(hierarchy, itemID, null, VSSolution);
                    }
                    itemex.GetCanonicalName();
                    itemex.GetItemType();
                }
                else
                {
                    GetSolutionExplorerHierarchy(ref rootHierarchy);
                    if (rootHierarchy != null)
                    {
                        IVsProject iVsProject = rootHierarchy as IVsProject;
                        if (iVsProject != null)
                        {
                            itemex = new VSPROJECTEX(iVsProject, itemID, null, VSSolution);
                        }
                        else
                        {
                            itemex = new VSITEMEX(rootHierarchy, itemID, null, VSSolution);
                        }
                        itemex.GetCanonicalName();
                        itemex.GetItemType();
                    }
                }
                if (itemex != null)
                {
                    Guid guidCanonical;
                    if (Guid.TryParse(itemex.canonicalName, out guidCanonical))
                    {
                        itemex.ParseCanonical();
                        itemex.GetCanonicalName();
                    }
                    itemex.GetParentID();
                }
                itemex.isItemSelected = true;
                if (!VSITEMEX.Exists(results, itemex.FullFilePath))
                {
                    results.Add(itemex);
                }
            }
            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                //SolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}"
                hr = multiSelect.GetSelectionInfo(out itemCount, out fSingleHierarchy);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectionInfo failed.");

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                hr = multiSelect.GetSelectedItems(0, itemCount, items);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectedItems failed.");

                foreach (VSITEMSELECTION item in items)
                {
                    IVsProject iVsProject = item.pHier as IVsProject;
                    VSPROJECTEX vsProject = null;
                    VSITEMEX itemEx = null;
                    if (iVsProject != null)
                    {
                        vsProject = new VSPROJECTEX(iVsProject, item.itemid, null, VSSolution);
                        vsProject.itemid = item.itemid;
                    }
                    if (vsProject != null)
                    {
                        itemEx = vsProject as VSITEMEX;
                    }
                    if (itemEx == null)
                    {
                        itemEx = new VSITEMEX(item, null, VSSolution);
                    }
                    itemEx.isSourceSelection = true;
                    itemEx.isItemSelected = true;
                    Guid typeGuid = itemEx.GetItemType();
                    Guid projectGuid = GetProjectGUID(itemEx.pHier);
                    string canonicalName = itemEx.GetCanonicalName();
                    itemEx.GetParentHierarchy();
                    //6bb5f8ee-4483-11d3-8bcf-00c04f8ec28c = GUID_ItemType_PhysicalFile
                    // 6bb5f8f0-4483-11d3-8bcf-00c04f8ec28c = GUID_ItemType_VirtualFolder
                    if (typeGuid == VSConstants.GUID_ItemType_PhysicalFile)
                    {
                        //itemEx.isParentSelected = false;
                    }
                    if (!results.Contains(itemEx))
                    {
                        results.Add(itemEx);
                    }
                }
            }
            //else
            {
                //if (results.Count < 1)
                {
                    // case where no visible project is open (single file)
                    ParseHierarchyPtr(hierarchyPtr, itemID, true, ref results);
                }
            }
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            return results;
        }
        public IList<VSITEMEX> GetSelectedHierarchyItem(IVsMonitorSelection selectionMonitor)
        {
            List<VSITEMEX> results = new List<VSITEMEX>();

            int hr = VSConstants.S_OK;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;

            uint itemCount = 0;
            int fSingleHierarchy = 0;

            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);

            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");
            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                //SolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}"
                hr = multiSelect.GetSelectionInfo(out itemCount, out fSingleHierarchy);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectionInfo failed.");

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                hr = multiSelect.GetSelectedItems(0, itemCount, items);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectedItems failed.");

                foreach (VSITEMSELECTION item in items)
                {
                    VSITEMEX itemEx = new VSITEMEX(item);
                    // IVsProject project = GetProjectOfItem(item.pHier, item.itemid);
                    if (!VSITEMEX.Exists(results,itemEx.FullFilePath))
                    {
                        results.Add(itemEx);
                    }
                }
            }
            else
            {
                // case where no visible project is open (single file)
                if (hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);

                    VSITEMEX itemEx = new VSITEMEX(hierarchy, itemID);
                    if (!results.Contains(itemEx))
                    {
                        results.Add(itemEx);
                    }
                }
            }
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            return results;
        }
        internal static string GetItemName(IVsHierarchy hier, uint itemid)
        {
            object name;
            if (!ErrorHandler.Succeeded(hier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out name)))
            {
                return string.Empty;
            }
            return (string)name;
        }
        private static IVsHierarchy GetSelectionContainer(IVsHierarchy hierarchy, uint itemId)
        {
            object parent;

            hierarchy.GetProperty(
                itemId,
                (int)__VSHPROPID.VSHPROPID_SelContainer,
                out parent
            );

            if (parent is IVsHierarchy)
            {
                return (parent as IVsHierarchy);
            }
            Debug.Write("Failed to get SelectionContainer.");
            return null;
        }

        private string GetProjectDir(IVsHierarchy hierarchy, uint itemId, bool bFailIfNotFound = false)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            object directory;
#pragma warning restore IDE0018 // Inline variable declaration

            hierarchy.GetProperty(
                itemId,
                (int)__VSHPROPID.VSHPROPID_ProjectDir,
                out directory
            );

            if (directory is string)
            {
                return directory.ToString();
            }
            hierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ProjectDir,
                out directory
             );
            if (directory is string)
            {
                return directory.ToString();
            }
            if (bFailIfNotFound)
            {
                return string.Empty;
            }
            if (itemId == (uint)VSConstants.VSITEMID.Root)
            {
                return GetSolutionDirectory();
            }
            Debug.Write("Failed to get ProjectDir.");
            return string.Empty;
        }
        private static string GetCanonicalName(IVsHierarchy hierarchy, uint itemId)
        {
            string canonicalName = string.Empty;
            uint newItemID = 0;

            if (ErrorHandler.Succeeded(hierarchy.GetCanonicalName(itemId, out canonicalName)))
            {
                return canonicalName;
            }
            return string.Empty;
        }

        private static string GetItemProperty(IVsHierarchy hierarchy, uint itemId, int VSPROPID)
        {
            object directory;

            hierarchy.GetProperty(
                itemId,
                (int)VSPROPID,
                out directory
            );

            if (directory is string)
            {
                return directory.ToString();
            }
            Debug.Fail("Failed to get ProjectDir.");
            return string.Empty;
        }

        //since 1.9.7
        protected string GetSelectedItemDirectory(Array arrInfo, string title = "cmdCommandWindow")
        {
            string selectedDir = string.Empty;
            if (arrInfo != null)
            {
                if (arrInfo.Length < 1)
                {
                    ShowMessageBox("No items selected", title);
                    return string.Empty;
                }
                ItemInfo pSelected = (ItemInfo)arrInfo.GetValue(0);
                if (!pSelected.HasPath)
                {
                    ShowMessageBox("The selected item \"" + pSelected.Name + "\" has no valid file path", title);
                    return string.Empty;
                }
                selectedDir = Path.GetDirectoryName(pSelected.FilePath);
            }
            return selectedDir;

        }

        //since 1.9.7
        public string GetSelectedItemDirectory(ref string fullItemFilePath)
        {
            string selectedDir = string.Empty;
            string projectDir = string.Empty;
            string canonicalName = string.Empty;
            string solutionDirectory = string.Empty;
            string solutionFilePath = string.Empty;
            string itemName = string.Empty;
            List<ItemInfo> listInfo = new List<ItemInfo>();
            IVsMonitorSelection monitorSelection = null;
            IList<VSITEMEX> vsRootSelectItems = new List<VSITEMEX>();
            Guid typeGUID = Guid.Empty;
            IVsHierarchy vsHierParent = null;
            uint parentId = 0;

            monitorSelection = _package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            solutionDirectory = GetSolutionDirectory(ref solutionFilePath);

            //            ItemInfo[] selectItems=GetSelectedItems(false);
            List<VSITEMEX> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            //IList<VSITEMEX> vsSelectItems = GetSelectedHierarchyItems(selectionMonitor);

            if (vsSelectItems.Count < 1)
            {
                fullItemFilePath = solutionFilePath;
                return solutionDirectory;
            }
            foreach (VSITEMEX vsItem in vsSelectItems)
            {
                itemName = vsItem.GetItemName();
                typeGUID = vsItem.GetItemType();
                vsHierParent = vsItem.GetParentHierarchy();
                parentId = vsItem.GetParentID();
                projectDir = vsItem.FileDirectory;// GetProjectDir(vsItem.pHier, vsItem.itemid, true);
                canonicalName = vsItem.FullFilePath;

                
                if (typeGUID == VSConstants.GUID_ItemType_VirtualFolder)
                {
                    fullItemFilePath = Path.Combine(vsItem.SolutionDirectory, itemName);
                    return fullItemFilePath;
                }

                if (!string.IsNullOrEmpty(canonicalName))
                {
                    if (FileTools.FileExists(canonicalName))
                    {
                        fullItemFilePath = canonicalName;
                        return vsItem.FileDirectory;
                    }
                    else
                    {
                        VSITEMEX vsParentItem = vsItem.GetParentItemEx();
                        if (vsParentItem != null)
                        {
                            selectedDir = vsParentItem.FileDirectory;
                            fullItemFilePath = vsParentItem.FullFilePath;
                            if (FileTools.FileExists(projectDir))
                            {
                                return selectedDir;//.GetCanonicalName();
                            }
                        }
                    }
                }
                return string.Empty;
                /*
                if (FileTools.FileExists(projectDir))
                {
                    fullItemFilePath = projectDir;
                    return projectDir;
                }
                
                selectedDir = Path.Combine(solutionDirectory, itemName);
                if (FileTools.FileExists(selectedDir))
                {
                    fullItemFilePath = selectedDir;
                    return selectedDir;
                }
                */
            }
            return string.Empty;
        }

        protected Array GetSelectedItemArray(string title = "cmdCommandWindow")
        {
            Array arrInfo = GetSelectedItems();
            if (arrInfo != null)
            {
                if (arrInfo.Length < 1)
                {
                    //                    ShowMessageBox("No items selected", title);
                    return null;
                }
                ItemInfo pSelected = (ItemInfo)arrInfo.GetValue(0);
                if (!pSelected.HasPath)
                {
                    ShowMessageBox("The selected item \"" + pSelected.Name + "\" has no valid file path", title);
                    return null;
                }
            }
            return arrInfo;

        }
        //since 1.9.7
        private static string ToCaseSensitive(string sDirOrFile)
        {
            string sTmp = "";
            string[] dirs = sDirOrFile.Split('\\');
            foreach (string sPth in dirs)
            {
                if (string.IsNullOrEmpty(sTmp))
                {
                    sTmp = sPth + "\\";
                    continue;
                }
                sTmp = Directory.GetFileSystemEntries(sTmp, sPth)[0];
            }
            if (sTmp.EndsWith("\\") && !sDirOrFile.EndsWith("\\"))
            {
                return sTmp.TrimSuffix("\\");
            }

            return sTmp;
        }
        //since 1.9.7
        public static bool ConvertToCaseSensitive(string pathIn, ref string pathOut)
        {
            if (string.IsNullOrEmpty(pathIn))
            {
                return false;
            }
            //pathOut = pathIn;
            try
            {
                pathOut = ToCaseSensitive(pathIn);
            }
            catch (Exception eConvert)
            {
                pathOut = pathIn;
                Debug.Write(eConvert.Message);
                return false;
            }
            return true;
        }
        //since 1.9.7
        public string GetTopmostRoot(bool bExcludeSolution = false)
        {
            string rootDir = string.Empty;

            if (!bExcludeSolution)
            {
                rootDir = GetSolutionDirectory();
            }
            if (FileTools.FileExists(rootDir))
            {
                return rootDir;
            }
            rootDir = GetProjectRoot();
            return rootDir;
        }
        public string GetProjectRoot()
        {
            List<ItemInfo> arrInfo = new List<ItemInfo>();
            string projectDirectory = string.Empty;
            string caseSensitivePath = string.Empty;

            IVsMonitorSelection monitorSelection;
            monitorSelection = _package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            List<VSITEMEX> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            if (vsSelectItems.ToArray().Length < 1)
            {
                return string.Empty;
            }
            foreach (VSITEMEX vsSelectItem in vsSelectItems)
            {
                Guid itemType = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string fullPath = string.Empty;


                itemName = GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);

                vsSelectItem.pHier.GetCanonicalName(vsSelectItem.itemid, out canonicalName);
                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);
                if (string.IsNullOrEmpty(projectDirectory))
                {
                    projectDirectory = Path.GetDirectoryName(canonicalName);
                }
            }
            if (ConvertToCaseSensitive(projectDirectory, ref caseSensitivePath))
            {
                return caseSensitivePath;
            }
            return projectDirectory;
        }
        public static bool GetBoolProperty(IVsHierarchy vshItem, uint itemid, int propID, bool bDefault = false)
        {
            object objProperty = bDefault;
            //(int)__VSHPROPID.VSHPROPID_IsNonMemberItem
            vshItem.GetProperty(itemid, propID, out objProperty);
            if (objProperty is bool)
            {
                return (bool)objProperty;
            }
            return bDefault;

        }
        public static bool TryParseCanonical(VSITEMEX vshParent, string targetDirectory, string fileName, ref uint itemID)
        {
            string canonicalName = Path.Combine(targetDirectory, fileName);
            uint id = VSITEMID_NIL;
            if (vshParent == null)
            {
                return false;
            }
            if (vshParent.ParseCanonicalName(canonicalName, ref id) == VSConstants.S_OK)
            {
                if(VSITEMEX.IsItemIDReal(id))
                {
                    itemID = id;
                    return true;
                }
                
            }
            string rootDir = vshParent.FileDirectory;
            int iMinLen = rootDir.Length;
            if (canonicalName.Length < iMinLen)
            {
                iMinLen = rootDir.Length;
            }
            string pathTrail = canonicalName.Substring(iMinLen);
            /*
            if (!FileTools.CanMapRoots(rootDir,targetDirectory))
            {
                return false;
            }
            string pathTrail=FileTools.MapRoots(targetDirectory, rootDir);
            canonicalName = Path.Combine(pathTrail, fileName);
            */
            if (vshParent.ParseCanonicalName(pathTrail, ref itemID) == VSConstants.S_OK)
            {
                if (VSITEMEX.IsItemIDReal(id))
                {
                    itemID = id;
                    return true;
                }

                
            }

            return false;
        }
        public static void ProcessDirectory(string itemName, string targetDirectory, List<ItemInfo> arrInfo, VSITEMEX vshParent, bool bIncludedInProject = true, bool bRecursive = true)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                uint itemID = 0;
                string canonicalName = Path.Combine(targetDirectory, fileName);
                bool isNonMember = bIncludedInProject;
                bool isHidden = false;
                object iResult = VSConstants.E_FAIL;
                if (!ItemInfo.Exists(arrInfo, canonicalName))
                {
                    if (vshParent != null)
                    {
                        if (TryParseCanonical(vshParent, targetDirectory, fileName, ref itemID))
                        {
                            //isNonMember = GetBoolProperty(vshParent.pHier, itemID, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem);
                            if (!ErrorHandler.Succeeded(vshParent.IsNonMember(ref isNonMember, itemID)))
                            {
                                isNonMember = true;
                            }
                            if (!ErrorHandler.Succeeded(vshParent.IsItemHidden(ref isHidden, itemID)))
                            {
                                isHidden = false;
                            }

                        }
                        else
                        {
                            isNonMember = true;
                        }
                    }
                    string fileAlone = Path.GetFileName(fileName);
                    ItemInfo pKidInfo = new ItemInfo(fileAlone, canonicalName, VSConstants.GUID_ItemType_PhysicalFile);
                    pKidInfo.IncludedInProject = (isNonMember ? false : true);
                    pKidInfo.IsHidden = isHidden;
                    arrInfo.Add(pKidInfo);
                }
            }

            // Recurse into subdirectories of this directory.
            if (bRecursive)
            {
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(itemName, subdirectory, arrInfo, vshParent, bIncludedInProject, bRecursive);
                }
            }
        }
        public Guid GetProjectGUID(IVsHierarchy vsHierarchy)
        {
            Guid projectGuid = Guid.Empty;
            if (!(VSSolution is IVsSolution))
            {
                return Guid.Empty;
            }
            if (!(vsHierarchy is IVsHierarchy))
            {
                return Guid.Empty;
            }

            if (ErrorHandler.Succeeded(VSSolution.GetGuidOfProject(vsHierarchy, out projectGuid)))
            {
                return projectGuid;
            }
            return Guid.Empty;
        }
        public int GetProjectRef(IVsHierarchy vsHierarchy, ref string projectRef)
        {
            string pref = string.Empty;
            int iRes = VSConstants.E_FAIL;
            if (!(VSSolution is IVsSolution))
            {
                return VSConstants.E_POINTER;
            }
            if (!(vsHierarchy is IVsHierarchy))
            {
                return VSConstants.E_POINTER;
            }
            iRes = VSSolution.GetProjrefOfProject(vsHierarchy, out pref);
            if (ErrorHandler.Succeeded(iRes))
            {
                projectRef = pref;
            }
            return iRes;
        }

        public int GetSolutionProjectFiles(ref string[] projectNames, ref uint iProjectCount)
        {
            string pref = string.Empty;
            int iRes = VSConstants.E_FAIL;
            //uint iProjectCount = 0;
            uint iProjectsFetched = 0;

            projectNames = new string[0];
            iProjectCount = 0;
            if (!(VSSolution is IVsSolution))
            {
                return VSConstants.E_POINTER;
            }
            //_VSGETPROJFILESFLAGS;//GPFF_SKIPUNLOADEDPROJECTS = 1
            iRes = VSSolution.GetProjectFilesInSolution(0, 0, null, out iProjectCount);
            if (!ErrorHandler.Succeeded(iRes))
            {
                return iRes;
            }
            string[] strProjectNames = new string[iProjectCount];
            iRes = VSSolution.GetProjectFilesInSolution(0, iProjectCount, strProjectNames, out iProjectsFetched);
            if (ErrorHandler.Succeeded(iRes))
            {
                projectNames = strProjectNames;
            }
            return iRes;
        }

        public string GetSolutionDirectory(ref string solutionFile)
        {
            string solutionDirectory = string.Empty;
            string solutionOptFile = string.Empty;

            if (!(VSSolution is IVsSolution))
            {
                return string.Empty;
            }
            VSSolution.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionOptFile);
            return solutionDirectory;
        }
        public string GetSolutionDirectory()
        {
            string solutionDirectory = string.Empty;
            string solutionOptFile = string.Empty;
            string solutionFile = string.Empty;

            if (!(VSSolution is IVsSolution))
            {
                return string.Empty;
            }
            solutionDirectory = GetSolutionDirectory(ref solutionFile);
            return solutionDirectory;
        }

        protected ItemInfo[] ProcessVSItemList(IList<VSITEMEX> vsSelectItems, IList<VSPROJECTEX> projectList, ref List<ItemInfo> arrInfo, bool bIncludeKids = false, bool isSolutionFolder = false)
        {
            string solutionFileName = String.Empty;
            string solutionDirectory = GetSolutionDirectory(ref solutionFileName);
            foreach (VSITEMEX vsItemEx in vsSelectItems)
            {
                Guid typeGUID = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullPath = string.Empty;
                bool isProjectFile = false;
                bool isNonMember = false;
                uint parentId = 0;

                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";
                //                parentId = vsItemEx.GetParentID();
                itemName = GetItemName(vsItemEx.pHier, vsItemEx.itemid);
                typeGUID = vsItemEx.GetItemType();
                canonicalName = vsItemEx.GetCanonicalName();
                Guid guidCanonical;
                parentId = vsItemEx.GetParentID();
                if (typeGUID == Guid.Empty && vsItemEx.itemid == VSConstants.VSITEMID_ROOT)
                {
                    isSolutionFolder = true;
                }
                if (isSolutionFolder)
                {
                    canonicalName = solutionFileName;
                }
                //typeGUID == ItemTypeGuid.VirtualFolder_guid)
                if (Guid.TryParse(canonicalName, out guidCanonical))
                {
                    int parsedID = vsItemEx.ParseCanonical(null, false);
                    if (parsedID != 0)
                    {
                        canonicalName = vsItemEx.GetCanonicalName(true);
                    }
                }
                ConvertToCaseSensitive(canonicalName, ref canonicalName);

                projectDirectory = vsItemEx.GetProjectDirectory();
                if (string.IsNullOrEmpty(projectDirectory))
                {
                    if (parentId == VSConstants.VSITEMID_NIL)
                    {
                        projectDirectory = solutionDirectory;
                    }
                }
                ConvertToCaseSensitive(projectDirectory, ref projectDirectory);

                string projName = GetItemProperty(vsItemEx.pHier, vsItemEx.itemid, (int)__VSHPROPID.VSHPROPID_ProjectName);
                string Name = GetItemProperty(vsItemEx.pHier, vsItemEx.itemid, (int)__VSHPROPID.VSHPROPID_Name);


                if (isSolutionFolder)
                {
                    isProjectFile = true;
                }
                isNonMember = false;
                if (FileTools.FileExists(canonicalName))
                {
                    fullPath = canonicalName;
                }
                else
                {
                    fullPath = Path.Combine(projectDirectory, itemName);
                }

                if (Path.GetFileNameWithoutExtension(fullPath).StartsWith(projName, StringComparison.CurrentCultureIgnoreCase))
                {
                    isProjectFile = true;
                }
                if (typeGUID == VSConstants.GUID_ItemType_SubProject)
                {
                    isProjectFile = true;
                }

                //vshSelContainer = GetSelectionContainer(vsItemEx.pHier, vsItemEx.itemid);

                isNonMember = GetBoolProperty(vsItemEx.pHier, vsItemEx.itemid, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem);
                if (!isNonMember)
                {
                    isNonMember = GetBoolProperty(vsItemEx.pHier, vsItemEx.itemid, (int)__VSHPROPID.VSHPROPID_IsHiddenItem);
                }
                if (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder)
                {
                    fullPath = FileTools.NormalizeDirectoryPath(fullPath);
                }

                if (typeGUID == VSConstants.GUID_ItemType_PhysicalFile)
                {
                    fullPath = FileTools.NormalizeFilePath(fullPath);
                }

                bool bGuidNonEmpty = (typeGUID != Guid.Empty);
                if (isSolutionFolder)
                {
                    bGuidNonEmpty = true;
                }
                bGuidNonEmpty = true;
                ConvertToCaseSensitive(fullPath, ref fullPath);
                bool bFileExists = FileTools.FileExists(fullPath);
                if (bFileExists && (itemName != string.Empty) && bGuidNonEmpty)
                {
                    ItemInfo pInfo = new ItemInfo(itemName, fullPath, typeGUID);
                    arrInfo.Add(pInfo);

                    if (bIncludeKids)// && (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder) || isProjectFile)
                    {
                        string barePath = Path.GetDirectoryName(fullPath);
                        //    string barePath = FileTools.NormalizeDirectoryPath(fullPath);
                        ProcessDirectory(itemName, barePath, arrInfo, vsItemEx, (isNonMember ? false : true));
                    }
                }
            }
            return arrInfo.ToArray();
        }
        protected ItemInfo[] ProcessVSProjectList(IList<VSPROJECTEX> arrProjects, IList<VSITEMEX> vsSelectItems, ref List<ItemInfo> arrInfo, bool bIncludeKids = false, bool isSolutionFolder = false)
        {
            foreach (VSITEMEX vsSelectItem in vsSelectItems)
            {
                Guid typeGUID = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string projectFilename = string.Empty;
                string fullPath = string.Empty;
                bool isProjectFile = false;
                bool isNonMember = false;


                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";
                itemName = GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);
                typeGUID = vsSelectItem.GetItemType();
                projectFilename = vsSelectItem.canonicalName;

                canonicalName = vsSelectItem.GetCanonicalName(true);
                ConvertToCaseSensitive(canonicalName, ref canonicalName);

                ConvertToCaseSensitive(projectFilename, ref projectFilename);
                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);
                projectDirectory = projectFilename;// vsSelectItem.canonicalName;
                ConvertToCaseSensitive(projectDirectory, ref projectDirectory);

                string projName = GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_ProjectName);
                string Name = GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_Name);


                if (isSolutionFolder)
                {
                    isProjectFile = true;
                }
                isNonMember = false;


                if (FileTools.FileExists(canonicalName))
                {
                    fullPath = canonicalName;
                }
                else
                {
                    fullPath = Path.Combine(projectDirectory, itemName);
                }

                if (Path.GetFileNameWithoutExtension(fullPath).StartsWith(projName, StringComparison.CurrentCultureIgnoreCase))
                {
                    isProjectFile = true;
                }
                if (typeGUID == VSConstants.GUID_ItemType_SubProject)
                {
                    isProjectFile = true;
                }

                //vshSelContainer = GetSelectionContainer(vsItemEx.pHier, vsItemEx.itemid);

                isNonMember = GetBoolProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem);
                if (!isNonMember)
                {
                    isNonMember = GetBoolProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_IsHiddenItem);
                }
                if (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder)
                {
                    fullPath = FileTools.NormalizeDirectoryPath(fullPath);
                }

                if (typeGUID == VSConstants.GUID_ItemType_PhysicalFile)
                {
                    fullPath = FileTools.NormalizeFilePath(fullPath);
                }

                bool bGuidNonEmpty = (typeGUID != Guid.Empty);
                if (isSolutionFolder)
                {
                    bGuidNonEmpty = true;
                }
                bGuidNonEmpty = true;
                bool bFileExists = FileTools.FileExists(fullPath);
                if (bFileExists && (itemName != string.Empty) && bGuidNonEmpty)
                {
                    ItemInfo pInfo = new ItemInfo(Path.GetFileName(projectFilename), projectFilename, typeGUID);
                    if (!vsSelectItem.bProcessed)
                    {
                        arrInfo.Add(pInfo);
                        vsSelectItem.bProcessed = true;
                    }

                    pInfo = new ItemInfo(itemName, fullPath, typeGUID);
                    arrInfo.Add(pInfo);

                    if (bIncludeKids && (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder) || isProjectFile)
                    {
                        string barePath = Path.GetDirectoryName(fullPath);
                        barePath = FileTools.NormalizeDirectoryPath(barePath);
                        ProcessDirectory(itemName, barePath, arrInfo, vsSelectItem, (isNonMember ? false : true));
                    }
                }
            }
            return arrInfo.ToArray();
        }
        public static void MarkSelectedItems(List<VSITEMEX> vsSelectItems, List<VSITEMEX> projectList)
        {
            foreach (VSITEMEX vsProject in projectList)
            {
                int iIndex = VSITEMEX.FindIndex(vsSelectItems, vsProject);
                if (iIndex >= 0)
                {
                    projectList[iIndex].isItemSelected = true;
                }
            }

        }
        public static List<VSITEMEX> FilterSelectedItems(List<VSITEMEX> vsSelectItems, List<VSITEMEX> projectList)
        {
            List<VSITEMEX> result = new List<VSITEMEX>();
            foreach (VSPROJECTEX vsProject in projectList)
            {
                int iIndex = VSITEMEX.FindIndex(vsSelectItems, vsProject);
                if (iIndex >= 0)
                {
                    projectList[iIndex].isItemSelected = true;
                    result.Add(projectList[iIndex]);
                }
            }
            return result;
        }

        public ItemInfo[] GetSelectedItems(bool bIncludeKids = false)
        {
            List<ItemInfo> arrInfo = new List<ItemInfo>();
            List<string> listProjNames = new List<string>();
            List<uint> listProjectIDs = new List<uint>();
            IVsMonitorSelection monitorSelection;
            IVsHierarchy vsRootHierarchy = null;
            Guid typeGUID = Guid.Empty;
            string solutionFile = string.Empty;



            string[] szProjectNames = new string[0];
            uint iProjectCount = 0;
            GetSolutionProjectFiles(ref szProjectNames, ref iProjectCount);

            monitorSelection = _package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            List<VSITEMEX> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            List<VSITEMEX> selectKids = new List<VSITEMEX>();
            List<VSITEMEX> vsRootSelectItems = GetSolutionExplorerHierarchy(ref vsRootHierarchy);
            List<VSITEMEX> rootSelections = new List<VSITEMEX>();
            List<VSPROJECTEX> listProjects = new List<VSPROJECTEX>();
            List<VSPROJECTEX> arrProjects = ListSolutionProjects(VSSolution);
            List<VSITEMEX> projectList = ProjectsToVSITEMEXs(arrProjects);// as IList<VSITEMEX>;

            if (vsSelectItems.Count() > 0)
            {
            //    VSITEMEX.ProcessHierarchy(vsSelectItems[0].pHier, ref selectKids);
            }

            if (vsRootSelectItems.Count() > 0)
            {
                VSITEMEX vsRootItem = vsRootSelectItems[0];
                typeGUID = vsRootItem.GetItemType();
                if (vsRootItem.IsRootSolution())
                {
                    //this is ROOT SOLUTION!
                    //   ProcessItems(vsSelectItems, ref arrInfo, bIncludeKids);
                    rootSelections = ListProjectsAsVSITEMEXs(arrProjects);
                    if (!ItemInfo.Exists(arrInfo, vsRootItem.FullFilePath))
                    {
                        arrInfo.Add(new ItemInfo(vsRootItem.GetItemName(), vsRootItem.FullFilePath, typeGUID));
                    }
                    ProcessItems(vsRootSelectItems, ref arrInfo, bIncludeKids, szProjectNames,true);
                    //vsSelectItems = rootSelections;
                    //ProcessItems(projectList, ref arrInfo, bIncludeKids, szProjectNames);
                }
            }
            IterateNodes(vsSelectItems, ref arrInfo, bIncludeKids, szProjectNames);
            
#if false
            List<VSITEMEX> filteredItems=FilterSelectedItems(vsSelectItems, projectList);
            ProcessItems(filteredItems, ref arrInfo, bIncludeKids, szProjectNames);
            if (vsSelectItems.Count() > 0)
            {
                ProcessItems(vsSelectItems, ref arrInfo, bIncludeKids, szProjectNames,false);
            }
#endif
            /*
            if(selectKids.Count()>0)
            {
                ProcessItems(selectKids, ref arrInfo, bIncludeKids, szProjectNames, false);
            }
            */

            return arrInfo.ToArray();
        }
        protected ItemInfo[] IterateNodes(IList<VSITEMEX> vsItems, ref List<ItemInfo> arrInfo, bool bIncludeKids, string[] szProjectNames, bool bProcessPhysicalDir = true)
        {
            foreach (VSITEMEX vsSelectItem in vsItems)
            {
                Guid typeGUID = Guid.Empty;
                Guid projectGUID = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullFilePath = string.Empty;
                IList<VSITEMEX> projectItems = new List<VSITEMEX>();
                bool isProjectFile = false;
                string projectRef = string.Empty;
                uint firstChildID = VSConstants.VSITEMID_NIL;
                uint folderID = VSConstants.VSITEMID_NIL;
                int iRes;
                bool isNonMember = false;

                List<VSITEMEX> hierarchyKids = new List<VSITEMEX>();

                fullFilePath = vsSelectItem.FullFilePath;

                itemName = vsSelectItem.GetItemName();
                typeGUID = vsSelectItem.GetItemType();
                isProjectFile = vsSelectItem.IsProjectFile(szProjectNames);

                iRes = vsSelectItem.IsNonMember(ref isNonMember);
                if (!ErrorHandler.Succeeded(iRes))
                {
                    isNonMember = false;
                }
                if (vsSelectItem.IsRootSolution())
                {
                    isNonMember = false;
                }

                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";

                firstChildID = vsSelectItem.GetFirstChild();

                if (IsFolder(typeGUID))
                {
                    fullFilePath = FileTools.NormalizeDirectoryPath(fullFilePath);
                }
                if(typeGUID == VSConstants.GUID_ItemType_VirtualFolder)
                {
                    vsSelectItem.ParseCanonical();
                    if (string.IsNullOrEmpty(fullFilePath))
                    {
                        fullFilePath = Path.Combine(vsSelectItem.SolutionDirectory, itemName);
                    }
                }
                if (FileTools.FileExists(fullFilePath) || (typeGUID== VSConstants.GUID_ItemType_VirtualFolder))
                {
                    hierarchyKids = new List<VSITEMEX>();
                    ProcessHierarchy(vsSelectItem.pHier, ref hierarchyKids, vsSelectItem.ItemID);// firstChildID);
                    ProcessItems2(hierarchyKids, ref arrInfo, bIncludeKids, szProjectNames);
                    hierarchyKids.Clear();
                }
            }

            return arrInfo.ToArray();
        }
        public static bool IsFolder(Guid typeGUID, VSITEMEX vsSelectItem = null)
        {
            if (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder)
            {
                return true;
            }
            if (typeGUID == VSConstants.GUID_ItemType_VirtualFolder)
            {
                return true;
            }
            return false;
        }
        public List<VSITEMEX> ProcessHierarchy(IVsHierarchy hierarchy, ref List<VSITEMEX> hierList, uint uStartAt = VSConstants.VSITEMID_ROOT)
        {
            List<VSITEMEX> result = new List<VSITEMEX>();
            // Traverse the nodes of the hierarchy from the root node
            ProcessHierarchyNodeRecursively(hierarchy, uStartAt, ref hierList);
            return result;
        }

        public void ProcessHierarchyNodeRecursively(IVsHierarchy hierarchy, uint itemId, ref List<VSITEMEX> hierList)
        {
            int result;
            IntPtr nestedHiearchyValue = IntPtr.Zero;
            uint nestedItemIdValue = 0;
            object value = null;
            uint visibleChildNode;
            Guid nestedHierarchyGuid;
            IVsHierarchy nestedHierarchy;

            // First, guess if the node is actually the root of another hierarchy (a project, for example)
            nestedHierarchyGuid = typeof(IVsHierarchy).GUID;
            result = hierarchy.GetNestedHierarchy(itemId, ref nestedHierarchyGuid, out nestedHiearchyValue, out nestedItemIdValue);

            if (result == VSConstants.S_OK && nestedHiearchyValue != IntPtr.Zero && nestedItemIdValue == VSConstants.VSITEMID_ROOT)
            {
                // Get the new hierarchy
                nestedHierarchy = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(nestedHiearchyValue) as IVsHierarchy;
                System.Runtime.InteropServices.Marshal.Release(nestedHiearchyValue);

                if (nestedHierarchy != null)
                {
                    ProcessHierarchy(nestedHierarchy, ref hierList);
                }
            }
            else // The node is not the root of another hierarchy, it is a regular node
            {
                AddToHierarchyList(hierarchy, itemId, ref hierList);


                // Get the first visible child node
                result = hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out value);

                while (result == VSConstants.S_OK && value != null)
                {
                    if (value is int && (uint)(int)value == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    else
                    {
                        visibleChildNode = Convert.ToUInt32(value);

                        // Enter in recursion
                        ProcessHierarchyNodeRecursively(hierarchy, visibleChildNode, ref hierList);

                        // Get the next visible sibling node
                        value = null;
                        result = hierarchy.GetProperty(visibleChildNode, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out value);
                    }
                }
            }
        }
        public void AddToHierarchyList(IVsHierarchy hierarchy, uint itemId, ref List<VSITEMEX> hierList)
        {
            int result;
            object value = null;
            string name = "";
            string canonicalName = null;

            if (hierarchy != null)
            {
                result = hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Name, out value);

                if (result == VSConstants.S_OK && value != null)
                {
                    name = value.ToString();
                }

                result = hierarchy.GetCanonicalName(itemId, out canonicalName);
                if (result == VSConstants.S_OK)
                {

                }
                if (string.IsNullOrEmpty(canonicalName))
                {
                    return;
                }
                if (!FileTools.FileExists(canonicalName))
                {
                    return;
                }
                IVsProject vsProject = hierarchy as IVsProject;
                VSITEMEX itemex = null;
                if (vsProject != null)
                {
                    itemex = new VSPROJECTEX(vsProject, itemId, canonicalName,VSSolution);
                    
                }
                else
                {
                    itemex= new VSITEMEX(hierarchy, itemId, canonicalName,VSSolution);
                }
                if (itemex != null)
                {
                    hierList.Add(itemex);
                }
            }
        }
        protected ItemInfo[] ProcessItems2(IList<VSITEMEX> vsItems, ref List<ItemInfo> arrInfo, bool bIncludeKids, string[] szProjectNames, bool bProcessPhysicalDir = true)
        {
            foreach (VSITEMEX vsSelectItem in vsItems)
            {
                Guid typeGUID = Guid.Empty;
                Guid projectGUID = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullFilePath = string.Empty;
                VSPROJECTEX vsProject = vsSelectItem as VSPROJECTEX;
                IList<VSITEMEX> projectItems = new List<VSITEMEX>();
                bool isProjectFile = false;
                string projectRef = string.Empty;
                uint firstChildID = VSConstants.VSITEMID_NIL;
                int iRes;
                bool isNonMember = false;
                List<VSITEMEX> hierarchyKids = new List<VSITEMEX>();

                fullFilePath = vsSelectItem.FullFilePath;

                itemName = vsSelectItem.GetItemName();
                typeGUID = vsSelectItem.GetItemType();
                isProjectFile = vsSelectItem.IsProjectFile(szProjectNames);

                if (isProjectFile && (vsProject != null))
                {
                    //projectItems = ListProjectItems(vsProject.vsProject, vsSelectItem.ItemID);
                }
                iRes = vsSelectItem.IsNonMember(ref isNonMember);
                if (!ErrorHandler.Succeeded(iRes))
                {
                    //isNonMember = true;
                }
                if (vsSelectItem.IsRootSolution())
                {
                    isNonMember = false;
                }

                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";

                firstChildID = vsSelectItem.GetFirstChild();

                if (IsFolder(typeGUID))
                {
                    fullFilePath = FileTools.NormalizeDirectoryPath(fullFilePath);
                }
                if (FileTools.FileExists(fullFilePath))
                {
                    FileTools.ConvertToCaseSensitive(fullFilePath, ref fullFilePath);

                    if (!ItemInfo.Exists(arrInfo, fullFilePath))
                    {
                        ItemInfo pInfo = new ItemInfo(itemName, fullFilePath, typeGUID);
                        pInfo.IncludedInProject = !isNonMember;
                        if (typeGUID == VSConstants.GUID_ItemType_PhysicalFile)
                        {
                            if(vsSelectItem.isItemSelected)
                            {
                                pInfo.IncludedInProject = true;
                            }
                        }

                        arrInfo.Add(pInfo);
                    }
                    bool bProcessDir = false;
                    bool bRecurse = false;
                    if (bIncludeKids)
                    {
                        if (isProjectFile)
                        {
                            bProcessDir = true;
                        }
                        if (IsFolder(typeGUID))
                        {
                            bProcessDir = true;
                            bRecurse = true;
                        }
                        if (vsSelectItem.IsRootSolution())
                        {
                            bProcessDir = true;
                            bRecurse = false;
                        }
                    }
                    bProcessDir = false;
                    if (bProcessDir)
                    {
                        string barePath = Path.GetDirectoryName(fullFilePath);
                        barePath = FileTools.NormalizeDirectoryPath(barePath);
                        ProcessDirectory(itemName, barePath, arrInfo, vsSelectItem, (isNonMember ? false : true), bRecurse);
                    }
                }
            }
            return arrInfo.ToArray();
        }

        protected ItemInfo[] ProcessItems(IList<VSITEMEX> vsSelectItems, ref List<ItemInfo> arrInfo, bool bIncludeKids, string[] szProjectNames, bool bProcessPhysicalDir=true)
        {
            foreach (VSITEMEX vsSelectItem in vsSelectItems)
            {
                Guid typeGUID = Guid.Empty;
                Guid projectGUID = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullFilePath = string.Empty;
                VSPROJECTEX vsProject = vsSelectItem as VSPROJECTEX;
                IList<VSITEMEX> projectItems = new List<VSITEMEX>();
                bool isProjectFile = false;
                string projectRef = string.Empty;
                uint firstChildID = VSConstants.VSITEMID_NIL;
                int iRes;
                bool isNonMember = false;
                List<VSITEMEX> hierarchyKids = new List<VSITEMEX>();

                fullFilePath = vsSelectItem.FullFilePath;

                itemName = vsSelectItem.GetItemName();
                typeGUID = vsSelectItem.GetItemType();
                isProjectFile = vsSelectItem.IsProjectFile(szProjectNames);

                if(vsSelectItem.isItemSelected || vsSelectItem.isParentSelected)
                {

                }
                if (isProjectFile && (vsProject!=null))
                {
                    //projectItems = ListProjectItems(vsProject.vsProject, vsSelectItem.ItemID);
                }

                iRes = vsSelectItem.IsNonMember(ref isNonMember);
                if (!ErrorHandler.Succeeded(iRes))
                {
                    isNonMember = true;
                }
                if (vsSelectItem.IsRootSolution())
                {
                    isNonMember = false;
                }

                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";

                firstChildID = vsSelectItem.GetFirstChild();

                if (IsFolder(typeGUID))
                {
                    fullFilePath = FileTools.NormalizeDirectoryPath(fullFilePath);
                }
                if (FileTools.FileExists(fullFilePath))
                {
                    FileTools.ConvertToCaseSensitive(fullFilePath, ref fullFilePath);

                    if (!ItemInfo.Exists(arrInfo, fullFilePath))
                    {
                        ItemInfo pInfo = new ItemInfo(itemName, fullFilePath, typeGUID);
                        pInfo.IncludedInProject = !isNonMember;
                        arrInfo.Add(pInfo);
                    }
                    bool bProcessDir = false;
                    bool bRecurse = false;
                    if (bIncludeKids)
                    {
                        if (isProjectFile)
                        {
                            bProcessDir = true;
                        }
                        if (IsFolder(typeGUID))
                        {
                            bProcessDir = true;
                            bRecurse = true;
                        }
                        if (vsSelectItem.IsRootSolution())
                        {
                            bProcessDir = true;
                        }
                    }
                    if (bProcessDir)
                    {
                        string barePath = Path.GetDirectoryName(fullFilePath);
                        barePath = FileTools.NormalizeDirectoryPath(barePath);
                        ProcessDirectory(itemName, barePath, arrInfo, vsSelectItem, (isNonMember ? false : true), bRecurse);
                    }
                }
            }
            return arrInfo.ToArray();
        }

    }
}

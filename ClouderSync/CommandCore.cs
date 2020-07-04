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

namespace ClouderSync
{
    public class CommandCore : BaseCommand
    {
        protected static IVsSolution _vsSolution=null;

        protected CommandCore(ClouderSyncPackage package, OleMenuCommandService commandService, Guid commandSet,int commandId = _commandId)
            : base(package, commandService, commandSet,commandId)
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
                if(_vsSolution==null)
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
        private List<IVsProject> ListSolutionProjects(IVsSolution vsSolution, bool bExcludeNonPhysical=true)
        {
            List<IVsProject> result = new List<IVsProject>();
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
                result.Add(project);
            }
            return result;
        }
        /// <summary>
        /// Enumerates files in the solution projects.
        /// </summary>
        /// <returns>File names collection.</returns>
        private List<VSITEMSELECTION> ListProjectsAsVSITEMSELECTIONs(IList<IVsProject> projectList)
        {
            List<VSITEMSELECTION> result = new List<VSITEMSELECTION>();
            foreach (IVsProject project in projectList)
            {
                foreach (VSITEMSELECTION vsSelItem in EnumerateProjectItems(project))
                {
                    result.Add(vsSelItem);
                }
            
            }
            return result;
        }

        internal static IEnumerable<VSITEMSELECTION> EnumerateProjectItems(IVsProject project)
        {
            var enumHierarchyItemsFactory = ClouderSyncPackage.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;
            var hierarchy = (IVsHierarchy)project;
            if (enumHierarchyItemsFactory != null && project != null)
            {
                IEnumHierarchyItems enumHierarchyItems;
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest),
                        (uint)VSConstants.VSITEMID_ROOT,
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
            vsSolution.GetProjectEnum(flags,ref guid,out hierarchies);
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
        public IList<VSITEMSELECTION> GetSolutionExplorerHierarchy(ref IVsHierarchy rootHierarchy)
        {
            List<VSITEMSELECTION> results = new List<VSITEMSELECTION>();
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
                if(!ErrorHandler.Succeeded(solutionExplorerFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out vshw)))
                {
                    return results;
                }
            }
            vsUIHierarchyWindow = vshw as IVsUIHierarchyWindow;
            if(vsUIHierarchyWindow==null)
            {
                return results;
            }
            vsUIHierarchyWindow.GetCurrentSelection(out hierarchyPtr, out itemID, out ppMIS);
            if (hierarchyPtr != IntPtr.Zero)
            {
                IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                rootHierarchy = hierarchy;
                results.Add(new VSITEMSELECTION() { itemid = itemID, pHier = hierarchy });
            }
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            return results;
        }
        public IList<VSITEMSELECTION> GetSelectedHierarchyItems(IVsMonitorSelection selectionMonitor)
        {
            IList<VSITEMSELECTION> results = new List<VSITEMSELECTION>();

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
            /*
            if (itemID == (uint)VSConstants.VSITEMID.Root)
            {
                rootResults = GetSolutionExplorerHierarchy(ref rootHierarchy);    
            }
            */
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
                   // IVsProject project = GetProjectOfItem(item.pHier, item.itemid);
                    if (!results.Contains(item))
                    {
                        results.Add(item);
                    }
                }
            }
            else
            {
                // case where no visible project is open (single file)
                if (hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                    results.Add(new VSITEMSELECTION() { itemid = itemID, pHier = hierarchy });
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
            if(!ErrorHandler.Succeeded(hier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out name)))
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

        private string GetProjectDir(IVsHierarchy hierarchy, uint itemId)
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
            if(itemId== (uint)VSConstants.VSITEMID.Root)
            {
                return GetSolutionDirectory();
            }
            Debug.Write("Failed to get ProjectDir.");
            return string.Empty;
        }
        private static string GetItemProperty(IVsHierarchy hierarchy, uint itemId,int VSPROPID)
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
        protected string GetSelectedItemDirectory(Array arrInfo,string title = "cmdCommandWindow")
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
        protected string GetSelectedItemDirectory(string title = "cmdCommandWindow")
        {
            string selectedDir = string.Empty;
            Array arrInfo = GetSelectedItemArray(title);
            selectedDir = GetSelectedItemDirectory(arrInfo, title);
            if (string.IsNullOrEmpty(selectedDir))
            {
                selectedDir = GetProjectRoot();
            }
            if (string.IsNullOrEmpty(selectedDir))
            {
                selectedDir = GetSolutionDirectory();
            }
            return selectedDir;

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
            if(string.IsNullOrEmpty(pathIn))
            {
                return false;
            }
            //pathOut = pathIn;
            try
            {
                pathOut = ToCaseSensitive(pathIn);
            }
            catch(Exception eConvert)
            {
                pathOut = pathIn;
                Debug.Write(eConvert.Message);
                return false;
            }
            return true;
        }
        //since 1.9.7
        public string GetTopmostRoot(bool bExcludeSolution=false)
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

            IList<VSITEMSELECTION> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            if(vsSelectItems.ToArray().Length<1)
            {
                return string.Empty;
            }
            foreach (VSITEMSELECTION vsSelectItem in vsSelectItems)
            {
                Guid itemType = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string fullPath = string.Empty;
                

                itemName = GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);

                vsSelectItem.pHier.GetCanonicalName(vsSelectItem.itemid, out canonicalName);
                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);
                if(string.IsNullOrEmpty(projectDirectory))
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
        public static bool GetBoolProperty(IVsHierarchy vshItem, uint itemid, int propID, bool bDefault=false)
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
        public static void ProcessDirectory(string itemName, string targetDirectory, List<ItemInfo> arrInfo, IVsHierarchy vshParent,bool bIncludedInProject=true)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                uint itemID = 0;
                string canonicalName = Path.Combine(targetDirectory, fileName);
                bool isNonMember = bIncludedInProject;

                if (vshParent != null)
                {
                    if (vshParent.ParseCanonicalName(canonicalName, out itemID) == VSConstants.S_OK)
                    {
                        isNonMember = GetBoolProperty(vshParent, itemID, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem);
                    }
                }


                string fileAlone = Path.GetFileName(fileName);
                ItemInfo pKidInfo = new ItemInfo(fileAlone, canonicalName, VSConstants.GUID_ItemType_PhysicalFile);
                pKidInfo.IncludedInProject = (isNonMember?false:true);
                arrInfo.Add(pKidInfo);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(itemName, subdirectory,arrInfo, vshParent, bIncludedInProject);
            }
        }
        public string GetSolutionDirectory()
        {
            string solutionDirectory = string.Empty;
            string solutionOptFile = string.Empty;

            if (!(VSSolution is IVsSolution))
            {
                return string.Empty;
            }
            VSSolution.GetSolutionInfo(out solutionDirectory, out string pbstrSolutionFile, out solutionOptFile);
            return solutionDirectory;
        }
        public ItemInfo[] GetSelectedItems(bool bIncludeKids=false)
        {
            List<ItemInfo> arrInfo = new List<ItemInfo>();
            IVsMonitorSelection monitorSelection;
            IVsHierarchy vsRootHierarchy = null;
            Guid typeGUID = Guid.Empty;

            monitorSelection = _package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            IList<VSITEMSELECTION> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            IList<VSITEMSELECTION> vsRootSelectItems = GetSolutionExplorerHierarchy(ref vsRootHierarchy);
            IList<VSITEMSELECTION>  rootSelections = new List<VSITEMSELECTION>();

            string solutionDirectory = GetSolutionDirectory();
            /*
            if(vsRootHierarchy!=null)
            {
                vsRootHierarchy.SetSite(Package.ServiceProvider);
            }
            */
            if(vsRootSelectItems.Count()>0)
            {
                vsRootSelectItems[0].pHier.GetGuidProperty(vsRootSelectItems[0].itemid, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGUID);
                if(typeGUID==Guid.Empty)
                {
                    //this is ROOT SOLUTION!
                    List<IVsProject> arrProjects = ListSolutionProjects(VSSolution);
                    IVsProject vsProject= arrProjects[0];
                    rootSelections=ListProjectsAsVSITEMSELECTIONs(arrProjects);
                    vsSelectItems = rootSelections;
                }

            }
            foreach (VSITEMSELECTION vsSelectItem in vsSelectItems)
            {
                typeGUID= Guid.Empty;
                IVsHierarchy vshSelContainer = null;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullPath = string.Empty;

                //EnvDTE.Constants.vsProjectItemKindPhysicalFolder
                //vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";

                vsSelectItem.pHier.GetGuidProperty(vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGUID);

                itemName = GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);

                vsSelectItem.pHier.GetCanonicalName(vsSelectItem.itemid,out canonicalName);
                ConvertToCaseSensitive(canonicalName, ref canonicalName);

                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);

                ConvertToCaseSensitive(projectDirectory, ref projectDirectory);

                string projName = GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_ProjectName);
                string Name= GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_Name);

                bool isProjectFile = false;
                bool isNonMember=false;


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
                vshSelContainer = GetSelectionContainer(vsSelectItem.pHier, vsSelectItem.itemid);

                isNonMember = GetBoolProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem);
                if(!isNonMember)
                {
                    isNonMember= GetBoolProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_IsHiddenItem);
                }
                if (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder)
                {
                    fullPath = FileTools.NormalizeDirectoryPath(fullPath);
                }
                if (FileTools.FileExists(fullPath) && (itemName != string.Empty) && (typeGUID != Guid.Empty))
                {
                    ItemInfo pInfo = new ItemInfo(itemName, fullPath, typeGUID);
                    arrInfo.Add(pInfo);

                    if (bIncludeKids && (typeGUID == VSConstants.GUID_ItemType_PhysicalFolder) || isProjectFile)
                    {
                        string barePath = Path.GetDirectoryName(fullPath);
                        barePath=FileTools.NormalizeDirectoryPath(barePath);
                        ProcessDirectory(itemName, barePath, arrInfo, vsSelectItem.pHier,(isNonMember?false:true));
                    }
                }
            }
            return arrInfo.ToArray();
        }
    }
}

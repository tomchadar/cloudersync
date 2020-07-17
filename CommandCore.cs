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


namespace CloudSync
{
    public class CommandCore
    {
        private static readonly char[] DirectorySeparators = new[] {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        protected /*readonly*/ CloudSyncPackage package;

        protected CommandCore(CloudSyncPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;
//            ServiceProvider = package;
            
        }
#if SYNC
        /*
        public static async System.Threading.Tasks.Task InitializeAsync(CloudSyncPackage package)
        {
            commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        }
        */
        public static OleMenuCommandService commandService
        {
            get;
            protected set;
        }

        public Guid CommandSet
        {
            get { return CloudSyncIDs.guidCloudSyncCmdSet; }
        }
        public IServiceProvider ServiceProvider
        {
            get;
            set;
        }
        protected static void InitCommand(CloudSyncPackage package,Guid CommandSet, int CommandId, EventHandler cbMenuItem)
        {
            commandService = package.GetMenuService();
            //commandService = package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(cbMenuItem, menuCommandID);
                commandService.AddCommand(menuItem);
            }

        }
#endif
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

            //p.StandardInput.WriteLine(@"pdftoxml.win32.1.2.7 -annotation " + filename);

            //p.StandardInput.WriteLine(@"cd D:\python-source\ds-xmlStudio-1.0-py27");

            //p.StandardInput.WriteLine(@"main.py -i example-8.xml -o outp.xml");

            //p.WaitForExit();

        }
        protected void ShowMessageBox(string message, string title = "Message")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
#if false
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
#endif

        }
#if false
        protected EnvDTE80.DTE2 GetDTE2()
        {
            var componentModel = this.package.GetServiceOfType(typeof(SComponentModel)) as IComponentModel;
            var newManager = componentModel != null ? componentModel.GetService<IVsHierarchyItemManager>() : null;

            EnvDTE80.DTE2 dte = null;
            //dte = this.package.GetServiceOfType(typeof(SDTE)) as EnvDTE80.DTE2;
            dte = this.package.GetServiceOfType(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            //EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            // EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(SDTE)) as EnvDTE80.DTE2;
            if (dte != null)
            {
                return dte;
            }
            dte = this.package.GetServiceOfType(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            if (dte != null)
            {
                return dte;
            }
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }
        protected ItemInfo GetItemInfo(UIHierarchyItem selItem)
        {
            ItemInfo pInfo = new ItemInfo(selItem);

            Type objType = selItem.Object.GetType();
            string szType = selItem.Object.ToString();
            TypeInfo objTI=objType.GetTypeInfo();
            string szGUID = objTI.GUID.ToString();
            
            //if(projItem!=null)
            {
                if (objTI.GUID.ToString() == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    //string strProjPath = projItem.ContainingProject.Properties.Item("FullPath").Value.ToString();
                    //pInfo.FilePath = strProjPath + "/" + selItem.Name;
                    return pInfo;
                }
            }
            if (selItem.Object is EnvDTE.Project)
            {
                Project objProject = selItem.Object as Project;
                pInfo.FilePath = objProject.FullName;

                //pInfo.FilePath = objProject.Properties.Item("FullPath").Value.ToString();
                return pInfo;
            }
            if (selItem.Object is EnvDTE.ProjectItem)
            {
                ProjectItem objProjectItem = selItem.Object as ProjectItem;
                pInfo.FilePath = objProjectItem.Properties.Item("FullPath").Value.ToString();
                return pInfo;
            }
            return pInfo;
        }
        protected ItemInfo GetItemInfo(SelectedItem selItem)
        {
            ItemInfo pInfo = new ItemInfo(selItem);
            ProjectItem projItem = selItem.ProjectItem;
            SelectedItems objItems = selItem.Collection;
            //objItems.Parent;

            if(projItem!=null)
            {
                //if (projItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    //string strProjPath = projItem.ContainingProject.Properties.Item("FullPath").Value.ToString();
                    //pInfo.FilePath = strProjPath + "/" + selItem.Name;
                    pInfo.FilePath = projItem.Properties.Item("FullPath").Value.ToString();
                    return pInfo;
                }
            }
            return pInfo;
        }
#endif
        public IEnumerable<IVsProject> GetSolutionProjects(IVsSolution vsSolution)
        {
            if (vsSolution == null)
            {
                Debug.Fail("Failed to get SVsSolution service.");
                yield break;
            }

            IEnumHierarchies enumerator = null;
            Guid guid = Guid.Empty;
            vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out enumerator);
            IVsHierarchy[] hierarchy = new IVsHierarchy[1] { null };
            uint fetched = 0;
            for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; /*nothing*/)
            {
                yield return (IVsProject)hierarchy[0];
            }
        }
        protected IVsProject GetProjectOfItem(IVsHierarchy hierarchy, uint itemID)
        {
            return (IVsProject)hierarchy;
        }
        public IList<VSITEMSELECTION> GetSelectedHierarchyItems(IVsMonitorSelection selectionMonitor)
        {
            List<VSITEMSELECTION> results = new List<VSITEMSELECTION>();

            int hr = VSConstants.S_OK;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;
            IntPtr containerPtr = IntPtr.Zero;
            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");

            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                uint itemCount = 0;
                int fSingleHierarchy = 0;
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

            return results;
        }

        /// <summary>
        /// Gets all of the currently selected items.
        /// </summary>
        /// <returns></returns>
        public IList<VSITEMSELECTION> GetSelectedHierarchyItems1(IVsMonitorSelection monitorSelection)
        {
            List<VSITEMSELECTION> results = new List<VSITEMSELECTION>(); 

            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainer = IntPtr.Zero;
            try
            {
                uint selectionItemId;
                IVsMultiItemSelect multiItemSelect = null;
                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out selectionItemId, out multiItemSelect, out selectionContainer));

                if (selectionItemId != VSConstants.VSITEMID_NIL && hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;

                    if (selectionItemId != VSConstants.VSITEMID_SELECTION)
                    {
                        // This is a single selection. Compare hirarchy with our hierarchy and get node from itemid
                        if (IsSameComObject(this, hierarchy))
                        {
                            results.Add(new VSITEMSELECTION() { itemid = selectionItemId, pHier = hierarchy });
                            return results;
//                            yield return new VSITEMSELECTION() { itemid = selectionItemId, pHier = hierarchy };
                        }
                    }
                    else if (multiItemSelect != null)
                    {
                        // This is a multiple item selection.
                        // Get number of items selected and also determine if the items are located in more than one hierarchy

                        uint numberOfSelectedItems;
                        int isSingleHierarchyInt;
                        ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out numberOfSelectedItems, out isSingleHierarchyInt));
                        bool isSingleHierarchy = (isSingleHierarchyInt != 0);

                        // Now loop all selected items and add to the list only those that are selected within this hierarchy
                        if (!isSingleHierarchy || (isSingleHierarchy && IsSameComObject(this, hierarchy)))
                        {
                            Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected itemd");
                            VSITEMSELECTION[] vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                            uint flags = (isSingleHierarchy) ? (uint)__VSGSIFLAGS.GSI_fOmitHierPtrs : 0;
                            ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(flags, numberOfSelectedItems, vsItemSelections));

                            foreach (VSITEMSELECTION vsItemSelection in vsItemSelections)
                            {
                                results.Add(new VSITEMSELECTION() { itemid = vsItemSelection.itemid, pHier = hierarchy });
                                //                 yield return new VSITEMSELECTION() { itemid = vsItemSelection.itemid, pHier = hierarchy };
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }
            return results;
        }
        public IList<IVsProject> GetProjectsOfCurrentSelections(IVsMonitorSelection selectionMonitor)
        {
            List<IVsProject> results = new List<IVsProject>();

            int hr = VSConstants.S_OK;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;
            IntPtr containerPtr = IntPtr.Zero;
            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");

            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                uint itemCount = 0;
                int fSingleHierarchy = 0;
                hr = multiSelect.GetSelectionInfo(out itemCount, out fSingleHierarchy);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectionInfo failed.");

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                hr = multiSelect.GetSelectedItems(0, itemCount, items);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectedItems failed.");

                foreach (VSITEMSELECTION item in items)
                {
                    IVsProject project = GetProjectOfItem(item.pHier, item.itemid);
                    if (!results.Contains(project))
                    {
                        results.Add(project);
                    }
                }
            }
            else
            {
                // case where no visible project is open (single file)
                if (hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                    results.Add(GetProjectOfItem(hierarchy, itemID));
                }
            }

            return results;
        }
        /// <summary>
        /// Gets the item id.
        /// </summary>
        /// <param name="variantValue">VARIANT holding an itemid.</param>
        /// <returns>Item Id of the concerned node</returns>
        private static uint GetItemId(object variantValue)
        {
            if (variantValue == null)
            {
                return VSConstants.VSITEMID_NIL;
            }
            if (variantValue is int)
            {
                return (uint)(int)variantValue;
            }
            if (variantValue is uint)
            {
                return (uint)variantValue;
            }
            if (variantValue is short)
            {
                return (uint)(short)variantValue;
            }
            if (variantValue is ushort)
            {
                return (uint)(ushort)variantValue;
            }
            if (variantValue is long)
            {
                return (uint)(long)variantValue;
            }
            return VSConstants.VSITEMID_NIL;
        }
        private bool TryGetItemId(object obj, out uint id)
        {
            const uint nil = (uint)VSConstants.VSITEMID.Nil;
            id = obj as uint? ?? nil;
            if (id == nil)
            {
                var asInt = obj as int?;
                if (asInt.HasValue)
                {
                    id = unchecked((uint)asInt.Value);
                }
            }
            return id != nil;
        }
        /// <summary>
        /// Verifies that two objects represent the same instance of a COM object.
        /// This essentially compares the IUnkown pointers of the 2 objects.
        /// This is needed in scenario where aggregation is involved.
        /// </summary>
        /// <param name="obj1">Can be an object, interface or IntPtr</param>
        /// <param name="obj2">Can be an object, interface or IntPtr</param>
        /// <returns>True if the 2 items represent the same thing</returns>
        public static bool IsSameComObject(object obj1, object obj2)
        {
            bool isSame = false;
            IntPtr unknown1 = IntPtr.Zero;
            IntPtr unknown2 = IntPtr.Zero;
            try
            {
                // If we have 2 null, then they are not COM objects and as such "it's not the same COM object"
                if (obj1 != null && obj2 != null)
                {
                    unknown1 = QueryInterfaceIUnknown(obj1);
                    unknown2 = QueryInterfaceIUnknown(obj2);

                    isSame = IntPtr.Equals(unknown1, unknown2);
                }
            }
            finally
            {
                if (unknown1 != IntPtr.Zero)
                {
                    Marshal.Release(unknown1);
                }

                if (unknown2 != IntPtr.Zero)
                {
                    Marshal.Release(unknown2);
                }

            }

            return isSame;
        }
        /// <summary>
        /// Retrieve the IUnknown for the managed or COM object passed in.
        /// </summary>
        /// <param name="objToQuery">Managed or COM object.</param>
        /// <returns>Pointer to the IUnknown interface of the object.</returns>
        internal static IntPtr QueryInterfaceIUnknown(object objToQuery)
        {
            bool releaseIt = false;
            IntPtr unknown = IntPtr.Zero;
            IntPtr result;
            try
            {
                if (objToQuery is IntPtr)
                {
                    unknown = (IntPtr)objToQuery;
                }
                else
                {
                    // This is a managed object (or RCW)
                    unknown = Marshal.GetIUnknownForObject(objToQuery);
                    releaseIt = true;
                }

                // We might already have an IUnknown, but if this is an aggregated
                // object, it may not be THE IUnknown until we QI for it.
                Guid IID_IUnknown = VSConstants.IID_IUnknown;
                ErrorHandler.ThrowOnFailure(Marshal.QueryInterface(unknown, ref IID_IUnknown, out result));
            }
            finally
            {
                if (releaseIt && unknown != IntPtr.Zero)
                {
                    Marshal.Release(unknown);
                }

            }

            return result;
        }
        internal static Uri MakeUri(string path, bool isDirectory, UriKind kind, string throwParameterName = "path")
        {
            try
            {
                if (isDirectory && !string.IsNullOrEmpty(path) && !HasEndSeparator(path))
                {
                    path += Path.DirectorySeparatorChar;
                }

                return new Uri(path, kind);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
        }
        /// <summary>
        /// Returns true if the path has a directory separator character at the end.
        /// </summary>
        public static bool HasEndSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[path.Length - 1]);
        }

        /// <summary>
        /// Normalizes and returns the provided directory path, always
        /// ending with '/'.
        /// </summary>
        public static string NormalizeDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var uri = MakeUri(path, true, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                if (uri.IsFile)
                {
                    return uri.LocalPath;
                }
                else
                {
                    return uri.AbsoluteUri.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }
            }
            else
            {
                return Uri.UnescapeDataString(uri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        internal static string GetItemName(IVsHierarchy hier, uint itemid)
        {
            object name;
            ErrorHandler.ThrowOnFailure(hier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out name));
            return (string)name;
        }
        private static object GetExtensionObject(IVsHierarchy hierarchy, uint itemId)
        {
            object project;

            hierarchy.GetProperty(
                itemId,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out project
            );
            
            if (project == null)
            {
                Debug.Fail("Failed to get ProjectItem.");
                return null;
            }
            return project;
        }
        private static IVsHierarchy GetParentHierarchy(IVsHierarchy hierarchy, uint itemId)
        {
            object parent;

            hierarchy.GetProperty(
                itemId,
                (int)__VSHPROPID.VSHPROPID_ParentHierarchy,
                out parent
            );

            if (parent is IVsHierarchy)
            {
                return (parent as IVsHierarchy);
            }
            Debug.Fail("Failed to get ParentHierarchy.");
            return null;
        }
        private static string GetProjectDir(IVsHierarchy hierarchy, uint itemId)
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
            Debug.Fail("Failed to get ProjectDir.");
            return string.Empty;
        }
        private static string GetItemProperty(IVsHierarchy hierarchy, uint itemId,int VSPROPID){
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

        internal static bool FileExists(string name)
        {
            if(File.Exists(name))
            {
                return true;
            }
            return (Directory.Exists(name));
        }
        protected Array GetSelectedItemArray(string title = "cmdCommandWindow")
        {
            Array arrInfo = GetSelectedItems();
            if (arrInfo != null)
            {
                if (arrInfo.Length < 1)
                {
                    ShowMessageBox("No items selected", title);
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
        public string GetProjectRoot()
        {
            List<ItemInfo> arrInfo = new List<ItemInfo>();
            string projectDirectory = string.Empty;

            IVsMonitorSelection monitorSelection;
            monitorSelection = package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            IList<VSITEMSELECTION> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            foreach (VSITEMSELECTION vsSelectItem in vsSelectItems)
            {
                Guid itemType = Guid.Empty;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string fullPath = string.Empty;

                itemName = GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);

                vsSelectItem.pHier.GetCanonicalName(vsSelectItem.itemid, out canonicalName);
                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);
            }
            return projectDirectory;
        }
        public static void ProcessDirectory(string itemName, string targetDirectory, List<ItemInfo> arrInfo)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ItemInfo pKidInfo = new ItemInfo(itemName, Path.Combine(targetDirectory, fileName), VSConstants.GUID_ItemType_PhysicalFile);
                arrInfo.Add(pKidInfo);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(itemName, subdirectory,arrInfo);
            }
        }
        public ItemInfo[] GetSelectedItems(bool bIncludeKids=false)
        {
            List<ItemInfo> arrInfo = new List<ItemInfo>();
/*
            IVsSolution vsSolution;
            IVsHierarchy vsHierarchy;
            IVsMonitorSelection selectionMonitor;
            */
            IVsMonitorSelection monitorSelection;
            monitorSelection = package.GetServiceOfType(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            IList<VSITEMSELECTION> vsSelectItems = GetSelectedHierarchyItems(monitorSelection);
            foreach (VSITEMSELECTION vsSelectItem in vsSelectItems)
            {
                Guid itemType= Guid.Empty;
                IVsHierarchy vshParent=null;
                string itemName = string.Empty;
                string canonicalName = string.Empty;
                string projectDirectory = string.Empty;
                string fullPath = string.Empty;

                itemName= GetItemName(vsSelectItem.pHier, vsSelectItem.itemid);

                vsSelectItem.pHier.GetCanonicalName(vsSelectItem.itemid,out canonicalName);
                projectDirectory = GetProjectDir(vsSelectItem.pHier, vsSelectItem.itemid);
                string projName = GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_ProjectName);
                string Name= GetItemProperty(vsSelectItem.pHier, vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_Name);
                bool isProjectFile = false;


                if (FileExists(canonicalName))
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
                vsSelectItem.pHier.GetGuidProperty(vsSelectItem.itemid, (int)__VSHPROPID.VSHPROPID_TypeGuid, out itemType);
                vshParent=GetParentHierarchy(vsSelectItem.pHier, vsSelectItem.itemid);
                
                if (itemType == VSConstants.GUID_ItemType_PhysicalFolder)
                {
                    fullPath = NormalizeDirectoryPath(fullPath);
                }
                //Path.GetDirectoryName(this.buildProject.FullPath)
                //if (itemType == VSConstants.GUID_ItemType_PhysicalFile || itemType == VSConstants.GUID_ItemType_PhysicalFolder)
                if (FileExists(fullPath) && (itemName != string.Empty) && (itemType != Guid.Empty))
                {
                    ItemInfo pInfo = new ItemInfo(itemName, fullPath, itemType);
                    arrInfo.Add(pInfo);

                    if (bIncludeKids && (itemType == VSConstants.GUID_ItemType_PhysicalFolder) || isProjectFile)
                    {
                        string barePath = Path.GetDirectoryName(fullPath);
                        barePath=NormalizeDirectoryPath(barePath);
                        ProcessDirectory(itemName, barePath, arrInfo);
                        // Process the list of files found in the directory.
                        /*
                        string[] fileEntries = Directory.GetFiles(fullPath);
                        foreach (string fileName in fileEntries)
                        {
                            ItemInfo pKidInfo = new ItemInfo(itemName, Path.Combine(fullPath,fileName), VSConstants.GUID_ItemType_PhysicalFile);
                            arrInfo.Add(pKidInfo);
                        }
                        */
                    }
                }
/*
                vsSelectItem.pHier.GetProperty(
                        vsSelectItem.itemid,
                        (int)__VSHPROPID.VSHPROPID_TypeGuid,
                        out objGuid);

                if (objGuid != null)
                {
                    Guid guid = (Guid)objGuid;
                    Kind = guid.ToString("B");
                    if (Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                    {
                        objGuid = null;
                    }
                }
*/
//pInfo.FilePath = objProject.Properties.Item("FullPath").Value.ToString();
//EnvDTE.ProjectItem dteProjectItem = GetExtensionObject(vsSelectItem.pHier, vsSelectItem.itemid);
//ErrorHandler.ThrowOnFailure(OpenWithNodejsEditor(vsItemSelection.itemid));
                }
            return arrInfo.ToArray();
            // = _package.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
#if false
            vsSolution = this.package.GetServiceOfType(typeof(SVsSolution)) as IVsSolution;

            selectionMonitor = this.package.GetServiceOfType(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            IList<IVsProject> arrProjects=GetProjectsOfCurrentSelections(selectionMonitor);

            IEnumerable<IVsProject> vsProjects = GetSolutionProjects(vsSolution);

            foreach(IVsProject vsProject in vsProjects)
            {
                vsHierarchy = (IVsHierarchy)vsProject;
                
                if(vsHierarchy!=null)
                {
                    object objFirstChildID;
                    vsHierarchy.GetProperty(
                        (uint)VSConstants.VSITEMID.Root,
                        (int)__VSHPROPID.VSHPROPID_FirstChild,
                        out objFirstChildID);

                    uint itemId = GetItemId(objFirstChildID);

                    while (TryGetItemId(objFirstChildID, out itemId))
                    {
                        Guid itemType;
                       // string mkDoc;

                        if (ErrorHandler.Succeeded(vsHierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out itemType)))
                        {
                            if(itemType != VSConstants.GUID_ItemType_PhysicalFile)
                            {
                                break;
                            }
                        }
                    }
                    object objExtObject;
                    vsHierarchy.GetProperty(itemId,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out objExtObject);

                    object objGuid;
                    string Kind;
                    vsHierarchy.GetProperty(
                            itemId,
                            (int)__VSHPROPID.VSHPROPID_TypeGuid,
                            out objGuid);

                    if (objGuid != null)
                    {
                        Guid guid = (Guid)objGuid;
                        Kind = guid.ToString("B");
                        if (Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                        {
                            objGuid = null;
                        }
                    }
                }
            }
                /*
                IEnumHierarchies enumHierarchies = null;
                Guid guidProject = Guid.Empty;
                vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, ref guidProject, out enumHierarchies);
                */

                DTE dte = (DTE)this.package.GetServiceOfType(typeof(DTE)) as DTE;

            IComponentModel componentModel = this.package.GetServiceOfType(typeof(SComponentModel)) as IComponentModel;

            if (false)//componentModel != null)
            {
                //vsSolution = componentModel.GetService<IVsSolution>();
                //vsHierarchy = componentModel.GetService<IVsHierarchy>();


                //vsHierarchy = this.package.GetServiceOfType(typeof(IVsHierarchy)) as IVsHierarchy;
                object objProjectCount = null;
                vsSolution.GetProperty((int)__VSPROPID.VSPROPID_ProjectCount, out objProjectCount);

                //vsHierarchy = objHierarchy as IVsHierarchy;
                //object objProjectDir = null;
                //vsHierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectDir,out objProjectDir);

                object extObject;
                ErrorHandler.ThrowOnFailure(
                    vsHierarchy.GetProperty(
                        VSConstants.VSITEMID_ROOT,
                        (int)__VSHPROPID.VSHPROPID_ExtObject,
                        out extObject
                    )
                );
                object objGuid;
                string Kind;
                ErrorHandler.ThrowOnFailure(
                    vsHierarchy.GetProperty(
                        VSConstants.VSITEMID_ROOT,
                        (int)__VSHPROPID.VSHPROPID_TypeGuid, 
                        out objGuid)
                );
                Guid guid = (Guid)objGuid;
                Kind=guid.ToString("B");
                if(Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    objGuid = null;
                }
            }
            return arrInfo.ToArray();
            //vsSolution = (IVsSolution)this.package.GetServiceOfType(typeof(SVsSolution));
            //vsHierarchy = vsSolution.GetProjectOfUniqueName(project.UniqueName);

            Projects projects = dte.Solution.Projects;
            
            
            foreach (Project project in projects)
            {
                
            }

            EnvDTE80.DTE2 dte2 = GetDTE2();
            if (dte2 == null)
            {
                return arrInfo.ToArray();
            }
            UIHierarchy uih = null;
            //UIHierarchy uih = dte2.ToolWindows.SolutionExplorer;
            //Solution objSolution=dte2.Solution;
            
            //UIHierarchyItems objItems=uih.UIHierarchyItems;
            //Projects objProjects = objSolution.Projects;
            SelectedItems objSelectedItems=dte2.SelectedItems;
            if (objSelectedItems == null)
            {
                return arrInfo.ToArray();
            }
            uih = objSelectedItems as UIHierarchy;
            //Array selectedItems = (Array)objSelectedItems;// uih.SelectedItems;
            //if (selectedItems != null)
            {
                foreach (SelectedItem selItem in objSelectedItems)
                {
                    arrInfo.Add(GetItemInfo(selItem));
                }
                /*
                foreach (UIHierarchyItem selItem in selectedItems)
                {
                    arrInfo.Add(GetItemInfo(selItem));
                }
                */
            }
            return arrInfo.ToArray();
#endif
        }
    }
}

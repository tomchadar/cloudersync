using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClouderSync
{

    public class VSPROJECTEX : VSITEMEX
    {

        public VSPROJECTEX(IVsProject _vsProject, uint _itemid,string _canonicalName=null, IVsSolution _vsSolution=null)
            : base((IVsHierarchy)_vsProject,_itemid, _canonicalName,_vsSolution)
        {
            vsProject = _vsProject;
            if (_canonicalName != null)
            {
                canonicalName = _canonicalName;
            }
            else
            {
                canonicalName = this.GetCanonicalName();
            }
            projectDirectory = Path.GetDirectoryName(canonicalName);
            BaseType = GetType();//.BaseType;
            uint oldID = itemid;
            //this.ParseCanonical();
            if (ErrorHandler.Succeeded(this.ParseCanonicalName(null, ref realid)))
            {
                itemid = realid;
            }
            if (itemid != oldID)
            {
                InitSolution();
            }

        }
        //
        // Summary:
        //     Pointer to an Microsoft.VisualStudio.Shell.Interop.IVsHierarchy interface.
        public IVsProject vsProject=null;
        //public string canonicalName;
    }

    //
    // Summary:
    //     Contains information that uniquely identifies a selected item within a hierarchy.
    public class VSITEMEX 
    {
        //
        // Summary:
        //     Pointer to an Microsoft.VisualStudio.Shell.Interop.IVsHierarchy interface.
        public IVsHierarchy pHier;
        //
        // Summary:
        //     identifier that represents the selected item. For valid itemid values, see VSITEMID.
        public uint itemid;
        public string canonicalName;
        public string fullFilePath = string.Empty;
        public string fileDirectory = string.Empty;
        public string projectDirectory;
        public bool isParentSelected;
        public Guid typeGuid;
        public IVsHierarchy vsParentHier;
        public uint parentId;
        //since 2.1.8
        public uint parentHierId;
        public uint realid;
        public VSITEMEX vsParentItem;
        public VSITEMEX vsParentProject;
        public IVsHierarchy parentProjectHier;
        public bool bProcessed;
        public Type BaseType = null;
        public IVsSolution vsSolution = null;
        public string solutionDirectory;
        public string solutionFile;
        public bool isSourceSelection=false;
        public bool isItemSelected=false;
        public bool isProjectFile = false;

        private VSITEMEX()
        {
            canonicalName = string.Empty;
            isParentSelected = false;
            typeGuid = Guid.Empty;
            vsParentHier = null;
            parentId = 0;
            parentHierId = 0;
            realid = 0;
            vsParentItem = null;
            bProcessed = false;
            parentProjectHier = null;
            vsParentProject = null;
            BaseType = GetType().BaseType;
            vsSolution = null;
            solutionDirectory = string.Empty;
            solutionFile = string.Empty;
            fileDirectory = string.Empty;
            isSourceSelection = false;
            isProjectFile = false;

        }
        public VSITEMEX(IVsHierarchy _pHier,uint _itemid, string _canonicalName = null, IVsSolution _vsSolution=null)
            :this()
        {
            pHier = _pHier;
            itemid = _itemid;

            if(!(pHier is IVsSolution))
            {
                SetVSSolution(_vsSolution);
            }
            if (_canonicalName == null)
            {
                canonicalName = string.Empty;
            }
            else
            {
                canonicalName = _canonicalName;
            }
            if(string.IsNullOrEmpty(canonicalName))
            {
                this.GetCanonicalName();
            }
            if (File.Exists(canonicalName))
            {
                fileDirectory = Path.GetDirectoryName(canonicalName);
            }
            InitSolution();
            if(pHier is IVsProject)
            {
                string _projectDir = projectDirectory;
                projectDirectory = this.GetProjectDirectory(true);
                if(string.IsNullOrEmpty(projectDirectory))
                {
                    projectDirectory = _projectDir;
                }
            }
            InitFullFilePath();
            InitFileDirectory();
#if DEBUG
            this.GetParentHierarchy();
            this.GetParentID();
#endif

        }
        public VSITEMEX(VSITEMSELECTION vsSelect, string _canonicalName=null, IVsSolution _vsSolution=null)
            :this(vsSelect.pHier, vsSelect.itemid, _canonicalName,_vsSolution)
        {
            /*
            pHier = vsSelect.pHier;
            itemid = vsSelect.itemid;
            */
        }
        public void SetVSSolution(IVsSolution _vsSolution)
        {
            if(_vsSolution is IVsSolution)
            {
                vsSolution = _vsSolution;
        //        InitSolution();
            }
        }

        protected bool InitSolution()
        {
            if (pHier is IVsSolution)
            {
                vsSolution = pHier as IVsSolution;
            }
            if(vsSolution is IVsSolution)
            {
                this.GetSolutionFile();
                if (itemid == (uint)VSConstants.VSITEMID.Root)
                {
                    Guid solutionGUID;
                    this.GetCanonicalName(true);
                    int iRes = vsSolution.GetGuidOfProject(pHier, out solutionGUID);
                    // this.ParseCanonical(canonicalName, true);
                    // this.ParseCanonical(solutionFile, true);
                }

            }
            return true;
        }
        public string InitFullFilePath()
        {
            {
                if (!string.IsNullOrEmpty(fullFilePath))
                {
                    return fullFilePath;
                }
                if (this.IsRootSolution())
                {
                    if (string.IsNullOrEmpty(solutionFile))
                    {
                        this.GetSolutionFile();
                    }
                    fullFilePath = solutionFile;
                }
                else
                {
                    if (File.Exists(canonicalName))
                    {
                        fullFilePath = canonicalName;
                    }
                    
                }
                return fullFilePath;
            }
        }
        public string InitFileDirectory()
        {
            {
                if (this.IsRootSolution())
                {
                    if (string.IsNullOrEmpty(solutionDirectory))
                    {
                        this.GetSolutionFile();
                    }
                    fileDirectory = solutionDirectory;
                    return fileDirectory;

                }
                if (pHier is IVsProject)
                {
                    if (!string.IsNullOrEmpty(projectDirectory))
                    {
                        fileDirectory = projectDirectory;
                        return fileDirectory;
                    }
                }
                return fileDirectory;
            }
        }

        public static IVsSolution GetParentSolution(VSITEMEX vsItemEx)
        {
            IVsSolution vsSolution = null;
            if(!VSITEMEX.IsItemIDReal(vsItemEx.itemid))
            {
                vsItemEx.ParseCanonical();
            }
            IVsHierarchy vsParentHier = vsItemEx.GetParentHierarchy();
            if (vsParentHier is IVsSolution)
            {
                vsSolution = vsParentHier as IVsSolution;
            }
            return vsSolution;
        }

        public uint ItemID
        {
            get
            {
                if(realid!=0)
                {
                    return realid;
                }
                return itemid;
            }
        }
        public string FullFilePath
        {
            get
            {
                return fullFilePath;
            }
        }
        public string FileDirectory
        {
            get
            {
                return fileDirectory;
            }
        }
        public string SolutionFile
        {
            get
            {
                if (vsSolution != null)
                {
                    if (!string.IsNullOrEmpty(solutionFile))
                    {
                        return solutionFile;
                    }
                }
                return canonicalName;
            }
        }
        public string SolutionDirectory
        {
            get
            {
                if (vsSolution != null)
                {
                    if (!string.IsNullOrEmpty(solutionDirectory))
                    {
                        return solutionDirectory;
                    }
                }
                return fileDirectory;
            }
        }
        public static bool IsItemIDReal(uint itemId)
        {
            if (itemId < VSConstants.VSITEMID_SELECTION)
            {
                return true;
            }
            return false;

        }
        public bool IsEqual(string _fullPath)
        {
            return string.Equals(fullFilePath, _fullPath, StringComparison.OrdinalIgnoreCase);
        }
        public bool IsEqual(uint _itemID)
        {
            return (this.itemid==_itemID);
        }
        public bool IsEqual(VSITEMEX itemex)
        {
            if (itemid == itemex.itemid)
            {
                string filePath = InitFullFilePath();
                string otherFile = itemex.InitFullFilePath();
                return string.Equals(filePath, otherFile, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public int Compare(string _fullPath)
        {
            return fullFilePath.CompareTo(_fullPath);
        }

        public static bool Exists(List<VSITEMEX> arrInfo, string fullPath)
        {
            List<VSITEMEX> listItems = arrInfo as List<VSITEMEX>;
            return listItems.Exists(cm => cm.IsEqual(fullPath));
        }
        public static bool Exists(List<VSITEMEX> arrInfo, uint itemID)
        {
            return arrInfo.Exists(cm => cm.IsEqual(itemID));
        }
        public static int FindIndex(List<VSITEMEX> arrInfo, string fullPath)
        {
            return arrInfo.FindIndex(cm => cm.IsEqual(fullPath));
        }
        public static int FindIndex(List<VSITEMEX> arrInfo, VSITEMEX itemex)
        {
            return arrInfo.FindIndex(cm => cm.IsEqual(itemex));
        }

        public static void ProcessHierarchy(IVsHierarchy hierarchy, ref List<VSITEMEX> hierList, uint uStartAt= VSConstants.VSITEMID_ROOT)
        {
            // Traverse the nodes of the hierarchy from the root node
            ProcessHierarchyNodeRecursively(hierarchy, uStartAt, ref hierList);
        }

        public static void ProcessHierarchyNodeRecursively(IVsHierarchy hierarchy, uint itemId, ref List<VSITEMEX> hierList)
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
                    ProcessHierarchy(nestedHierarchy,ref hierList);
                }
            }
            else // The node is not the root of another hierarchy, it is a regular node
            {
                AddToList(hierarchy, itemId, ref hierList);
                

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
        public static void AddToList(IVsHierarchy hierarchy, uint itemId, ref List<VSITEMEX> hierList)
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
                if(string.IsNullOrEmpty(canonicalName))
                {
                    return;
                }
                if(!FileTools.FileExists(canonicalName))
                {
                    return;
                }
                IVsProject vsProject = hierarchy as IVsProject;
                if (vsProject!=null)
                {
                    hierList.Add(new VSPROJECTEX(vsProject, itemId, canonicalName));
                }
                else
                {
                    hierList.Add(new VSITEMEX(hierarchy, itemId, canonicalName));
                }
            }
        }

    }


}

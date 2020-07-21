using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClouderSync
{
    internal static partial class VSITEMExtension
    {
        //        private static VSITEMEX _vsItem;

        internal static string GetCanonicalName(this VSITEMEX vsItemEx, bool bForce = false)
        {

            if (!bForce && !string.IsNullOrEmpty(vsItemEx.canonicalName))
            {
                return vsItemEx.canonicalName;
            }
            string canonicalName = string.Empty;
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetCanonicalName(vsItemEx.ItemID, out canonicalName)))
            {
                vsItemEx.canonicalName = canonicalName;
            }
            return canonicalName;
        }
        internal static string GetProjectDirectory(this VSITEMEX vsItemEx, bool bForce = true)
        {
            if (!bForce && !string.IsNullOrEmpty(vsItemEx.projectDirectory))
            {
                return vsItemEx.projectDirectory;
            }
            string projectDirectory = string.Empty;
            object directory;
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_ProjectDir, out directory)))
            {
                projectDirectory = directory.ToString();
                vsItemEx.projectDirectory = projectDirectory;
                return projectDirectory;
            }
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out directory)))
            {
                projectDirectory = directory.ToString();
                vsItemEx.projectDirectory = projectDirectory;
                return projectDirectory;
            }
            return projectDirectory;
        }
        internal static Guid GetItemType(this VSITEMEX vsItemEx, bool bForce=false)
        {
            Guid typeGUID = Guid.Empty;
            if(!bForce && vsItemEx.typeGuid!=Guid.Empty)
            {
                return vsItemEx.typeGuid;
            }
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetGuidProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGUID)))
            {
                vsItemEx.typeGuid = typeGUID;
            }
            return typeGUID;
        }
        internal static int ParseCanonical(this VSITEMEX vsItemEx, string _canonical = null, bool bSetNewID = true)
        {
            uint itemId = 0;
            int iRes = VSConstants.S_FALSE;
            iRes = vsItemEx.ParseCanonicalName(_canonical, ref itemId);
            int iResultiResult = iRes;
            if (ErrorHandler.Succeeded(iRes))
            {
                if (bSetNewID)
                {
                    vsItemEx.itemid = itemId;
                }
                return (int)itemId;

            }
            return 0;
        }

        internal static int ParseCanonicalName(this VSITEMEX vsItemEx, string _canonical, ref uint itemId)
        {
            uint _itemId = 0;
            int iRes = VSConstants.S_OK;
            string canonicalName = vsItemEx.canonicalName;
            if (!string.IsNullOrEmpty(_canonical))
            {
                canonicalName = _canonical;
            }
            iRes = vsItemEx.pHier.ParseCanonicalName(canonicalName, out _itemId);
            if (ErrorHandler.Succeeded(iRes))
            {
                itemId = _itemId;
            }
            return iRes;
        }


        internal static IVsHierarchy GetParentHierarchy(this VSITEMEX vsItemEx)
        {
            IVsHierarchy vsParent = null;
            object objHierarchy = null;
            int iRes = vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_ParentHierarchy, out objHierarchy);
            if (ErrorHandler.Succeeded(iRes))
            {
                vsParent = objHierarchy as IVsHierarchy;
                if (vsParent == null)
                {
                    // vsParent = Marshal.GetIUnknownForObject(objHierarchy);
                }
                else
                {
                    vsItemEx.vsParentHier = vsParent;
                }
                return vsItemEx.vsParentHier;
            }
            return vsParent;
        }
        internal static string GetItemName(this VSITEMEX vsItemEx)
        {
            object objName;
            string itemName = string.Empty;


            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_Name, out objName)))
            {
                if (objName is string)
                {
                    itemName = objName.ToString();
                }
            }
            return itemName;
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
        private static bool TryGetItemId(object obj, out uint id)
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
        internal static uint GetParentHierarchyID(this VSITEMEX vsItemEx)
        {
            uint parentHierId = 0;
            object objID = null;
            IntPtr ptrID = IntPtr.Zero;
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_ParentHierarchyItemid, out objID)))
            {
                if (TryGetItemId(objID, out parentHierId))
                {
                    vsItemEx.parentHierId = parentHierId;
                }
            }
            return parentHierId;
        }
        internal static uint GetParentID(this VSITEMEX vsItemEx)
        {
            uint parentId = 0;
            object objID = null;
            IntPtr ptrID = IntPtr.Zero;
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_Parent, out objID)))
            {
                parentId = GetItemId(objID);
                vsItemEx.parentId = parentId;
                return parentId;
            }
            return parentId;
        }

        internal static VSITEMEX GetParentItemEx(this VSITEMEX vsItemEx, bool bForce = false)
        {
            if (!bForce && vsItemEx.vsParentItem != null)
            {
                return vsItemEx.vsParentItem;
            }
            VSITEMEX vsParentItem = null;
            uint parentId = vsItemEx.GetParentID();
            if (parentId == VSConstants.VSITEMID_NIL)
            {
                vsItemEx.vsParentItem = null;
                return null;
            }
            uint parentHierId = vsItemEx.GetParentHierarchyID();
            IVsHierarchy vsParent = vsItemEx.GetParentHierarchy();
            if (vsParent == null)
            {
                return null;
            }
            vsParentItem = new VSITEMEX(vsParent, parentId);
            vsParentItem.GetCanonicalName();
            vsItemEx.vsParentItem = vsParentItem;
            return vsParentItem;
        }
        internal static VSITEMEX GetTopmostParent(this VSITEMEX vsItemEx, bool bForce = false)
        {
            VSITEMEX vsParentItem = vsItemEx;//.GetParentItemEx();
            VSITEMEX vsTopmostParent = vsParentItem;
            while (vsParentItem != null)
            {
                vsParentItem = vsParentItem.GetParentItemEx();
                if (vsParentItem == null)
                {
                    break;
                }
                vsTopmostParent = vsParentItem;
            }
            return vsTopmostParent;
        }
        internal static bool IsChildOf(this VSITEMEX vsItemEx, IVsHierarchy vshParent)
        {

            VSITEMEX vsTopmostParent = null;
            VSITEMEX vsParentItem = vsItemEx;
            if (vsParentItem != null)
            {
                vsTopmostParent = vsParentItem;
            }
            vsParentItem = GetParentItemEx(vsParentItem);
            return false;
        }
        //since 2.1.2
        internal static bool IsRootSolution(this VSITEMEX vsItemEx)
        {
            vsItemEx.GetItemType();
            vsItemEx.GetParentID();
            if (vsItemEx.pHier is IVsSolution)
            {
                return true;
            }
            //          vsItemEx.GetParentHierarchy();
            if (vsItemEx.typeGuid == Guid.Empty && (vsItemEx.parentId == VSConstants.VSITEMID_NIL || vsItemEx.parentId == 0))
            {
                return true;
            }
            return false;
        }
        internal static bool GetSolutionFile(this VSITEMEX vsItemEx)
        {
            string solutionDir = string.Empty;
            string solutionFile = string.Empty;
            return vsItemEx.GetSolutionFile(ref solutionDir, ref solutionFile);
        }
        internal static bool GetSolutionFile(this VSITEMEX vsItemEx, ref string solutionDirectory, ref string fullFileName)
        {

            if (vsItemEx.vsSolution == null)
            {
                return false;
            }
            string solutionDir = string.Empty;
            string solutionFile = string.Empty;
            string solutionOptFile = string.Empty;

            if (ErrorHandler.Succeeded(vsItemEx.vsSolution.GetSolutionInfo(out solutionDir, out solutionFile, out solutionOptFile)))
            {
                vsItemEx.solutionFile = solutionFile;
                vsItemEx.solutionDirectory = solutionDir;
                solutionDirectory = solutionDir;
                fullFileName = solutionFile;
                return true;
            }
            return false;
        }
        internal static int GetBoolProperty(this VSITEMEX vsItemEx, int propid, ref bool bResult, uint itemId = VSConstants.VSITEMID_NIL)
        {
            int iRes = VSConstants.E_FAIL;
            object objProperty = true;

            if (itemId == VSConstants.VSITEMID_NIL)
            {
                itemId = vsItemEx.ItemID;
            }
            iRes = vsItemEx.pHier.GetProperty(itemId, propid, out objProperty);
            if (ErrorHandler.Succeeded(iRes))
            {
                if (objProperty is bool)
                {
                    bResult = (bool)objProperty;
                }
            }
            return iRes;
        }

        internal static int IsNonMember(this VSITEMEX vsItemEx, ref bool bIsNonMember, uint itemId = VSConstants.VSITEMID_NIL)
        {
            int iRes = VSConstants.E_FAIL;
            iRes = vsItemEx.GetBoolProperty((int)__VSHPROPID.VSHPROPID_IsNonMemberItem, ref bIsNonMember);
            return iRes;
        }
        internal static int IsItemHidden(this VSITEMEX vsItemEx, ref bool bIsHiddenItem, uint itemId = VSConstants.VSITEMID_NIL)
        {
            int iRes = VSConstants.E_FAIL;
            iRes = vsItemEx.GetBoolProperty((int)__VSHPROPID.VSHPROPID_IsHiddenItem, ref bIsHiddenItem);
            return iRes;
        }
        internal static uint GetFirstChild(this VSITEMEX vsItemEx)
        {
            uint childId = VSConstants.VSITEMID_NIL;
            object objID = null;
            IntPtr ptrID = IntPtr.Zero;
            if (ErrorHandler.Succeeded(vsItemEx.pHier.GetProperty(vsItemEx.ItemID, (int)__VSHPROPID.VSHPROPID_FirstChild, out objID)))
            {
                childId = GetItemId(objID);
                //vsItemEx.parentId = parentId;
                return childId;
            }
            return VSConstants.VSITEMID_NIL;
        }
        internal static bool IsProjectFile(this VSITEMEX vsItemEx, string[] szProjectNames)
        {
            bool isProjectFile = false;
            if(vsItemEx.isProjectFile)
            {
                return true;
            }
            if (vsItemEx.pHier is IVsSolution)
            {
                isProjectFile=false;
            }
            if (!File.Exists(vsItemEx.canonicalName))
            {
                isProjectFile=false;
            }
            if (szProjectNames.Contains(vsItemEx.canonicalName, StringComparer.OrdinalIgnoreCase))
            {
                isProjectFile = true;
            }
            /*
            if (!(vsItemEx.pHier is IVsProject))
            {
                isProjectFile=false;
            }
            */
            vsItemEx.isProjectFile = isProjectFile;
            return isProjectFile;
        }


    }
}

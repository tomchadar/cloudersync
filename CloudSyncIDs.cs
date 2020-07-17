using System;

namespace CloudSync
{
    public static class CloudSyncIDs
    {
        /// <summary>
        /// CloudSyncPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "38c40e62-693b-4d18-9981-f5efb7cfeaf8";
        public const string stringCloudSyncCmdSet = "d10c84e7-2237-4b60-85eb-e0e1864f929a";
        public const string stringConfigureSSHCmdSet = "d10c84e7-2237-4b60-85eb-e0e1864f929b";
        public const string stringDeployWindowCmdSet = "d10c84e7-2237-4b60-85eb-e0e1864f929c";
        public const string stringDeployWindow = "00efb722-6d38-4498-96ea-46b9abdd6bbd";
        public static readonly Guid guidCloudSyncCmdSet = new Guid(stringCloudSyncCmdSet);
        public static readonly Guid guidCloudSyncConfigureSSHSet = new Guid(stringConfigureSSHCmdSet);
        public static readonly Guid guidDeployWindowCmdSet = new Guid(stringDeployWindowCmdSet);
        public static readonly Guid guidDeployWindow = new Guid(stringDeployWindow);
        public const int cmdidConfigureSSH = 0x0100;
        public const int cmdidCommandWnd = 0x0104;
        public const int cmdidCloudDeploy = 0x0102;
        public const int cmdidDeployWindow = 0x0105;
    }
}

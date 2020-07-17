using System;
using EnvDTE;
using Microsoft.VisualStudio.Settings;

namespace CloudSync
{


    public class CloudSyncSettings
    {

        public CloudSyncSettings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        public static SettingsManager GetAppSettings(CloudSyncPackage package)
        {
            var dte = package.GetServiceOfType(typeof(EnvDTE.DTE));
            if (dte is DTE)
            {
                return SettingsManagerCreator.GetSettingsManager((DTE)dte);
            }
            return SettingsManagerCreator.GetSettingsManager(package);
            
        }
        public static SettingsManager GetAppSettings(DTE dte)
        {
            //dte = this.package.GetServiceOfType(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            SettingsManager sm = SettingsManagerCreator.GetSettingsManager(dte);
            return sm;
            //string appFolder= sm.GetApplicationDataFolder(ApplicationDataFolder.Configuration);
            //var store = sm.GetWritableSettingsStore(SettingsScope.UserSettings);
        }
    }
}

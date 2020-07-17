//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;

namespace CloudSync
{
    static class SettingsManagerCreator
    {
//        const string VSVersion = "15.0";
#if DEV14
        VSVersion = "14.0";
#elif DEV15
        //const string VSVersion = "15.0";
#else
        //#error Unrecognized VS Version.
#endif
        public static string GetCurrentVSVersion()
        {

            System.Diagnostics.Process proc =System.Diagnostics.Process.GetCurrentProcess();

            ProcessModule mainMod = proc.MainModule;
            FileVersionInfo fileVer=mainMod.FileVersionInfo;
            string szVer = string.Format(CultureInfo.InvariantCulture, @"{0}.0", fileVer.FileMajorPart);
            return szVer;
        }
        public static void GetVSVersions()
        {
            var registry = Registry.ClassesRoot;
            var subKeyNames = registry.GetSubKeyNames();
            var regex = new Regex(@"^VisualStudio\.edmx\.(\d+)\.(\d+)$");
            foreach (var subKeyName in subKeyNames)
            {
                var match = regex.Match(subKeyName);
                if (match.Success)
                {
                    Console.WriteLine("V" + match.Groups[1].Value + "." + match.Groups[2].Value);
                }
            }
        }
        public static SettingsManager GetSettingsManager(DTE dte)
        {
            return GetSettingsManager(new ServiceProvider(((IOleServiceProvider)dte)));
        }

        public static SettingsManager GetSettingsManager(IServiceProvider provider)
        {
            SettingsManager settings = null;
            string devenvPath = null;
            string vsVersion = GetCurrentVSVersion();
            if (provider == null)
            {
                provider = ServiceProvider.GlobalProvider;
            }

            if (provider != null)
            {
                try
                {
                    settings = new ShellSettingsManager(provider);
                }
                catch (NotSupportedException)
                {
                    var dte = (DTE)provider.GetService(typeof(DTE));
                    if (dte != null) {
                        devenvPath = dte.FullName;
                    }
                }
            }
            if (settings == null)
            {
                if (!File.Exists(devenvPath))
                {
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))

                    using (var key = root.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"Software\Microsoft\VisualStudio\{0}\Setup\VS", vsVersion)))
                    {
                        if (key == null)
                        {
                            throw new InvalidOperationException("Cannot find settings store for Visual Studio " + vsVersion);
                        }
                        devenvPath = key.GetValue("EnvironmentPath") as string;
                    }
                }
                if (!File.Exists(devenvPath)) {
                    throw new InvalidOperationException("Cannot find settings store for Visual Studio " + vsVersion);
                }
#if DEBUGFALSE
                settings = ExternalSettingsManager.CreateForApplication(devenvPath, "Exp");
#else
                settings = ExternalSettingsManager.CreateForApplication(devenvPath);
#endif
            }

            return settings;
        }
#if false
        public static EnvDTE80.DTE2 GetDTE2(CloudSyncPackage package)
        {
            EnvDTE80.DTE2 dte = null;
            dte = package.GetServiceOfType(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            if (dte != null)
            {
                return dte;
            }
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }
#endif
        public static SettingsManager GetSettingsManager(CloudSyncPackage package)
        {
            SettingsManager settings = null;
            string devenvPath = null;
            string vsVersion = GetCurrentVSVersion();
            IServiceProvider provider = package.ServiceProvider;
            if (provider == null)
            {
                provider = ServiceProvider.GlobalProvider;
            }
            try
            {
                settings = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            }
            catch (NotSupportedException)
            {
            }
            if(settings!=null)
            {
                return settings;
            }
            if (provider != null)
            {
                try
                {
                    settings = new ShellSettingsManager(provider);
                }
                catch (NotSupportedException)
                {
                    var dte = (DTE)provider.GetService(typeof(DTE));
                    //var dte = GetDTE2(package);
                    if (dte != null)
                    {
                        devenvPath = dte.FullName;
                    }
                }
            }
            if (settings == null)
            {
                if (!File.Exists(devenvPath))
                {
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))

                    using (var key = root.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"Software\Microsoft\VisualStudio\{0}\Setup\VS", vsVersion)))
                    {
                        if (key == null)
                        {
                            throw new InvalidOperationException("Cannot find settings store for Visual Studio " + vsVersion);
                        }
                        devenvPath = key.GetValue("EnvironmentPath") as string;
                    }
                }
                if (!File.Exists(devenvPath))
                {
                    throw new InvalidOperationException("Cannot find settings store for Visual Studio " + vsVersion);
                }
#if DEBUGFALSE
                settings = ExternalSettingsManager.CreateForApplication(devenvPath, "Exp");
#else
                settings = ExternalSettingsManager.CreateForApplication(devenvPath);
#endif
            }

            return settings;
        }
    }
}

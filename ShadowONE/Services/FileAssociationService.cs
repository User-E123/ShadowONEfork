using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ShadowONE.Services
{
    public static class FileAssociationService
    {
        public static void RegisterFileAssociation()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RegisterWindowsFileAssociation();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                RegisterLinuxFileAssociation();
            }
        }

        private static string? GetIconPath()
        {
            var exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            if (string.IsNullOrEmpty(exeDir))
                return null;

            var iconPath = Path.Combine(exeDir, "Assets", "logo.ico");
            return File.Exists(iconPath) ? iconPath : null;
        }

        private static void RegisterWindowsFileAssociation()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                    return;

                var iconPath = GetIconPath();

                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes", true))
                {
                    using (var oneExtKey = key.CreateSubKey(".one"))
                    {
                        oneExtKey.SetValue("", "ShadowONE.File");
                    }

                    using (var progIdKey = key.CreateSubKey("ShadowONE.File"))
                    {
                        progIdKey.SetValue("", "ShadowONE Archive");
                        progIdKey.SetValue("PerceivedType", "text");

                        using (var iconKey = progIdKey.CreateSubKey("DefaultIcon"))
                        {
                            var iconValue = !string.IsNullOrEmpty(iconPath) 
                                ? $"\"{iconPath}\"" 
                                : $"\"{exePath}\",0";
                            iconKey.SetValue("", iconValue);
                        }

                        using (var commandKey = progIdKey.CreateSubKey("shell\\open\\command"))
                        {
                            commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register file association: {ex.Message}");
            }
        }

        private static void RegisterLinuxFileAssociation()
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var applicationsDir = Path.Combine(homeDir, ".local", "share", "applications");
                var iconsDir = Path.Combine(homeDir, ".local", "share", "icons");
                var desktopFilePath = Path.Combine(applicationsDir, "shadowone.desktop");

                Directory.CreateDirectory(applicationsDir);
                Directory.CreateDirectory(iconsDir);

                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                    return;

                var sourceIconPath = GetIconPath();
                var destIconPath = Path.Combine(iconsDir, "shadowone.ico");

                if (!string.IsNullOrEmpty(sourceIconPath) && File.Exists(sourceIconPath))
                {
                    File.Copy(sourceIconPath, destIconPath, true);
                }

                var iconRef = File.Exists(destIconPath) ? destIconPath : "shadowone";

                var desktopFileContent = $@"[Desktop Entry]
Name=ShadowONE
Comment=ONE File Editor for Shadow and Sonic Heroes
Exec=""{exePath}"" %f
Icon={iconRef}
Type=Application
Categories=Utility;
MimeType=application/x-one;
Terminal=false
StartupNotify=false";

                File.WriteAllText(desktopFilePath, desktopFileContent);

                try
                {
                    Process.Start("update-desktop-database", applicationsDir);
                }
                catch
                {
                }

                var mimeDir = Path.Combine(homeDir, ".local", "share", "mime");
                var packagesDir = Path.Combine(mimeDir, "packages");
                Directory.CreateDirectory(packagesDir);

                var mimeFilePath = Path.Combine(packagesDir, "shadowone.xml");
                var mimeContent = @"<?xml version=""1.0""?>
<mime-info xmlns=""http://www.freedesktop.org/standards/shared-mime-info"">
  <mime-type type=""application/x-one"">
    <comment>ONE Archive</comment>
    <glob pattern=""*.one""/>
    <glob pattern=""*.ONE""/>
  </mime-type>
</mime-info>";

                File.WriteAllText(mimeFilePath, mimeContent);

                try
                {
                    Process.Start("update-mime-database", mimeDir);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register file association: {ex.Message}");
            }
        }
    }
}

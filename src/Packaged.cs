// Dino Joe - Microsoft Store (MSIX) verzija: prepoznavanje paketa i autostart preko StartupTask-a.
// U MSIX paketu Run ključ u registru ne radi, pa se pokretanje sa Windowsom uključuje/isključuje u
// Postavke > Aplikacije > Pokretanje (StartupTask "DinoJoeStartup" iz AppxManifest.xml).
// C# 5 kompatibilno.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DinoJoe
{
    static class Packaged
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetCurrentPackageFamilyName(ref int packageFamilyNameLength, StringBuilder packageFamilyName);

        static int state = -1;   // -1 nepoznato, 0 ne, 1 da

        /// true kad aplikacija radi iz MSIX paketa (Microsoft Store).
        public static bool IsPackaged
        {
            get
            {
                if (state < 0)
                {
                    try
                    {
                        int len = 0;
                        int r = GetCurrentPackageFullName(ref len, null);
                        state = r == 15700 ? 0 : 1;   // APPMODEL_ERROR_NO_PACKAGE
                    }
                    catch { state = 0; }   // Windows 7 / mono
                }
                return state == 1;
            }
        }

        /// U Store verziji Windows preusmjerava %AppData% aplikacije u
        /// %LocalAppData%\Packages\<familija>\LocalCache\Roaming. Explorer i Notepad rade van paketa i
        /// vide samo pravu putanju, pa im se mora dati ona (inače Explorer otvori Dokumente).
        public static string RealPath(string path)
        {
            try
            {
                if (!IsPackaged || string.IsNullOrEmpty(path)) return path;
                string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).TrimEnd('\\');
                if (!path.StartsWith(roaming, StringComparison.OrdinalIgnoreCase)) return path;
                int len = 0;
                GetCurrentPackageFamilyName(ref len, null);
                StringBuilder fam = new StringBuilder(len + 1);
                if (GetCurrentPackageFamilyName(ref len, fam) != 0) return path;
                string real = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages"), fam.ToString()), "LocalCache"), "Roaming")
                    + path.Substring(roaming.Length);
                return System.IO.File.Exists(real) || System.IO.Directory.Exists(real) ? real : path;
            }
            catch { return path; }
        }

        /// Otvori folder u Exploreru (radi i u Store verziji).
        public static void OpenFolder(string dir)
        {
            try { System.IO.Directory.CreateDirectory(dir); } catch { }
            try { System.Diagnostics.Process.Start("explorer.exe", "\"" + RealPath(dir) + "\""); } catch { }
        }

        public static void OpenStartupSettings()
        {
            try { System.Diagnostics.Process.Start("ms-settings:startupapps"); } catch { }
        }
    }
}

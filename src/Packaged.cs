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

        public static void OpenStartupSettings()
        {
            try { System.Diagnostics.Process.Start("ms-settings:startupapps"); } catch { }
        }
    }
}

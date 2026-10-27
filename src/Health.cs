// Dino Joe - pouzdanost: log grešaka i pauza kad korisnik ne gleda (igra/prezentacija preko cijelog ekrana, zaključan ekran).
// Log: %AppData%\DinoJoe\logs\error-YYYY-MM-DD.log (ista greška se upisuje najviše jednom u minuti).
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DinoJoe
{
    static class ErrorLog
    {
        static readonly Dictionary<string, DateTime> lastSeen = new Dictionary<string, DateTime>();
        static readonly object sync = new object();

        /// Poziva se kad se greška upiše (PetForm pokaže oblačić).
        public static event Action Logged;

        public static string Dir
        {
            get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id), "logs"); }
        }

        public static void Install()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e) { Write("UI", e.Exception); };
            AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e) { Write("FATAL", e.ExceptionObject as Exception); };
        }

        public static void Write(string where, Exception ex)
        {
            try
            {
                string key = where + "|" + (ex == null ? "?" : ex.GetType().Name + ex.Message);
                lock (sync)
                {
                    DateTime t;
                    if (lastSeen.TryGetValue(key, out t) && (DateTime.Now - t).TotalSeconds < 60) return;
                    lastSeen[key] = DateTime.Now;
                }
                Directory.CreateDirectory(Dir);
                StringBuilder b = new StringBuilder();
                b.Append("=== ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(" [").Append(where).Append("]\r\n");
                b.Append("Dino Joe ").Append(typeof(ErrorLog).Assembly.GetName().Version)
                 .Append(" | ").Append(Environment.OSVersion).Append(Environment.Is64BitOperatingSystem ? " x64" : " x86")
                 .Append(" | .NET ").Append(Environment.Version)
                 .Append(" | ").Append(Packaged.IsPackaged ? "Store" : "zip/winget")
                 .Append(" | jezik ").Append(Settings.LangCode)
                 .Append(" | ekrana ").Append(ScreenCount()).Append("\r\n");
                b.Append(ex == null ? "(nepoznata greška)" : ex.ToString()).Append("\r\n\r\n");
                File.AppendAllText(Path.Combine(Dir, "error-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"), b.ToString(), new UTF8Encoding(false));
                if (Logged != null) Logged();
            }
            catch { }
        }

        static string ScreenCount() { try { return Screen.AllScreens.Length.ToString(); } catch { return "?"; } }

        /// Otvori folder sa logovima i stranicu za prijavu problema.
        public static void OpenReport()
        {
            try { Directory.CreateDirectory(Dir); System.Diagnostics.Process.Start("explorer.exe", "\"" + Dir + "\""); } catch { }
            try { System.Diagnostics.Process.Start("https://github.com/freedole/dino-joe/issues/new"); } catch { }
        }
    }

    /// Da li je trenutno bolje da dino ne smeta (igra / prezentacija preko cijelog ekrana, zaključan ekran).
    static class Quiet
    {
        [DllImport("shell32.dll")] static extern int SHQueryUserNotificationState(out int state);
        // 1 QUNS_NOT_PRESENT (zaključano / screensaver / drugi korisnik), 2 QUNS_BUSY (app preko cijelog ekrana),
        // 3 QUNS_RUNNING_D3D_FULL_SCREEN (igra), 4 QUNS_PRESENTATION_MODE, 5 QUNS_ACCEPTS_NOTIFICATIONS, 6 QUNS_QUIET_TIME, 7 QUNS_APP
        static bool failed;

        /// Zadnje stanje: 2 cijeli ekran (film/prezentacija), 3 igra (D3D), 4 prezentacijski mod, 1 zaključano.
        public static int LastState;

        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);

        /// Programi za udaljeni rad i virtuelne mašine: preko cijelog ekrana su "radni", ne film/igra, pa dino ostaje.
        static readonly string[] RemoteApps = { "mstsc", "msrdc", "rdcman", "vmconnect", "vmware", "vmware-vmx", "virtualboxvm",
                                                "teamviewer", "anydesk", "rustdesk", "vncviewer", "tvnviewer", "remotedesktopmanager", "parsecd", "citrix", "wfica32", "cdviewer" };

        static bool RemoteInFront()
        {
            try
            {
                uint pid;
                GetWindowThreadProcessId(GetForegroundWindow(), out pid);
                if (pid == 0) return false;
                string name = System.Diagnostics.Process.GetProcessById((int)pid).ProcessName.ToLowerInvariant();
                foreach (string r in RemoteApps) if (name == r || name.StartsWith(r)) return true;
            }
            catch { }
            return false;
        }

        public static bool ShouldHide
        {
            get
            {
                if (failed) return false;
                try
                {
                    int st;
                    if (SHQueryUserNotificationState(out st) != 0) return false;
                    LastState = st;
                    if (st == 1) return true;                                   // zaključan ekran
                    if (st == 2 || st == 3 || st == 4) return !RemoteInFront();  // cijeli ekran, osim RDP/VM/udaljenog pristupa
                    return false;
                }
                catch { failed = true; return false; }   // stariji Windows / mono
            }
        }
    }
}

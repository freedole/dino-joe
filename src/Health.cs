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

        public static void Write(string where, Exception ex) { Write(where, ex, true); }

        // ---- sesija: ako se prethodno pokretanje nije uredno završilo (proces prekinut bez .NET greške), zapiši to ----
        static string FlagPath { get { return Path.Combine(Path.GetDirectoryName(Dir), "session.flag"); } }
        static DateTime lastTouch = DateTime.MinValue;

        public static void SessionStart()
        {
            try
            {
                if (File.Exists(FlagPath))
                {
                    string last = File.ReadAllText(FlagPath, Encoding.UTF8);
                    Write("NESTAO", new Exception("Prethodno pokretanje nije uredno zatvoreno (proces je prekinut ili se srušio bez .NET greške). Zadnje stanje:\r\n" + last), false);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(FlagPath));
                File.WriteAllText(FlagPath, "start=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), new UTF8Encoding(false));
            }
            catch { }
        }

        /// Povremeno upiši zadnje stanje (pozicija, stanje, ekrani) - korisno ako dino "nestane".
        public static void SessionTouch(string info)
        {
            try
            {
                if ((DateTime.Now - lastTouch).TotalSeconds < 10) return;
                lastTouch = DateTime.Now;
                File.WriteAllText(FlagPath, "time=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + info, new UTF8Encoding(false));
            }
            catch { }
        }

        public static void SessionEnd() { try { if (File.Exists(FlagPath)) File.Delete(FlagPath); } catch { } }

        public static void Write(string where, Exception ex, bool notify)
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
                if (notify && Logged != null) Logged();
            }
            catch { }
        }

        static string ScreenCount() { try { return Screen.AllScreens.Length.ToString(); } catch { return "?"; } }

        /// Otvori folder sa logovima i stranicu za prijavu problema.
        public static void OpenReport()
        {
            Packaged.OpenFolder(Dir);
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

        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr h, System.Text.StringBuilder s, int n);
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int idx);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] static extern IntPtr MonitorFromWindow(IntPtr h, uint flags);
        [DllImport("user32.dll")] static extern bool GetMonitorInfo(IntPtr m, ref MONITORINFO mi);
        [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int attr, out int v, int size);
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] struct MONITORINFO { public int cbSize; public RECT rcMonitor, rcWork; public uint dwFlags; }

        static readonly string[] ShellClasses = { "Progman", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "NotifyIconOverflowWindow", "Windows.UI.Core.CoreWindow" };

        /// Prozor u fokusu je stvaran (vidljiv, nije desktop/taskbar, nije providni sloj) i pokriva cijeli svoj monitor.
        static bool RealFullscreenInFront()
        {
            try
            {
                IntPtr h = GetForegroundWindow();
                if (h == IntPtr.Zero || !IsWindowVisible(h) || IsIconic(h)) return false;
                uint pid; GetWindowThreadProcessId(h, out pid);
                if (pid == (uint)System.Diagnostics.Process.GetCurrentProcess().Id) return false;
                System.Text.StringBuilder cls = new System.Text.StringBuilder(256);
                GetClassName(h, cls, cls.Capacity);
                foreach (string c in ShellClasses) if (cls.ToString() == c) return false;
                int ex = GetWindowLong(h, -20);                    // GWL_EXSTYLE
                if ((ex & 0x20) != 0 || (ex & 0x80) != 0) return false;   // WS_EX_TRANSPARENT (click-through sloj), WS_EX_TOOLWINDOW (widget)
                int cloaked = 0;
                try { if (DwmGetWindowAttribute(h, 14, out cloaked, 4) == 0 && cloaked != 0) return false; } catch { }
                RECT r; if (!GetWindowRect(h, out r)) return false;
                MONITORINFO mi = new MONITORINFO(); mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                if (!GetMonitorInfo(MonitorFromWindow(h, 2), ref mi)) return false;
                return r.Left <= mi.rcMonitor.Left && r.Top <= mi.rcMonitor.Top && r.Right >= mi.rcMonitor.Right && r.Bottom >= mi.rcMonitor.Bottom;
            }
            catch { return true; }   // ako provjera ne uspije, ponašaj se kao ranije
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
                    if (st == 4) return !RemoteInFront();                       // prezentacijski mod (uključen ručno)
                    // 2/3: Windows ponekad javlja "cijeli ekran" i kad je u fokusu desktop ili nevidljivi sloj
                    // (npr. plutajući widgeti antivirusa). Sakrij se samo ako pravi prozor zaista pokriva cijeli monitor.
                    if (st == 2 || st == 3) return RealFullscreenInFront() && !RemoteInFront();
                    return false;
                    return false;
                }
                catch { failed = true; return false; }   // stariji Windows / mono
            }
        }
    }
}

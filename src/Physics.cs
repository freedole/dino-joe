// Dino Joe - prozori kao platforme: dino može stajati i šetati po gornjoj ivici prozora.
// Uzimaju se samo vidljivi, nemaksimizovani glavni prozori čija je gornja ivica stvarno vidljiva (nije pokrivena drugim prozorom).
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace DinoJoe
{
    static class WindowPlatforms
    {
        delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] struct POINT { public int X, Y; public POINT(int x, int y) { X = x; Y = y; } }

        [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr lParam);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
        [DllImport("user32.dll")] static extern bool IsZoomed(IntPtr h);
        [DllImport("user32.dll")] static extern bool IsWindow(IntPtr h);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int idx);
        [DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr h);
        [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(POINT p);
        [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr h, uint flags);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
        [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int attr, out RECT r, int size);
        [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int attr, out int v, int size);

        public struct Win { public IntPtr Handle; public Rectangle Rect; }

        static readonly List<Win> cache = new List<Win>();
        static int cacheTick = -100000;
        static bool failed;
        static readonly uint myPid = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

        /// Pravi pravougaonik prozora (bez nevidljive DWM ivice).
        public static bool TryRect(IntPtr h, out Rectangle rect)
        {
            rect = Rectangle.Empty;
            if (failed) return false;
            try
            {
                if (!IsWindow(h) || !IsWindowVisible(h) || IsIconic(h) || IsZoomed(h)) return false;
                RECT r;
                if (DwmGetWindowAttribute(h, 9, out r, Marshal.SizeOf(typeof(RECT))) != 0 && !GetWindowRect(h, out r)) return false;
                rect = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
                return rect.Width >= 200 && rect.Height >= 80;
            }
            catch { failed = true; return false; }
        }

        static bool Usable(IntPtr h, Rectangle screen)
        {
            if (GetWindowTextLength(h) == 0) return false;
            if ((GetWindowLong(h, -20) & 0x80) != 0) return false;            // WS_EX_TOOLWINDOW
            uint pid; GetWindowThreadProcessId(h, out pid);
            if (pid == myPid) return false;                                    // ne svoje prozore
            int cloaked;
            if (DwmGetWindowAttribute(h, 14, out cloaked, 4) == 0 && cloaked != 0) return false;   // skriveni UWP / drugi desktop
            return true;
        }

        /// Lista kandidata (osvježava se najviše svakih 300 ms).
        public static List<Win> List()
        {
            if (failed) return cache;
            int now = Environment.TickCount;
            if (now - cacheTick < 300) return cache;
            cacheTick = now;
            cache.Clear();
            try
            {
                EnumWindows(delegate(IntPtr h, IntPtr l)
                {
                    Rectangle r;
                    if (TryRect(h, out r) && Usable(h, r)) { Win w; w.Handle = h; w.Rect = r; cache.Add(w); }
                    return true;
                }, IntPtr.Zero);
            }
            catch { failed = true; }
            return cache;
        }

        /// Da li je gornja ivica prozora na x vidljiva (nije pokrivena drugim prozorom).
        public static bool TopVisibleAt(IntPtr h, int x, int top)
        {
            try
            {
                IntPtr w = WindowFromPoint(new POINT(x, top + 3));
                if (w == IntPtr.Zero) return false;
                return GetAncestor(w, 2) == h;   // GA_ROOT
            }
            catch { return false; }
        }
    }
}

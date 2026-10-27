// Dino Joe - trigger fore: reakcije na sistem i vrijeme (CPU, baterija, ponoć, ponedjeljak 8h, povratak nakon neaktivnosti).
// Provjera svakih 5 sekundi; svaki trigger ima svoj "cooldown" da ne dosađuje.
// C# 5 kompatibilno.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DinoJoe
{
    class Triggers
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
        [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        PerformanceCounter cpu;
        bool cpuFailed;
        int cpuHigh;
        DateTime cpuCooldown = DateTime.MinValue, batCooldown = DateTime.MinValue;
        DateTime lastMidnight = DateTime.MinValue, lastMonday = DateTime.MinValue;
        bool wasIdle;
        double check;
        readonly Random rnd = new Random();

        const double IdleMinutes = 10;

        /// Poziva se svaki frejm; vraća tekst za oblačić ili null.
        public string Step(double dt)
        {
            check += dt;
            if (check < 5) return null;
            check = 0;
            DateTime now = DateTime.Now;

            // povratak nakon neaktivnosti
            double idle = IdleSeconds();
            if (idle >= IdleMinutes * 60) wasIdle = true;
            else if (wasIdle && idle < 10) { wasIdle = false; return Pick("trg.back"); }

            // ponoć (jednom dnevno)
            if (now.Hour == 0 && now.Minute < 2 && lastMidnight.Date != now.Date) { lastMidnight = now; return Pick("trg.midnight"); }

            // ponedjeljak 8h
            if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 8 && now.Minute < 2 && lastMonday.Date != now.Date) { lastMonday = now; return Pick("trg.monday"); }

            // baterija ispod 15% i nije na punjaču
            try
            {
                PowerStatus ps = SystemInformation.PowerStatus;
                if (ps.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery && ps.BatteryLifePercent > 0 &&
                    ps.BatteryLifePercent < 0.15f && ps.PowerLineStatus == PowerLineStatus.Offline && now > batCooldown)
                { batCooldown = now.AddMinutes(30); return Pick("trg.battery"); }
            }
            catch { }

            // CPU preko 90% duže od 15 sekundi
            float load = Cpu();
            if (load > 90) cpuHigh++; else cpuHigh = 0;
            if (cpuHigh >= 3 && now > cpuCooldown) { cpuHigh = 0; cpuCooldown = now.AddMinutes(15); return Pick("trg.cpu"); }

            return null;
        }

        /// Ključ zadnjeg okinutog triggera (npr. "trg.midnight") - za dostignuća.
        public string LastKey;

        string Pick(string key)
        {
            LastKey = key;
            string[] v = Lang.Gender(Lang.Get(key)).Split('|');
            return v[rnd.Next(v.Length)];
        }

        float Cpu()
        {
            if (cpuFailed) return 0;
            try
            {
                if (cpu == null) { cpu = new PerformanceCounter("Processor", "_Total", "% Processor Time"); cpu.NextValue(); return 0; }
                return cpu.NextValue();
            }
            catch { cpuFailed = true; return 0; }
        }

        static double IdleSeconds()
        {
            try
            {
                LASTINPUTINFO li = new LASTINPUTINFO();
                li.cbSize = (uint)Marshal.SizeOf(li);
                if (!GetLastInputInfo(ref li)) return 0;
                return ((uint)Environment.TickCount - li.dwTime) / 1000.0;
            }
            catch { return 0; }
        }
    }
}

// Dino Joe - životni ciklus: jaje -> beba -> mladunče -> odrasli.
// Jaje se grije klikom, mazenjem i mišem iznad njega (1-2 minute). Rast traje danima i brži je uz dobru brigu.
// Podaci: %AppData%\DinoJoe\life.txt. Korisnici iz v1.0 (nema life.txt, a setup je završen) ostaju odrasli.
// C# 5 kompatibilno.

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DinoJoe
{
    static class Life
    {
        public const int Egg = 0, Baby = 1, Juvenile = 2, Adult = 3;

        /// Sati rasta potrebni za prelazak u sljedeću fazu (pri prosječnoj brizi).
        public static readonly double[] StageHours = { 0, 24, 72, 0 };
        /// Veličina u odnosu na odraslog.
        public static readonly float[] StageScale = { 1f, 0.56f, 0.78f, 1f };

        public static int Stage = Adult;
        public static double Warmth;          // 0..100, samo za jaje
        public static double Growth;          // sati rasta u trenutnoj fazi
        public static int Eggs;               // koliko je jaja izleženo (za "Novo jaje")
        static DateTime lastSeen = DateTime.Now;
        static bool exists;

        static string FilePath
        {
            get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id), "life.txt"); }
        }

        public static bool Exists { get { return exists; } }

        public static void Load()
        {
            exists = false;
            try
            {
                if (!File.Exists(FilePath)) return;
                exists = true;
                foreach (string raw in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    int i = raw.IndexOf('=');
                    if (i <= 0) continue;
                    string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                    int n; double d; long l;
                    if (k == "stage" && int.TryParse(v, out n)) Stage = Math.Max(Egg, Math.Min(Adult, n));
                    else if (k == "warmth" && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) Warmth = d;
                    else if (k == "growth" && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) Growth = d;
                    else if (k == "eggs" && int.TryParse(v, out n)) Eggs = n;
                    else if (k == "last" && long.TryParse(v, out l)) lastSeen = new DateTime(l);
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                lastSeen = DateTime.Now;
                File.WriteAllText(FilePath,
                    "# Dino Joe - faza zivota (0 jaje, 1 beba, 2 mladunce, 3 odrasli)\r\n" +
                    "stage=" + Stage + "\r\n" +
                    "warmth=" + Warmth.ToString("0.##", CultureInfo.InvariantCulture) + "\r\n" +
                    "growth=" + Growth.ToString("0.####", CultureInfo.InvariantCulture) + "\r\n" +
                    "eggs=" + Eggs + "\r\n" +
                    "last=" + lastSeen.Ticks + "\r\n", new UTF8Encoding(false));
                exists = true;
            }
            catch { }
        }

        /// Novi dino: počinje kao jaje.
        public static void StartEgg()
        {
            Stage = Egg; Warmth = 0; Growth = 0;
            Save();
        }

        /// Korisnik iz v1.0: ostaje odrasli.
        public static void StartAdult()
        {
            Stage = Adult; Warmth = 100; Growth = 0;
            Save();
        }

        public static float Scale { get { return StageScale[Stage]; } }

        /// Rast dok je aplikacija bila ugašena (najviše 3 dana, normalnim tempom).
        public static bool CatchUp(bool paused)
        {
            double hours = (DateTime.Now - lastSeen).TotalHours;
            if (paused || hours <= 0.01) return false;
            return Grow(Math.Min(72, hours) * 3600, 50);
        }

        /// Dodaje rast; happiness 0..100 (Zadovoljstvo) ubrzava ili usporava rast 0.5x..1.5x.
        /// Vraća true kad dino pređe u sljedeću fazu.
        public static bool Grow(double seconds, double happiness)
        {
            if (Stage != Baby && Stage != Juvenile) return false;
            Growth += seconds / 3600.0 * (0.5 + Math.Max(0, Math.Min(100, happiness)) / 100.0);
            if (Growth < StageHours[Stage]) return false;
            Growth = 0;
            Stage++;
            Save();
            return true;
        }

        /// Grijanje jajeta; vraća true kad je jaje spremno za izlijeganje.
        public static bool Warm(double amount)
        {
            if (Stage != Egg) return false;
            Warmth = Math.Min(100, Warmth + amount);
            return Warmth >= 100;
        }

        public static void Hatched()
        {
            Stage = Baby; Growth = 0; Warmth = 100; Eggs++;
            Save();
        }

        /// 0..1 napredak do sljedeće faze.
        public static double Progress
        {
            get
            {
                if (Stage == Egg) return Warmth / 100.0;
                if (Stage == Adult) return 1;
                return Math.Min(1, Growth / StageHours[Stage]);
            }
        }

        public static string StageName(int s) { return Lang.Get("life.stage" + s); }
    }
}

// Dino Joe - potrebe ljubimca (status): hrana, voda, energija, mazenje, zabava, higijena, zdravlje.
// Vrijednosti 0..100 (100 = sve super). Zadovoljstvo = ponderisani prosjek, Briga = koliko je zanemaren.
// Pad se računa i dok je aplikacija ugašena; "Godišnji odmor" pauzira sve.
// Hardcore Tamagotchi mod: brži pad, nema donje granice, može se razboljeti i "pobjeći".
// C# 5 kompatibilno.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DinoJoe
{
    enum Need { Food, Water, Energy, Love, Fun, Hygiene, Health }

    class Needs
    {
        public const int Count = 7;
        public readonly double[] V = { 85, 85, 90, 80, 80, 95, 100 };
        public bool Vacation;
        public bool RanAway;
        DateTime lastUpdate = DateTime.Now;

        // pad po satu (normalni mod); energija u snu raste
        static readonly double[] DecayPerHour = { 11, 14, 7, 9, 12, 4, 0 };
        static readonly int[] Weight = { 20, 15, 10, 20, 15, 10, 10 };
        public static readonly string[] Keys = { "food", "water", "energy", "love", "fun", "hygiene", "health" };

        public static bool Hardcore { get { return Settings.PersonalityId == "tamagotchi"; } }
        double Mul { get { return Hardcore ? 2.5 : 1.0; } }
        double Floor { get { return Hardcore ? 0 : 20; } }   // normalni mod: ljubimac nikad ne propadne skroz

        string Path_ { get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id), "needs.txt"); } }

        public double this[Need n] { get { return V[(int)n]; } }

        // ---------------- vrijeme ----------------
        public void Tick(double seconds, bool sleeping)
        {
            lastUpdate = DateTime.Now;
            if (Vacation || RanAway) return;
            Apply(seconds / 3600.0, sleeping, 1.0);
        }

        void Apply(double hours, bool sleeping, double rate)
        {
            double m = Mul * rate;
            for (int i = 0; i < Count; i++)
            {
                if (i == (int)Need.Health) continue;
                double d = DecayPerHour[i] * m * hours;
                if (i == (int)Need.Energy && sleeping) d = -30 * hours;     // spavanje puni energiju
                if (sleeping && i != (int)Need.Energy) d *= 0.5;            // u snu sve sporije pada
                V[i] = Clamp(V[i] - d, i == (int)Need.Energy ? 0 : Floor);
            }
            // zdravlje: pada ako su hrana/voda/higijena kritične, inače se oporavlja
            bool bad = V[0] < 20 || V[1] < 20 || V[5] < 15;
            bool good = V[0] > 50 && V[1] > 50 && V[5] > 40;
            if (bad) V[6] = Clamp(V[6] - 6 * m * hours, Hardcore ? 0 : 40);
            else if (good) V[6] = Clamp(V[6] + 4 * hours, 0);
        }

        static double Clamp(double v, double floor) { return Math.Max(floor, Math.Min(100, v)); }

        // ---------------- akcije ----------------
        public void Add(Need n, double amount) { V[(int)n] = Clamp(V[(int)n] + amount, 0); }

        // ---------------- izvedene vrijednosti ----------------
        public int Happiness
        {
            get
            {
                double s = 0; int w = 0;
                for (int i = 0; i < Count; i++) { s += V[i] * Weight[i]; w += Weight[i]; }
                return (int)Math.Round(s / w);
            }
        }

        public int Worry
        {
            get
            {
                double s = 0; int w = 0;
                for (int i = 0; i < Count; i++) { s += Math.Max(0, 30 - V[i]) / 30.0 * Weight[i]; w += Weight[i]; }
                return (int)Math.Round(Math.Min(100, s / w * 250));
            }
        }

        public Need Lowest
        {
            get
            {
                int m = 0;
                for (int i = 1; i < Count; i++) if (V[i] < V[m]) m = i;
                return (Need)m;
            }
        }

        public bool Sick { get { return V[6] < 30; } }
        public bool ShouldRunAway { get { return Hardcore && !Vacation && Happiness < 15 && V[6] < 15; } }

        // ---------------- snimanje ----------------
        public void Load()
        {
            try
            {
                if (!File.Exists(Path_)) { lastUpdate = DateTime.Now; return; }
                DateTime saved = DateTime.Now;
                foreach (string raw in File.ReadAllLines(Path_))
                {
                    int i = raw.IndexOf('=');
                    if (i <= 0) continue;
                    string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                    double d; long l;
                    int idx = Array.IndexOf(Keys, k);
                    if (idx >= 0 && double.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d)) V[idx] = Clamp(d, 0);
                    else if (k == "time" && long.TryParse(v, out l)) saved = new DateTime(l);
                    else if (k == "vacation") Vacation = v == "1";
                    else if (k == "ranaway") RanAway = v == "1";
                }
                // vrijeme dok je app bio ugašen: računa se kao spavanje, najviše 3 dana
                double hours = Math.Max(0, Math.Min(72, (DateTime.Now - saved).TotalHours));
                if (!Vacation && !RanAway && hours > 0.01) Apply(hours, true, 1.0);
                lastUpdate = DateTime.Now;
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < Count; i++)
                    b.Append(Keys[i]).Append('=').Append(V[i].ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)).Append("\r\n");
                b.Append("time=").Append(lastUpdate.Ticks).Append("\r\n");
                b.Append("vacation=").Append(Vacation ? "1" : "0").Append("\r\n");
                b.Append("ranaway=").Append(RanAway ? "1" : "0").Append("\r\n");
                Directory.CreateDirectory(Path.GetDirectoryName(Path_));
                File.WriteAllText(Path_, b.ToString());
            }
            catch { }
        }

        public static string Label(Need n) { return Lang.Get("need." + Keys[(int)n]); }
    }

    // ======================================================================
    //  PROZOR STATUS
    // ======================================================================
    class StatusForm : ThemedForm
    {
        readonly Needs needs;
        readonly Timer timer = new Timer();
        readonly CheckBox chVacation = new CheckBox();
        public event Action<string> ActionClicked;   // "feed", "water", "pet", "play", "sleep", "bath"

        public StatusForm(Needs needs, string petName)
        {
            this.needs = needs;
            Text = petName + " - " + Lang.Get("st.title");
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(400, 490);
            TopMost = false;   // ne pokrivaj browser (npr. stranicu za API ključ)
            DoubleBuffered = true;
            BackColor = Color.White;

            string[] acts = { "feed", "water", "pet", "play", "sleep", "bath" };
            for (int i = 0; i < acts.Length; i++)
            {
                string a = acts[i];
                Button b = new ModernButton();
                b.Text = Lang.Get("act." + a);
                b.SetBounds(16 + (i % 3) * 124, 380 + (i / 3) * 36, 118, 30);
                b.Click += delegate { if (ActionClicked != null) ActionClicked(a); Invalidate(); };
                Controls.Add(b);
            }
            chVacation.Text = Lang.Get("st.vacation");
            chVacation.Checked = needs.Vacation;
            chVacation.SetBounds(16, 454, 360, 26);
            chVacation.CheckedChanged += delegate { needs.Vacation = chVacation.Checked; needs.Save(); Invalidate(); };
            Controls.Add(chVacation);

            timer.Interval = 1000;
            timer.Tick += delegate { Invalidate(); };
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // dvije glavne linije
            BigBar(g, 16, 14, Lang.Get("st.happy"), needs.Happiness, false);
            BigBar(g, 16, 66, Lang.Get("st.worry"), needs.Worry, true);

            using (Pen sep = new Pen(Color.FromArgb(230, 230, 230))) g.DrawLine(sep, 16, 118, 384, 118);

            for (int i = 0; i < Needs.Count; i++)
                SmallBar(g, 16, 130 + i * 31, Needs.Label((Need)i), (int)Math.Round(needs.V[i]));

            string life = Life.Stage == Life.Egg ? string.Format(Lang.Get("life.egg"), (int)Life.Warmth)
                        : Life.Stage == Life.Adult ? Lang.Get("life.stage") + " " + Life.StageName(Life.Adult)
                        : string.Format(Lang.Get("life.next"), Life.StageName(Life.Stage), (int)(Life.Progress * 100));
            using (SolidBrush lb = new SolidBrush(Color.FromArgb(90, 100, 110)))
            {
                SizeF ls = g.MeasureString(life, Font);
                g.DrawString(life, Font, lb, 384 - ls.Width, 350);
            }

            string state = needs.Vacation ? Lang.Get("st.onvacation") : (needs.Sick ? Lang.Get("st.sick") : "");
            if (state.Length > 0)
                TextRenderer.DrawText(g, state, Font, new Point(16, 350), Color.FromArgb(180, 90, 30), TextFormatFlags.NoPrefix);   // GDI: emoji fallback
        }

        static Color BarColor(int v, bool inverse)
        {
            int x = inverse ? 100 - v : v;
            if (x >= 60) return Color.FromArgb(70, 175, 90);
            if (x >= 30) return Color.FromArgb(235, 170, 50);
            return Color.FromArgb(220, 70, 70);
        }

        void BigBar(Graphics g, int x, int y, string label, int v, bool inverse)
        {
            using (Font f = new Font("Segoe UI Semibold", 10.5f))
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                g.DrawString(label, f, tb, x, y);
                string pct = v + "%";
                SizeF sz = g.MeasureString(pct, f);
                g.DrawString(pct, f, tb, 384 - sz.Width, y);
            }
            Bar(g, x, y + 24, 368, 16, v, BarColor(v, inverse));
        }

        void SmallBar(Graphics g, int x, int y, string label, int v)
        {
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(70, 70, 70))) g.DrawString(label, Font, tb, x, y + 2);
            Bar(g, x + 110, y + 5, 220, 12, v, BarColor(v, false));
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(110, 110, 110))) g.DrawString(v.ToString(), Font, tb, x + 340, y + 2);
        }

        static void Bar(Graphics g, float x, float y, float w, float h, int v, Color c)
        {
            using (GraphicsPath bg = Round(x, y, w, h))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(236, 238, 240))) g.FillPath(b, bg);
            float fw = Math.Max(h, w * Math.Max(0, Math.Min(100, v)) / 100f);
            if (v > 0)
                using (GraphicsPath fg = Round(x, y, fw, h))
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, y), new PointF(0, y + h), ControlPaint.Light(c, 0.3f), c))
                    g.FillPath(b, fg);
        }

        static GraphicsPath Round(float x, float y, float w, float h)
        {
            GraphicsPath p = new GraphicsPath();
            float d = h;
            p.AddArc(x, y, d, d, 90, 180);
            p.AddArc(x + w - d, y, d, d, 270, 180);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Stop(); timer.Dispose(); }
            base.Dispose(disposing);
        }
    }
}

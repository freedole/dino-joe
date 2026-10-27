// Dino Joe - dostignuća (achievementi): brojači, otključavanje vrsta i šara, prozor sa listom.
// Podaci: %AppData%\DinoJoe\achievements.ini (unlocked.<id>=datum, count.<brojač>=n, last_day=datum).
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DinoJoe
{
    class Achievement
    {
        public string Id;
        public string Counter;   // null = jednokratno
        public int Goal;
        public string Reward;    // "species:4", "pattern:6" ili null
        public string Name { get { return Lang.Gender(Lang.Get("ach." + Id)); } }
        public string Desc { get { return Lang.Gender(Lang.Get("ach." + Id + ".d")); } }
    }

    static class Achievements
    {
        public static readonly List<Achievement> All = new List<Achievement>();
        static readonly Dictionary<string, string> unlocked = new Dictionary<string, string>();
        static readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        static string lastDay = "";

        /// Poziva se kad se nešto otključa (PetForm prikaže oblačić).
        public static event Action<Achievement> Unlocked;

        static void Def(string id, string counter, int goal, string reward)
        {
            Achievement a = new Achievement();
            a.Id = id; a.Counter = counter; a.Goal = goal; a.Reward = reward;
            All.Add(a);
        }

        static Achievements()
        {
            Def("hatch", null, 0, null);
            Def("juvenile", null, 0, null);
            Def("adult", null, 0, "pattern:6");          // zlatna šara
            Def("veteran", null, 0, "species:4");        // Stegosaurus
            Def("days7", "days", 7, "species:4");        // Stegosaurus
            Def("days30", "days", 30, "pattern:7");      // duga
            Def("jokes300", "jokes", 300, "species:5");  // Pterodaktil
            Def("feed50", "feed", 50, null);
            Def("pet100", "pet", 100, null);
            Def("like10", "like", 10, null);
            Def("night", null, 0, null);
            Def("monday", null, 0, null);
            Def("ai", null, 0, null);
            Def("pack", null, 0, null);
            Def("vacation", null, 0, null);
            Def("happy", null, 0, null);
            Def("stylist", null, 0, null);
            Def("newegg", null, 0, null);
            Def("fly", null, 0, null);
            Def("climber", null, 0, null);
            Def("games5", "gamewins", 5, null);
        }

        static string FilePath
        {
            get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id), "achievements.ini"); }
        }

        public static void Load()
        {
            unlocked.Clear(); counts.Clear(); lastDay = "";
            try
            {
                if (!File.Exists(FilePath)) return;
                foreach (string raw in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    int i = raw.IndexOf('=');
                    if (i <= 0 || raw.StartsWith("#")) continue;
                    string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                    int n;
                    if (k.StartsWith("unlocked.")) unlocked[k.Substring(9)] = v;
                    else if (k.StartsWith("count.") && int.TryParse(v, out n)) counts[k.Substring(6)] = n;
                    else if (k == "last_day") lastDay = v;
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                StringBuilder b = new StringBuilder("# Dino Joe - dostignuca\r\n");
                b.Append("last_day=").Append(lastDay).Append("\r\n");
                foreach (KeyValuePair<string, int> kv in counts) b.Append("count.").Append(kv.Key).Append('=').Append(kv.Value).Append("\r\n");
                foreach (KeyValuePair<string, string> kv in unlocked) b.Append("unlocked.").Append(kv.Key).Append('=').Append(kv.Value).Append("\r\n");
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, b.ToString(), new UTF8Encoding(false));
            }
            catch { }
        }

        public static bool Has(string id) { return unlocked.ContainsKey(id); }
        public static int Count(string counter) { int n; return counts.TryGetValue(counter, out n) ? n : 0; }
        public static int UnlockedCount { get { return unlocked.Count; } }
        public static string UnlockedDate(string id) { string d; return unlocked.TryGetValue(id, out d) ? d : null; }

        public static Achievement Find(string id)
        {
            foreach (Achievement a in All) if (a.Id == id) return a;
            return null;
        }

        public static void Unlock(string id)
        {
            if (Has(id)) return;
            Achievement a = Find(id);
            if (a == null) return;
            unlocked[id] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Save();
            if (Unlocked != null) Unlocked(a);
        }

        /// Uvećaj brojač i otključaj sve što je dostiglo cilj.
        public static void Add(string counter, int by)
        {
            counts[counter] = Count(counter) + by;
            bool any = false;
            foreach (Achievement a in All)
                if (a.Counter == counter && !Has(a.Id) && Count(counter) >= a.Goal) { Unlock(a.Id); any = true; }
            if (!any && Count(counter) % 10 == 0) Save();
        }

        /// Jednom dnevno: brojač dana druženja.
        public static void TouchDay()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (today == lastDay) return;
            lastDay = today;
            Add("days", 1);
            Save();
        }

        // ---------------- otključavanje vrsta i šara ----------------
        static Achievement RewardFor(string reward)
        {
            foreach (Achievement a in All) if (a.Reward == reward) return a;
            return null;
        }

        static bool RewardOpen(string reward)
        {
            bool locked = false;
            foreach (Achievement a in All)
                if (a.Reward == reward) { if (Has(a.Id)) return true; locked = true; }
            return !locked;
        }

        public static bool SpeciesOpen(int s) { return RewardOpen("species:" + s); }
        public static bool PatternOpen(int p) { return RewardOpen("pattern:" + p); }

        /// Tekst "otključava se: ..." za zaključanu vrstu/šaru.
        public static string HowToUnlock(string reward)
        {
            List<string> l = new List<string>();
            foreach (Achievement a in All) if (a.Reward == reward && a.Id != "veteran") l.Add(a.Name);
            return string.Format(Lang.Get("ach.howto"), string.Join(" / ", l.ToArray()));
        }

        public static string RewardName(string reward)
        {
            if (reward == null) return null;
            int n = int.Parse(reward.Substring(reward.IndexOf(':') + 1));
            return reward.StartsWith("species:") ? DinoJoe.Species.Name(n) : Skins.PatternName(n);
        }
    }

    // ======================================================================
    //  PROZOR DOSTIGNUĆA
    // ======================================================================
    class AchievementsForm : ThemedForm
    {
        readonly Panel list = new Panel();
        const int RowH = 66;

        public AchievementsForm()
        {
            Text = Profile.Name + " - " + Lang.Get("ach.title");
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(460, 560);
            TopMost = false;
            BackColor = Color.White;

            Label head = new Label();
            head.Font = Theme.Title;
            head.ForeColor = Theme.Text;
            head.Text = Lang.Get("ach.title");
            head.SetBounds(16, 12, 300, 32);
            Controls.Add(head);

            Label sub = new Label();
            sub.ForeColor = Theme.TextMuted;
            sub.Text = string.Format(Lang.Get("ach.progress"), Achievements.UnlockedCount, Achievements.All.Count) +
                       "   " + Lang.Get("life.stage") + " " + Life.StageName(Life.Stage);
            sub.SetBounds(16, 46, 428, 22);
            Controls.Add(sub);

            list.SetBounds(16, 74, 428, 432);
            list.AutoScroll = true;
            list.BackColor = Color.White;
            Controls.Add(list);

            int y = 0;
            // prvo otključana, pa zaključana
            List<Achievement> sorted = new List<Achievement>();
            foreach (Achievement a in Achievements.All) if (Achievements.Has(a.Id)) sorted.Add(a);
            foreach (Achievement a in Achievements.All) if (!Achievements.Has(a.Id)) sorted.Add(a);
            foreach (Achievement a in sorted)
            {
                Row r = new Row(a);
                r.SetBounds(0, y, 404, RowH - 6);
                list.Controls.Add(r);
                y += RowH;
            }

            ModernButton ok = new ModernButton();
            ok.Primary = true;
            ok.Text = "OK";
            ok.SetBounds(344, 518, 100, 30);
            ok.Click += delegate { Close(); };
            Controls.Add(ok);
            AcceptButton = ok;
        }

        class Row : Control
        {
            readonly Achievement a;
            public Row(Achievement a)
            {
                this.a = a;
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                bool on = Achievements.Has(a.Id);
                Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath gp = Theme.Round(r, 8))
                {
                    using (SolidBrush b = new SolidBrush(on ? Theme.AccentSoft : Theme.Page)) g.FillPath(b, gp);
                    using (Pen p = new Pen(on ? Color.FromArgb(180, Theme.Accent) : Theme.Border)) g.DrawPath(p, gp);
                }

                // medalja
                Rectangle m = new Rectangle(10, (Height - 34) / 2, 34, 34);
                if (on)
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(m, Color.FromArgb(255, 222, 110), Color.FromArgb(225, 160, 30), 90f)) g.FillEllipse(b, m);
                    using (Pen p = new Pen(Color.FromArgb(170, 115, 20), 2f)) g.DrawEllipse(p, m);
                    using (Font f = new Font("Segoe UI Symbol", 13f, FontStyle.Bold))
                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (SolidBrush t = new SolidBrush(Color.FromArgb(130, 80, 10)))
                        g.DrawString("★", f, t, m, sf);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(225, 228, 232))) g.FillEllipse(b, m);
                    // katanac
                    using (Pen p = new Pen(Theme.TextMuted, 2f)) g.DrawArc(p, m.X + 11, m.Y + 7, 12, 12, 180, 180);
                    using (SolidBrush b = new SolidBrush(Theme.TextMuted)) g.FillRectangle(b, m.X + 9, m.Y + 14, 16, 12);
                }

                Color tc = on ? Theme.Text : Theme.TextMuted;
                TextRenderer.DrawText(g, a.Name, Theme.Bold, new Point(52, 6), tc, TextFormatFlags.NoPrefix);
                if (on)
                    using (SolidBrush b = new SolidBrush(Theme.TextMuted))
                    {
                        string dt = Achievements.UnlockedDate(a.Id);
                        SizeF ds = g.MeasureString(dt, Font);
                        g.DrawString(dt, Font, b, Width - ds.Width - 8, 7);
                    }
                string d = a.Desc;
                if (!on && a.Counter != null) d += "  (" + Math.Min(Achievements.Count(a.Counter), a.Goal) + "/" + a.Goal + ")";
                if (a.Reward != null) d += "  " + string.Format(Lang.Get("ach.reward"), Achievements.RewardName(a.Reward));
                // TextRenderer (GDI) ima font fallback, pa se i emoji/simboli prikažu ispravno
                TextRenderer.DrawText(g, d, Font, new Rectangle(52, 26, Width - 60, Height - 26), Theme.TextMuted,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.Top);
            }
        }
    }
}

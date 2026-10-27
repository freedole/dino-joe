// Dino Joe - čarobnjak pri prvom pokretanju (i "Podešavanja..." kasnije).
// Koraci: jezik, ko si ti (izlijeganje + pol + ime), izgled, ličnost, paketi, AI + autostart.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DinoJoe
{
    // ======================================================================
    //  ŽIVI PREVIEW (jaje -> dino)
    // ======================================================================
    class PreviewBox : Control
    {
        public readonly Pose Pose = new Pose();
        public bool EggMode = true;
        public Color EggSpots = Color.FromArgb(80, 175, 85);

        readonly Timer timer = new Timer();
        readonly Random rnd = new Random();
        double t, hatchT = -1, happyLeft, nextBlink = 1.5, blinkLeft, bubbleLeft;
        Bitmap buf;

        public PreviewBox()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Pose.State = State.Idle;
            timer.Interval = 33;
            timer.Tick += delegate { Tick(); };
            timer.Start();
        }

        public void Hatch()
        {
            if (!EggMode || hatchT >= 0) return;
            hatchT = 0;
        }

        public void ShowDino()
        {
            EggMode = false; hatchT = -1;
        }

        public void Happy(double sec) { happyLeft = sec; Pose.State = State.Happy; Pose.HeartsT = 0; }

        public void Say(string text, double sec) { Pose.Bubble = text; Pose.BubbleAlpha = 1; bubbleLeft = sec; }

        void Tick()
        {
            const double dt = 0.033;
            t += dt;
            Pose.T = t;
            if (hatchT >= 0)
            {
                hatchT += dt;
                if (hatchT > 2.0) { EggMode = false; hatchT = -1; Happy(2.5); }
            }
            if (happyLeft > 0)
            {
                happyLeft -= dt;
                if (Pose.HeartsT >= 0) Pose.HeartsT = Math.Min(1.01, Pose.HeartsT + dt / 1.6);
                if (happyLeft <= 0) { Pose.State = State.Idle; Pose.HeartsT = -1; }
            }
            nextBlink -= dt;
            if (nextBlink <= 0) { Pose.Blink = true; blinkLeft = 0.14; nextBlink = 2 + rnd.NextDouble() * 3; }
            if (blinkLeft > 0) { blinkLeft -= dt; if (blinkLeft <= 0) Pose.Blink = false; }
            if (bubbleLeft > 0)
            {
                bubbleLeft -= dt;
                Pose.BubbleAlpha = Math.Min(1, bubbleLeft / 0.4);
                if (bubbleLeft <= 0) Pose.Bubble = null;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            if (r.Width < 10 || r.Height < 10) return;
            using (LinearGradientBrush bg = new LinearGradientBrush(r, Color.FromArgb(238, 246, 255), Color.FromArgb(214, 232, 220), 90f))
                g.FillRectangle(bg, r);
            using (SolidBrush ground = new SolidBrush(Color.FromArgb(40, 60, 120, 60)))
                g.FillEllipse(ground, r.Width * 0.1f, r.Height - 34, r.Width * 0.8f, 30);

            if (EggMode) { DrawEgg(g, r); return; }

            float k = Math.Min(r.Width / Painter.CanvasW, (r.Height - 6) / Painter.CanvasH);
            int bw = (int)Math.Ceiling(Painter.CanvasW * k), bh = (int)Math.Ceiling(Painter.CanvasH * k);
            if (buf == null || buf.Width != bw || buf.Height != bh)
            {
                if (buf != null) buf.Dispose();
                buf = new Bitmap(bw, bh, PixelFormat.Format32bppArgb);
            }
            using (Graphics bg2 = Graphics.FromImage(buf)) Painter.Render(bg2, k, Pose);
            g.DrawImage(buf, (r.Width - bw) / 2, r.Height - bh - 6);
        }

        void DrawEgg(Graphics g, Rectangle r)
        {
            float w = 92, h = 116;
            float cx = r.Width / 2f, by = r.Height - 22;
            double wob = hatchT >= 0 ? Math.Sin(hatchT * 28) * (6 + hatchT * 6) : Math.Sin(t * 3) * 3;
            GraphicsState gs = g.Save();
            g.TranslateTransform(cx, by);
            g.RotateTransform((float)wob);

            using (GraphicsPath egg = new GraphicsPath())
            {
                egg.AddBezier(0, -h, w * 0.62f, -h, w * 0.58f, 0, 0, 0);
                egg.AddBezier(0, 0, -w * 0.58f, 0, -w * 0.62f, -h, 0, -h);
                egg.CloseFigure();
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(-w / 2, -h), new PointF(w / 2, 0), Color.FromArgb(255, 252, 238), Color.FromArgb(232, 220, 190)))
                    g.FillPath(b, egg);
                Region old = g.Clip;
                g.SetClip(egg, CombineMode.Intersect);
                using (SolidBrush sp = new SolidBrush(Color.FromArgb(170, EggSpots)))
                {
                    g.FillEllipse(sp, -30, -90, 18, 14);
                    g.FillEllipse(sp, 10, -70, 22, 16);
                    g.FillEllipse(sp, -22, -45, 14, 11);
                    g.FillEllipse(sp, 18, -30, 12, 10);
                    g.FillEllipse(sp, -6, -108, 10, 8);
                }
                using (SolidBrush hl = new SolidBrush(Color.FromArgb(110, 255, 255, 255)))
                    g.FillEllipse(hl, -24, -96, 16, 30);
                g.Clip = old;
                using (Pen o = new Pen(Color.FromArgb(150, 120, 80), 2.5f)) g.DrawPath(o, egg);

                if (hatchT > 0.9)
                {
                    using (Pen c = new Pen(Color.FromArgb(110, 80, 50), 2.2f))
                    {
                        c.LineJoin = LineJoin.Round;
                        float y = -h * 0.55f;
                        g.DrawLines(c, new PointF[] {
                            new PointF(-w * 0.45f, y), new PointF(-w * 0.3f, y - 9), new PointF(-w * 0.15f, y + 4),
                            new PointF(0, y - 10), new PointF(w * 0.15f, y + 3), new PointF(w * 0.3f, y - 8), new PointF(w * 0.45f, y) });
                    }
                }
            }
            g.Restore(gs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Stop(); timer.Dispose(); if (buf != null) buf.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // ======================================================================
    //  ČAROBNJAK
    // ======================================================================
    class SetupWizard : ThemedForm
    {
        readonly bool firstRun;
        readonly AiConfig ai;
        bool committed;

        // radne vrijednosti
        string langCode;
        bool female;
        string customName;
        string personality;
        int color1, color2;
        bool glasses;
        int species, pattern, accessory;
        string packs;
        bool packsTouched;
        bool showHidden;
        bool autostart;

        int step;
        const int Steps = 6;

        PreviewBox preview;
        Label lblTitle, lblSub, lblStep;
        Panel content;
        ModernButton btBack, btNext;
        readonly Random rnd = new Random();
        static readonly Font FBase = new Font("Segoe UI", 10f);

        public SetupWizard(bool firstRun, AiConfig ai)
        {
            this.firstRun = firstRun;
            this.ai = ai;
            langCode = Settings.LangCode;
            female = Settings.Female;
            customName = Settings.CustomName;
            personality = Settings.PersonalityId;
            color1 = Settings.Color1;
            color2 = Settings.Color2;
            glasses = Settings.Glasses;
            species = Settings.Species;
            pattern = Settings.Pattern;
            accessory = Settings.Accessory;
            packs = Settings.Packs;
            autostart = firstRun ? true : Settings.IsAutostart();

            Font = FBase;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 500);
            BackColor = Color.White;
            try { Icon = Painter.MakeIcon(); } catch { }
            TopMost = false;   // ne pokrivaj browser (npr. stranicu za API ključ)

            preview = new PreviewBox();
            preview.SetBounds(0, 0, 280, 500);
            preview.EggMode = firstRun;
            preview.Pose.Female = female;
            preview.Pose.Glasses = glasses;
            Controls.Add(preview);

            lblTitle = new Label();
            lblTitle.Font = new Font("Segoe UI Semibold", 17f);
            lblTitle.ForeColor = Color.FromArgb(30, 70, 40);
            lblTitle.SetBounds(300, 18, 440, 36);
            Controls.Add(lblTitle);

            lblSub = new Label();
            lblSub.ForeColor = Color.FromArgb(90, 90, 90);
            lblSub.SetBounds(302, 56, 440, 40);
            Controls.Add(lblSub);

            content = new Panel();
            content.SetBounds(300, 100, 445, 330);
            Controls.Add(content);

            lblStep = new Label();
            lblStep.ForeColor = Color.Gray;
            lblStep.SetBounds(302, 456, 160, 24);
            Controls.Add(lblStep);

            btBack = new ModernButton();
            btBack.SetBounds(500, 448, 110, 34);
            btBack.Click += delegate { Go(step - 1); };
            Controls.Add(btBack);

            btNext = new ModernButton();
            btNext.Primary = true;
            btNext.SetBounds(625, 448, 120, 34);
            btNext.Font = new Font("Segoe UI Semibold", 10f);
            btNext.Click += delegate { if (step == Steps - 1) Finish(); else Go(step + 1); };
            Controls.Add(btNext);
            AcceptButton = btNext;

            ApplyLookLive();
            Go(0);
        }

        // ---------------- navigacija ----------------
        void Go(int s)
        {
            if (s < 0 || s >= Steps) return;
            if (s == 1 && preview.EggMode) preview.Hatch();
            if (s >= 2 && preview.EggMode) preview.ShowDino();
            step = s;
            Lang.Current = Lang.FromCode(langCode);
            Text = Lang.Get(firstRun ? "wiz.title" : "wiz.settings");
            lblStep.Text = string.Format(Lang.Get("wiz.step"), step + 1, Steps);
            btBack.Text = Lang.Get("wiz.back");
            btBack.Enabled = step > 0;
            btNext.Text = Lang.Get(step == Steps - 1 ? "wiz.finish" : "wiz.next");

            content.SuspendLayout();
            foreach (Control c in content.Controls) c.Dispose();
            content.Controls.Clear();
            switch (step)
            {
                case 0: StepLang(); break;
                case 1: StepWho(); break;
                case 2: StepLook(); break;
                case 3: StepPersonality(); break;
                case 4: StepPacks(); break;
                case 5: StepAi(); break;
            }
            content.ResumeLayout();
        }

        void Header(string title, string sub) { lblTitle.Text = Lang.Get(title); lblSub.Text = Lang.Get(sub); }

        RadioButton Radio(string text, int x, int y, int w, bool on)
        {
            RadioButton rb = new RadioButton();
            rb.Text = text; rb.Checked = on;
            rb.SetBounds(x, y, w, 30);
            content.Controls.Add(rb);
            return rb;
        }

        Label Lbl(string text, int x, int y, int w, int h)
        {
            Label l = new Label();
            l.Text = text; l.SetBounds(x, y, w, h);
            content.Controls.Add(l);
            return l;
        }

        string CurrentName()
        {
            return customName.Trim().Length > 0 ? customName.Trim() : Settings.DefaultName(female, Lang.FromCode(langCode));
        }

        // ---------------- 1. jezik ----------------
        void StepLang()
        {
            Header("s.lang", "s.lang.sub");
            for (int n = 0; n < Lang.Order.Length; n++)
            {
                int i = Lang.Order[n];
                string code = Lang.Codes[i];
                RadioButton rb = Radio(Lang.Names[i], 10, 10 + n * 44, 400, langCode == code);
                rb.Font = new Font("Segoe UI", 12f);
                rb.CheckedChanged += delegate(object o, EventArgs e)
                {
                    if (!((RadioButton)o).Checked) return;
                    langCode = code;
                    BeginInvoke((MethodInvoker)delegate { Go(0); });
                };
            }
        }

        // ---------------- 2. ko si ti ----------------
        void StepWho()
        {
            Header("s.who", "s.who.sub");
            int L = Lang.FromCode(langCode);
            RadioButton m = Radio(Lang.Get("s.who.male") + "  -  " + Settings.DefaultName(false, L), 10, 10, 400, !female);
            RadioButton f = Radio(Lang.Get("s.who.female") + "  -  " + Settings.DefaultName(true, L), 10, 46, 400, female);
            m.Font = f.Font = new Font("Segoe UI", 12f);
            EventHandler h = delegate
            {
                female = f.Checked;
                preview.Pose.Female = female;
                preview.Happy(1.2);
            };
            m.CheckedChanged += h; f.CheckedChanged += h;

            Lbl(Lang.Get("s.who.name"), 12, 110, 420, 24);
            TextBox tb = new TextBox();
            tb.SetBounds(12, 136, 300, 28);
            tb.MaxLength = 30;
            tb.Text = customName;
            tb.TextChanged += delegate { customName = tb.Text; };
            content.Controls.Add(tb);
        }

        // ---------------- 3. izgled ----------------
        ComboBox cbColor1, cbColor2, cbSpecies, cbPattern, cbAccessory;
        CheckBox chGlasses;

        void StepLook()
        {
            Header("s.look", "s.look.sub");

            Lbl(Lang.Get("s.look.species"), 12, 12, 120, 24);
            ComboBox sp = new ComboBox();
            sp.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < DinoJoe.Species.Count; i++)
                sp.Items.Add(DinoJoe.Species.Name(i) + (Achievements.SpeciesOpen(i) ? "" : "  " + Lang.Get("s.look.locked")));
            if (!Achievements.SpeciesOpen(species)) species = 0;
            sp.SelectedIndex = Math.Max(0, Math.Min(DinoJoe.Species.Count - 1, species));
            sp.SetBounds(140, 9, 240, 28);
            sp.SelectedIndexChanged += delegate
            {
                if (!Achievements.SpeciesOpen(sp.SelectedIndex))
                {
                    ShowLock(Achievements.HowToUnlock("species:" + sp.SelectedIndex));
                    sp.SelectedIndex = species;
                    return;
                }
                ShowLock(null);
                species = sp.SelectedIndex; ApplyLookLive(); preview.Happy(1.2);
            };
            content.Controls.Add(sp);
            cbSpecies = sp;

            Lbl(Lang.Get("s.look.color"), 12, 52, 120, 24);
            cbColor1 = ColorCombo(false);
            cbColor1.SetBounds(140, 49, 240, 28);
            cbColor1.SelectedIndex = Math.Max(0, Math.Min(Palette.All.Count - 1, color1));
            cbColor1.SelectedIndexChanged += delegate { color1 = cbColor1.SelectedIndex; ApplyLookLive(); };

            Lbl(Lang.Get("s.look.color2"), 12, 92, 120, 24);
            cbColor2 = ColorCombo(true);
            cbColor2.SetBounds(140, 89, 240, 28);
            cbColor2.SelectedIndex = color2 < 0 ? 0 : Math.Min(Palette.All.Count, color2 + 1);
            cbColor2.SelectedIndexChanged += delegate { color2 = cbColor2.SelectedIndex - 1; ApplyLookLive(); };

            Lbl(Lang.Get("s.look.pattern"), 12, 132, 120, 24);
            cbPattern = new ComboBox();
            cbPattern.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < Skins.PatternCount; i++)
                cbPattern.Items.Add(Skins.PatternName(i) + (Achievements.PatternOpen(i) ? "" : "  " + Lang.Get("s.look.locked")));
            if (!Achievements.PatternOpen(pattern)) pattern = 0;
            cbPattern.SelectedIndex = Math.Max(0, Math.Min(Skins.PatternCount - 1, pattern));
            cbPattern.SetBounds(140, 129, 240, 28);
            cbPattern.SelectedIndexChanged += delegate
            {
                if (!Achievements.PatternOpen(cbPattern.SelectedIndex))
                {
                    ShowLock(Achievements.HowToUnlock("pattern:" + cbPattern.SelectedIndex));
                    cbPattern.SelectedIndex = pattern;
                    return;
                }
                ShowLock(null);
                pattern = cbPattern.SelectedIndex; ApplyLookLive();
            };
            content.Controls.Add(cbPattern);

            Lbl(Lang.Get("s.look.accessory"), 12, 172, 120, 24);
            cbAccessory = new ComboBox();
            cbAccessory.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < Skins.AccessoryCount; i++) cbAccessory.Items.Add(Skins.AccessoryName(i));
            cbAccessory.SelectedIndex = Math.Max(0, Math.Min(Skins.AccessoryCount - 1, accessory));
            cbAccessory.SetBounds(140, 169, 240, 28);
            cbAccessory.SelectedIndexChanged += delegate { accessory = cbAccessory.SelectedIndex; ApplyLookLive(); preview.Happy(1); };
            content.Controls.Add(cbAccessory);

            chGlasses = new CheckBox();
            chGlasses.Text = Lang.Get("s.look.glasses");
            chGlasses.Checked = glasses;
            chGlasses.SetBounds(140, 206, 200, 28);
            chGlasses.CheckedChanged += delegate { glasses = chGlasses.Checked; ApplyLookLive(); };
            content.Controls.Add(chGlasses);

            Button egg = new ModernButton();
            egg.Text = Lang.Get("s.look.egg");
            egg.SetBounds(140, 244, 240, 38);
            egg.Click += delegate
            {
                int rs; do rs = rnd.Next(DinoJoe.Species.Count); while (!Achievements.SpeciesOpen(rs));
                cbSpecies.SelectedIndex = rs;
                cbColor1.SelectedIndex = rnd.Next(Palette.All.Count);
                cbColor2.SelectedIndex = rnd.Next(3) == 0 ? 1 + rnd.Next(Palette.All.Count) : 0;
                chGlasses.Checked = rnd.Next(4) == 0;
                int rp; do rp = rnd.Next(10) < 4 ? 0 : rnd.Next(Skins.PatternCount); while (!Achievements.PatternOpen(rp));
                cbPattern.SelectedIndex = rp;
                cbAccessory.SelectedIndex = rnd.Next(10) < 4 ? 0 : rnd.Next(Skins.AccessoryCount);
                preview.Happy(1.5);
            };
            content.Controls.Add(egg);

            lblLock = new Label();
            lblLock.ForeColor = Color.FromArgb(150, 95, 20);
            lblLock.SetBounds(140, 288, 240, 40);
            lblLock.Visible = false;
            content.Controls.Add(lblLock);
        }

        Label lblLock;

        void ShowLock(string text)
        {
            if (lblLock == null) return;
            lblLock.Text = "🔒 " + (text ?? "");
            lblLock.Visible = text != null;
        }

        ComboBox ColorCombo(bool withAuto)
        {
            ComboBox cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.DrawMode = DrawMode.OwnerDrawFixed;
            cb.ItemHeight = 22;
            if (withAuto) cb.Items.Add(Lang.Get("s.look.auto"));
            foreach (Palette p in Palette.All) cb.Items.Add(p);
            cb.DrawItem += delegate(object o, DrawItemEventArgs e)
            {
                e.DrawBackground();
                if (e.Index < 0) return;
                object it = cb.Items[e.Index];
                Palette p = it as Palette;
                int x = e.Bounds.X + 4, y = e.Bounds.Y + 3, s = e.Bounds.Height - 6;
                if (p != null)
                {
                    using (SolidBrush b = new SolidBrush(p.Primary)) e.Graphics.FillEllipse(b, x, y, s, s);
                    using (Pen pn = new Pen(Color.FromArgb(90, 0, 0, 0))) e.Graphics.DrawEllipse(pn, x, y, s, s);
                }
                using (SolidBrush tb = new SolidBrush(e.ForeColor))
                    e.Graphics.DrawString(it.ToString(), e.Font, tb, x + s + 8, e.Bounds.Y + 2);
                e.DrawFocusRectangle();
            };
            content.Controls.Add(cb);
            return cb;
        }

        void ApplyLookLive()
        {
            Settings.ApplyColors(color1, color2);
            preview.EggSpots = Palette.Get(color1).Primary;
            preview.Pose.Glasses = glasses;
            preview.Pose.Female = female;
            preview.Pose.Species = species;
            preview.Pose.Pattern = pattern;
            preview.Pose.Accessory = accessory;
        }

        // ---------------- 4. ličnost ----------------
        void StepPersonality()
        {
            Header("s.pers", "s.pers.sub");
            Label sample = Lbl("", 12, 250, 420, 70);
            sample.Font = new Font("Segoe UI", 10f, FontStyle.Italic);
            sample.ForeColor = Color.FromArgb(40, 90, 50);
            List<Personality> vis = new List<Personality>();
            foreach (Personality x in Personality.All)
                if (showHidden || x.Id == personality || !Settings.IsHidden(x.Id)) vis.Add(x);
            for (int i = 0; i < vis.Count; i++)
            {
                Personality p = vis[i];
                int col = i < 5 ? 0 : 1, row = i < 5 ? i : i - 5;
                bool hid = Settings.IsHidden(p.Id);
                RadioButton rb = Radio(p.Name + (hid ? "  " + Lang.Get("pers.hiddenmark") : ""), 10 + col * 215, 4 + row * 40, 210, personality == p.Id);
                if (hid) rb.ForeColor = Theme.TextMuted;
                ContextMenuStrip cm = new ContextMenuStrip();
                cm.Items.Add(Lang.Get(hid ? "pers.show" : "pers.hide"), null, delegate
                {
                    if (!hid && p.Id == personality) return;     // aktivna se ne sakriva
                    Settings.SetHidden(p.Id, !hid);
                    BeginInvoke((MethodInvoker)delegate { Go(step); });
                });
                rb.ContextMenuStrip = cm;
                if (p.Id == "tamagotchi") rb.Font = new Font("Segoe UI Semibold", 10f);
                rb.CheckedChanged += delegate(object o, EventArgs e)
                {
                    if (!((RadioButton)o).Checked) return;
                    personality = p.Id;
                    sample.Text = "„" + Lang.GenderFor(p.Sample, female) + "“";
                    preview.Say(Lang.GenderFor(p.Sample, female), 4);
                };
                if (personality == p.Id) sample.Text = "„" + Lang.GenderFor(p.Sample, female) + "“";
            }
            preview.Say(Lang.GenderFor(Personality.Find(personality).Sample, female), 4);

            CheckBox sh = new CheckBox();
            sh.Text = Lang.Get("pers.showhidden");
            sh.Checked = showHidden;
            sh.SetBounds(12, 216, 420, 24);
            sh.ForeColor = Theme.TextMuted;
            sh.CheckedChanged += delegate { showHidden = sh.Checked; BeginInvoke((MethodInvoker)delegate { Go(step); }); };
            content.Controls.Add(sh);
        }

        // ---------------- 5. paketi ----------------
        void StepPacks()
        {
            Header("s.packs", "s.packs.sub");
            List<Pack> list = new List<Pack>();
            foreach (Pack x in Packs.All()) if (!x.IsPersonality && Packs.LangCompatible(x.Lang)) list.Add(x);
            if (!packsTouched) packs = Packs.DefaultPacks();
            if (list.Count == 0) { Lbl(Lang.Get("s.packs.none"), 12, 12, 400, 24).ForeColor = Color.Gray; return; }
            ModernCheckList cl = new ModernCheckList();
            cl.SetBounds(10, 8, 425, 300);
            foreach (Pack pi in list)
            {
                bool on = ("," + packs + ",").Contains("," + pi.Id + ",");
                cl.AddItem(pi.Name + "  (" + pi.JokeCount + ")" + (pi.Desc.Length > 0 ? "  -  " + pi.Desc : ""), on);
            }
            cl.CheckChanged += delegate
            {
                List<string> ids = new List<string>();
                for (int i = 0; i < list.Count; i++) if (cl.GetItemChecked(i)) ids.Add(list[i].Id);
                packs = string.Join(",", ids.ToArray());
                packsTouched = true;
            };
            content.Controls.Add(cl);
        }

        // ---------------- 6. AI + autostart ----------------
        void StepAi()
        {
            Header("s.ai", "");
            lblSub.Text = "";
            Lbl(string.Format(Lang.Get("s.ai.text"), CurrentName()), 12, 0, 425, 190);

            Label status = Lbl(ai != null && ai.Ready ? Lang.Get("s.ai.ok") : "", 12, 246, 420, 24);
            status.ForeColor = Color.FromArgb(40, 120, 60);

            ModernButton connect = new ModernButton();
            connect.Text = Lang.Get("s.ai.connect");
            connect.Primary = true;
            connect.SetBounds(12, 200, 150, 36);
            connect.Click += delegate
            {
                if (ai == null) return;
                using (AiSettingsForm f = new AiSettingsForm(ai)) { f.ShowDialog(this); }
                ai.Prompted = true; ai.Save();
                status.Text = ai.Ready ? Lang.Get("s.ai.ok") : Lang.Get("s.ai.off");
                if (ai.Ready) preview.Happy(2);
            };
            content.Controls.Add(connect);

            Button later = new ModernButton();
            later.Text = Lang.Get("s.ai.later");
            later.SetBounds(172, 200, 150, 36);
            later.Click += delegate
            {
                if (ai != null) { ai.Prompted = true; ai.Save(); }
                status.Text = Lang.Get("s.ai.off");
            };
            content.Controls.Add(later);

            if (Packaged.IsPackaged)
            {
                Label st = Lbl(Lang.Get("s.autostart.store"), 12, 290, 420, 44);
                st.ForeColor = Theme.TextMuted;
                return;
            }
            CheckBox auto = new CheckBox();
            auto.Text = Lang.Get("s.autostart");
            auto.Checked = autostart;
            auto.SetBounds(12, 290, 300, 28);
            auto.CheckedChanged += delegate { autostart = auto.Checked; };
            content.Controls.Add(auto);
        }

        // ---------------- kraj ----------------
        void Finish()
        {
            Commit();
            DialogResult = DialogResult.OK;
            Close();
        }

        void Commit()
        {
            if (committed) return;
            committed = true;
            Settings.SetupDone = true;
            Settings.LangCode = langCode;
            Settings.Female = female;
            Settings.CustomName = customName.Trim();
            Settings.PersonalityId = personality;
            Settings.Color1 = color1;
            Settings.Color2 = color2;
            Settings.Glasses = glasses;
            Settings.Species = species;
            Settings.Pattern = pattern;
            Settings.Accessory = accessory;
            Settings.Packs = packs;
            Settings.Save();
            // prvo pokretanje: izbor paketa i ličnosti postaje prvi profil
            if (firstRun && Packs.LoadProfiles().Count == 0) Packs.SaveCurrentAs(Lang.Get("prof.default"));
            Settings.SetAutostart(autostart);
            Settings.Apply();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // prvo pokretanje: zatvaranje = prihvati šta je izabrano (ne blokiramo korisnika)
            if (!committed)
            {
                if (firstRun) Commit();
                else Settings.Apply();   // otkaži: vrati stare boje
            }
            base.OnFormClosing(e);
        }
    }
}

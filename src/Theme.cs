// Dino Joe - moderna tema za sve prozore (flat dugmad sa zaobljenim ivicama, owner-draw liste,
// Windows 11 zaobljeni uglovi i boja naslovne trake). Forme nasljeđuju ThemedForm.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DinoJoe
{
    static class Theme
    {
        public static readonly Color Accent = Color.FromArgb(34, 139, 84);
        public static readonly Color AccentHover = Color.FromArgb(28, 120, 72);
        public static readonly Color AccentDown = Color.FromArgb(22, 100, 60);
        public static readonly Color AccentSoft = Color.FromArgb(226, 243, 233);
        public static readonly Color Surface = Color.White;
        public static readonly Color Page = Color.FromArgb(247, 248, 250);
        public static readonly Color Border = Color.FromArgb(222, 226, 231);
        public static readonly Color Text = Color.FromArgb(31, 35, 40);
        public static readonly Color TextMuted = Color.FromArgb(101, 109, 118);
        public static readonly Color BtnBg = Color.FromArgb(236, 239, 242);
        public static readonly Color BtnHover = Color.FromArgb(224, 228, 233);
        public static readonly Color BtnDown = Color.FromArgb(210, 215, 221);
        public static readonly Color Danger = Color.FromArgb(207, 34, 46);

        public static readonly Font Body = new Font("Segoe UI", 9.75f);
        public static readonly Font Bold = new Font("Segoe UI Semibold", 9.75f);
        public static readonly Font Title = new Font("Segoe UI Semibold", 15f);

        // ---------------- Windows 11: zaobljeni uglovi, svijetla naslovna traka ----------------
        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        public static void Window(Form f)
        {
            try
            {
                int round = 2;                         // DWMWCP_ROUND
                DwmSetWindowAttribute(f.Handle, 33, ref round, 4);
                int caption = ColorTranslator.ToWin32(Surface);
                DwmSetWindowAttribute(f.Handle, 35, ref caption, 4);   // DWMWA_CAPTION_COLOR
                int border = ColorTranslator.ToWin32(Border);
                DwmSetWindowAttribute(f.Handle, 34, ref border, 4);    // DWMWA_BORDER_COLOR
            }
            catch { }   // Windows 10 / Linux: ignoriše se
        }

        public static GraphicsPath Round(RectangleF r, float radius)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ---------------- primjena na kontrole ----------------
        public static void Apply(Control root, IButtonControl primary)
        {
            foreach (Control c in root.Controls)
            {
                ModernButton mb = c as ModernButton;
                if (mb != null) { if (primary == mb) mb.Primary = true; }
                else if (c is ComboBox) ((ComboBox)c).FlatStyle = FlatStyle.Flat;
                else if (c is TextBox) { TextBox t = (TextBox)c; if (t.BorderStyle == BorderStyle.Fixed3D) t.BorderStyle = BorderStyle.FixedSingle; }
                else if (c is CheckBox) { ((CheckBox)c).FlatStyle = FlatStyle.System; ((CheckBox)c).ForeColor = Text; }
                else if (c is RadioButton) { ((RadioButton)c).FlatStyle = FlatStyle.System; ((RadioButton)c).ForeColor = Text; }
                else if (c is ListView) ModernList((ListView)c);
                if (c.Controls.Count > 0) Apply(c, primary);
            }
        }

        // ---------------- ListView: owner-draw redovi, header i čekboksovi ----------------
        static readonly HashSet<ListView> styled = new HashSet<ListView>();

        public static void ModernList(ListView lv)
        {
            if (styled.Contains(lv)) return;
            styled.Add(lv);
            lv.OwnerDraw = true;
            lv.BorderStyle = BorderStyle.None;
            lv.BackColor = Surface;
            lv.Font = Body;
            ImageList rowH = new ImageList();
            rowH.ImageSize = new Size(1, 32);          // visina reda
            lv.SmallImageList = rowH;

            lv.DrawColumnHeader += delegate(object o, DrawListViewColumnHeaderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Page)) e.Graphics.FillRectangle(b, e.Bounds);
                using (Pen p = new Pen(Border)) e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                Rectangle r = e.Bounds; r.Inflate(-8, 0);
                if (e.ColumnIndex == 0 && lv.CheckBoxes) { r.X += 24; r.Width -= 24; }
                TextFormatFlags fl = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    (e.Header.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, Bold, r, TextMuted, fl);
            };
            lv.DrawItem += delegate(object o, DrawListViewItemEventArgs e) { };
            lv.DrawSubItem += delegate(object o, DrawListViewSubItemEventArgs e)
            {
                Graphics g = e.Graphics;
                bool sel = e.Item.Selected;
                Color bg = sel ? AccentSoft : (e.ItemIndex % 2 == 0 ? Surface : Color.FromArgb(251, 251, 252));
                using (SolidBrush b = new SolidBrush(bg)) g.FillRectangle(b, e.Bounds);
                Rectangle r = e.Bounds; r.Inflate(-8, 0);
                if (e.ColumnIndex == 0 && lv.CheckBoxes)
                {
                    DrawCheck(g, new Rectangle(e.Bounds.X + 8, e.Bounds.Y + (e.Bounds.Height - 16) / 2, 16, 16), e.Item.Checked);
                    r.X += 24; r.Width -= 24;
                }
                TextFormatFlags fl = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    (lv.Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left);
                TextRenderer.DrawText(g, e.SubItem.Text, e.ColumnIndex == 0 ? Bold : Body, r, e.ColumnIndex == 0 ? Text : TextMuted, fl);
            };
            // klik na naš čekboks
            lv.MouseClick += delegate(object o, MouseEventArgs e)
            {
                if (!lv.CheckBoxes) return;
                ListViewHitTestInfo hit = lv.HitTest(e.Location);
                if (hit.Item != null && e.X < hit.Item.Bounds.X + 30 && e.X > hit.Item.Bounds.X + 2)
                {
                    // nativni state-image klik već mijenja stanje kad je X u njegovoj zoni; ovdje pokrivamo ostatak
                    if (e.X > hit.Item.Bounds.X + 20) hit.Item.Checked = !hit.Item.Checked;
                }
            };
            lv.ItemChecked += delegate { lv.Invalidate(); };
        }

        public static void DrawCheck(Graphics g, Rectangle r, bool on)
        {
            SmoothingMode sm = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = Round(r, 4))
            {
                if (on)
                {
                    using (SolidBrush b = new SolidBrush(Accent)) g.FillPath(b, p);
                    using (Pen pen = new Pen(Color.White, 2f))
                    {
                        pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; pen.LineJoin = LineJoin.Round;
                        g.DrawLines(pen, new PointF[] { new PointF(r.X + 4, r.Y + 8.5f), new PointF(r.X + 7, r.Y + 11.5f), new PointF(r.X + 12, r.Y + 5) });
                    }
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Surface)) g.FillPath(b, p);
                    using (Pen pen = new Pen(Color.FromArgb(160, 166, 174), 1.4f)) g.DrawPath(pen, p);
                }
            }
            g.SmoothingMode = sm;
        }
    }

    // ======================================================================
    //  BAZNA FORMA SA TEMOM
    // ======================================================================
    class ThemedForm : Form
    {
        public ThemedForm()
        {
            Font = Theme.Body;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
        }

        protected override void OnLoad(EventArgs e)
        {
            Theme.Apply(this, AcceptButton);
            base.OnLoad(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.Window(this);
        }
    }

    // ======================================================================
    //  MODERNO DUGME
    // ======================================================================
    class ModernButton : Button
    {
        bool hover, down;
        public bool Primary;

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Theme.Bold;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parent = Parent != null ? Parent.BackColor : Theme.Surface;
            using (SolidBrush pb = new SolidBrush(parent)) g.FillRectangle(pb, ClientRectangle);

            Color bg, fg;
            if (!Enabled) { bg = Color.FromArgb(243, 244, 246); fg = Color.FromArgb(170, 175, 182); }
            else if (Primary) { bg = down ? Theme.AccentDown : (hover ? Theme.AccentHover : Theme.Accent); fg = Color.White; }
            else { bg = down ? Theme.BtnDown : (hover ? Theme.BtnHover : Theme.BtnBg); fg = Theme.Text; }

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (GraphicsPath p = Theme.Round(r, 6))
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            if (Focused && ShowFocusCues && Enabled)
                using (GraphicsPath p = Theme.Round(new RectangleF(1.5f, 1.5f, Width - 3.5f, Height - 3.5f), 5))
                using (Pen pen = new Pen(Color.FromArgb(120, Primary ? Color.White : Theme.Accent), 1.2f)) g.DrawPath(pen, p);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    // ======================================================================
    //  LISTA SA ČEKBOKSOVIMA (zamjena za CheckedListBox)
    // ======================================================================
    class ModernCheckList : ListBox
    {
        readonly List<bool> checks = new List<bool>();
        public event EventHandler CheckChanged;

        public ModernCheckList()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 32;
            BorderStyle = BorderStyle.None;
            Font = Theme.Body;
            IntegralHeight = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void AddItem(string text, bool on) { Items.Add(text); checks.Add(on); }
        public void ClearItems() { Items.Clear(); checks.Clear(); }
        public bool GetItemChecked(int i) { return i >= 0 && i < checks.Count && checks[i]; }

        public void SetItemChecked(int i, bool on)
        {
            if (i < 0 || i >= checks.Count || checks[i] == on) return;
            checks[i] = on;
            Invalidate(GetItemRectangle(i));
            if (CheckChanged != null) CheckChanged(this, EventArgs.Empty);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            Graphics g = e.Graphics;
            bool sel = (e.State & DrawItemState.Selected) != 0;
            using (SolidBrush b = new SolidBrush(sel ? Theme.AccentSoft : (e.Index % 2 == 0 ? Theme.Surface : Color.FromArgb(251, 251, 252))))
                g.FillRectangle(b, e.Bounds);
            Theme.DrawCheck(g, new Rectangle(e.Bounds.X + 8, e.Bounds.Y + (e.Bounds.Height - 16) / 2, 16, 16), GetItemChecked(e.Index));
            Rectangle r = new Rectangle(e.Bounds.X + 34, e.Bounds.Y, e.Bounds.Width - 40, e.Bounds.Height);
            TextRenderer.DrawText(g, Items[e.Index].ToString(), Font, r, Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int i = IndexFromPoint(e.Location);
            if (i >= 0) SetItemChecked(i, !GetItemChecked(i));
            base.OnMouseClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && SelectedIndex >= 0) { SetItemChecked(SelectedIndex, !GetItemChecked(SelectedIndex)); e.Handled = true; }
            base.OnKeyDown(e);
        }
    }
}

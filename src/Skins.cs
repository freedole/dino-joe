// Dino Joe - šare (preko tijela) i dodaci (na glavi / vratu) za sve vrste.
// Šare se crtaju klipovane na siluetu tijela, prije stomaka; dodaci u lokalnim koordinatama glave (56x40).
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DinoJoe
{
    static class Skins
    {
        public const int PatternCount = 8;      // 0 = bez šare; 6 i 7 se otključavaju dostignućima
        public const int AccessoryCount = 13;   // 0 = bez dodatka; 11, 12 sezonski (biraju se i ručno)
        public static readonly string[] PatternKeys = { "none", "stripes", "dots", "gradient", "camo", "galaxy", "gold", "rainbow" };
        public static readonly string[] AccessoryKeys = { "none", "beanie", "tophat", "tie", "headphones", "crown", "helmet", "scarf", "flower", "mustache", "bowtie", "witchhat", "santahat" };

        public static string PatternName(int i) { return Lang.Get("pat." + PatternKeys[Math.Max(0, Math.Min(PatternCount - 1, i))]); }
        public static string AccessoryName(int i) { return Lang.Get("acc." + AccessoryKeys[Math.Max(0, Math.Min(AccessoryCount - 1, i))]); }

        /// Dodaci koji pokrivaju vrh glave (tada se mašna ne crta).
        public static bool CoversHead(int acc) { return acc == 1 || acc == 2 || acc == 5 || acc == 6 || acc == 4 || acc == 11 || acc == 12; }

        /// Sezonski dodatak za danas (0 = nema): Noć vještica 24.10-1.11, Božić/Nova godina 15.12-7.1.
        public static int SeasonalAccessory(DateTime d)
        {
            if ((d.Month == 10 && d.Day >= 24) || (d.Month == 11 && d.Day <= 1)) return 11;
            if ((d.Month == 12 && d.Day >= 15) || (d.Month == 1 && d.Day <= 7)) return 12;
            return 0;
        }

        /// "halloween", "xmas" ili null.
        public static string SeasonKey(DateTime d)
        {
            int a = SeasonalAccessory(d);
            return a == 11 ? "halloween" : (a == 12 ? "xmas" : null);
        }
    }

    static partial class Painter
    {
        static int curPattern;

        // ---------------- šare ----------------
        static void DrawPattern(Graphics g, List<GraphicsPath> parts, float top)
        {
            if (curPattern <= 0) return;
            Region old = g.Clip;
            using (Region clip = new Region(new Rectangle(0, 0, 0, 0)))
            {
                foreach (GraphicsPath gp in parts) clip.Union(gp);
                g.SetClip(clip, CombineMode.Intersect);
                RectangleF b = RectangleF.Empty;
                foreach (GraphicsPath gp in parts) b = b.IsEmpty ? gp.GetBounds() : RectangleF.Union(b, gp.GetBounds());
                Color dark = Color.FromArgb(70, COut);
                switch (curPattern)
                {
                    case 1: // pruge
                        using (Pen pen = new Pen(Color.FromArgb(75, COut), 5f))
                            for (float x = b.Left - b.Height; x < b.Right; x += 13)
                                g.DrawLine(pen, x, b.Bottom, x + b.Height * 0.55f, b.Top);
                        break;
                    case 2: // tačke
                        using (SolidBrush br = new SolidBrush(Color.FromArgb(80, CSpikeTop)))
                        using (SolidBrush br2 = new SolidBrush(Color.FromArgb(55, COut)))
                        {
                            int row = 0;
                            for (float y = b.Top; y < b.Bottom; y += 11, row++)
                                for (float x = b.Left + (row % 2) * 6; x < b.Right; x += 12)
                                    g.FillEllipse(row % 3 == 0 ? br2 : br, x, y, 6, 6);
                        }
                        break;
                    case 3: // gradijent (boja bodlji prema repu)
                        using (LinearGradientBrush lg = new LinearGradientBrush(new PointF(b.Left, 0), new PointF(b.Right, 0), Color.FromArgb(220, CSpike), Color.FromArgb(0, CSpike)))
                            g.FillRectangle(lg, b);
                        break;
                    case 4: // kamuflaža
                        {
                            Random r = new Random(7);
                            using (SolidBrush d = new SolidBrush(Color.FromArgb(115, COut)))
                            using (SolidBrush l = new SolidBrush(Color.FromArgb(110, CSpikeTop)))
                                for (int i = 0; i < 40; i++)
                                {
                                    float x = b.Left + (float)r.NextDouble() * b.Width, y = b.Top + (float)r.NextDouble() * b.Height;
                                    float w = 8 + (float)r.NextDouble() * 14, h = 5 + (float)r.NextDouble() * 8;
                                    g.FillEllipse(i % 2 == 0 ? d : l, x - w / 2, y - h / 2, w, h);
                                }
                        }
                        break;
                    case 5: // galaksija
                        using (LinearGradientBrush lg = new LinearGradientBrush(new PointF(0, b.Top), new PointF(0, b.Bottom), Color.FromArgb(185, 40, 30, 95), Color.FromArgb(150, 90, 40, 140)))
                            g.FillRectangle(lg, b);
                        {
                            Random r = new Random(3);
                            using (SolidBrush star = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                            using (SolidBrush neb = new SolidBrush(Color.FromArgb(50, 120, 200, 255)))
                            {
                                for (int i = 0; i < 6; i++)
                                    g.FillEllipse(neb, b.Left + (float)r.NextDouble() * b.Width - 12, b.Top + (float)r.NextDouble() * b.Height - 8, 24, 16);
                                for (int i = 0; i < 45; i++)
                                {
                                    float s = r.Next(4) == 0 ? 2.4f : 1.4f;
                                    g.FillEllipse(star, b.Left + (float)r.NextDouble() * b.Width, b.Top + (float)r.NextDouble() * b.Height, s, s);
                                }
                            }
                        }
                        break;
                    case 6: // zlatna (sjaj + iskrice)
                        using (LinearGradientBrush lg = new LinearGradientBrush(new PointF(b.Left, b.Top), new PointF(b.Right, b.Bottom), Color.FromArgb(170, 255, 214, 90), Color.FromArgb(170, 190, 130, 20)))
                        {
                            ColorBlend cb = new ColorBlend(4);
                            cb.Colors = new Color[] { Color.FromArgb(175, 250, 205, 70), Color.FromArgb(185, 255, 244, 190), Color.FromArgb(175, 225, 165, 40), Color.FromArgb(185, 170, 115, 20) };
                            cb.Positions = new float[] { 0f, 0.35f, 0.6f, 1f };
                            lg.InterpolationColors = cb;
                            g.FillRectangle(lg, b);
                        }
                        {
                            Random r = new Random(11);
                            using (Pen sp = new Pen(Color.FromArgb(230, 255, 255, 235), 1.4f))
                                for (int i = 0; i < 14; i++)
                                {
                                    float x = b.Left + (float)r.NextDouble() * b.Width, y = b.Top + (float)r.NextDouble() * b.Height, z = 2 + (float)r.NextDouble() * 2.5f;
                                    g.DrawLine(sp, x - z, y, x + z, y); g.DrawLine(sp, x, y - z, x, y + z);
                                }
                        }
                        break;
                    case 7: // duga
                        {
                            Color[] rb = { Color.FromArgb(150, 235, 70, 70), Color.FromArgb(150, 245, 150, 50), Color.FromArgb(150, 245, 220, 60),
                                           Color.FromArgb(150, 80, 190, 90), Color.FromArgb(150, 70, 140, 230), Color.FromArgb(150, 150, 90, 210) };
                            float bw = b.Height / rb.Length + 1;
                            for (int i = 0; i < rb.Length; i++)
                                using (SolidBrush br = new SolidBrush(rb[i])) g.FillRectangle(br, b.Left, b.Top + i * (bw - 1), b.Width, bw);
                        }
                        break;
                }
                g.Clip = old;
            }
        }

        // ---------------- dodaci (lokalne koordinate glave: x 0..56, y 0..40, gleda desno) ----------------
        static void DrawAccessory(Graphics g, int acc, float hx, float hy, float s)
        {
            if (acc <= 0) return;
            GraphicsState gs = g.Save();
            g.TranslateTransform(hx, hy);
            g.ScaleTransform(s, s);
            Color ink = Color.FromArgb(40, 40, 46);
            using (Pen o = new Pen(ink, 2.2f))
            {
                o.LineJoin = LineJoin.Round;
                switch (acc)
                {
                    case 1: // kapa (zimska)
                        {
                            Color c = Color.FromArgb(214, 64, 70);
                            using (GraphicsPath p = new GraphicsPath())
                            {
                                p.AddArc(8, -12, 38, 30, 180, 180);
                                p.CloseFigure();
                                using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, p);
                                g.DrawPath(o, p);
                            }
                            using (GraphicsPath band = Theme.Round(new RectangleF(6, 0, 42, 8), 4))
                            {
                                using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 245, 245))) g.FillPath(b, band);
                                g.DrawPath(o, band);
                            }
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 245, 245))) g.FillEllipse(b, 21, -19, 11, 11);
                            g.DrawEllipse(o, 21, -19, 11, 11);
                        }
                        break;
                    case 2: // cilindar
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(35, 35, 40)))
                        {
                            g.FillEllipse(b, 4, -3, 46, 8);
                            g.FillRectangle(b, 13, -30, 28, 29);
                            g.FillEllipse(b, 13, -33, 28, 6);
                        }
                        using (SolidBrush band = new SolidBrush(Color.FromArgb(200, 60, 60))) g.FillRectangle(band, 13, -9, 28, 5);
                        using (Pen hl = new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f)) g.DrawLine(hl, 17, -28, 17, -11);
                        break;
                    case 3: // kravata
                        {
                            Color c = Color.FromArgb(40, 90, 170);
                            PointF[] knot = { new PointF(22, 38), new PointF(32, 38), new PointF(30, 45), new PointF(24, 45) };
                            PointF[] tie = { new PointF(24, 45), new PointF(30, 45), new PointF(34, 66), new PointF(27, 73), new PointF(20, 66) };
                            using (SolidBrush b = new SolidBrush(c)) { g.FillPolygon(b, tie); g.FillPolygon(b, knot); }
                            g.DrawPolygon(o, tie); g.DrawPolygon(o, knot);
                            using (Pen st = new Pen(Color.FromArgb(120, 255, 255, 255), 2f)) { g.DrawLine(st, 23, 52, 31, 56); g.DrawLine(st, 22, 60, 32, 64); }
                        }
                        break;
                    case 4: // slušalice
                        using (Pen band = new Pen(Color.FromArgb(50, 50, 58), 5f)) g.DrawArc(band, 8, -10, 36, 34, 190, 160);
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 60, 90)))
                        {
                            g.FillEllipse(b, 3, 6, 14, 18);
                            g.DrawEllipse(o, 3, 6, 14, 18);
                        }
                        break;
                    case 5: // krunica
                        {
                            PointF[] cr = { new PointF(14, 2), new PointF(14, -12), new PointF(20, -5), new PointF(27, -16), new PointF(34, -5), new PointF(40, -12), new PointF(40, 2) };
                            using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, -16), new PointF(0, 2), Color.FromArgb(255, 230, 120), Color.FromArgb(225, 165, 30)))
                                g.FillPolygon(b, cr);
                            g.DrawPolygon(o, cr);
                            using (SolidBrush gem = new SolidBrush(Color.FromArgb(220, 50, 80))) g.FillEllipse(gem, 24, -5, 6, 6);
                        }
                        break;
                    case 6: // motoristička kaciga
                        {
                            using (GraphicsPath shell = new GraphicsPath())
                            {
                                shell.AddArc(-4, -10, 54, 46, 180, 200);
                                shell.AddLine(48, 12, 46, 30);
                                shell.AddLine(46, 30, -2, 30);
                                shell.CloseFigure();
                                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, -10), new PointF(0, 30), Color.FromArgb(70, 120, 220), Color.FromArgb(25, 55, 140)))
                                    g.FillPath(b, shell);
                                g.DrawPath(o, shell);
                            }
                            using (GraphicsPath visor = Theme.Round(new RectangleF(24, 6, 26, 14), 6))
                            {
                                using (SolidBrush v = new SolidBrush(Color.FromArgb(150, 180, 220, 255))) g.FillPath(v, visor);
                                g.DrawPath(o, visor);
                            }
                            using (Pen stripe = new Pen(Color.FromArgb(230, 245, 245, 245), 3f)) g.DrawArc(stripe, 4, -4, 40, 36, 200, 120);
                        }
                        break;
                    case 7: // šal
                        {
                            Color c = Color.FromArgb(230, 120, 40);
                            using (GraphicsPath sc = Theme.Round(new RectangleF(2, 34, 40, 11), 5))
                            {
                                using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, sc);
                                g.DrawPath(o, sc);
                            }
                            PointF[] tail = { new PointF(8, 42), new PointF(16, 42), new PointF(13, 64), new PointF(5, 62) };
                            using (SolidBrush b = new SolidBrush(c)) g.FillPolygon(b, tail);
                            g.DrawPolygon(o, tail);
                            using (Pen st = new Pen(Color.FromArgb(160, 255, 255, 255), 2f)) { g.DrawLine(st, 12, 36, 12, 43); g.DrawLine(st, 24, 36, 24, 43); g.DrawLine(st, 7, 52, 14, 53); }
                        }
                        break;
                    case 8: // cvijet
                        {
                            float cx = 12, cy = 2;
                            using (SolidBrush pet = new SolidBrush(Color.FromArgb(255, 250, 250)))
                                for (int i = 0; i < 5; i++)
                                {
                                    double a = i * Math.PI * 2 / 5;
                                    float px = cx + (float)Math.Cos(a) * 6, py = cy + (float)Math.Sin(a) * 6;
                                    g.FillEllipse(pet, px - 5, py - 5, 10, 10);
                                    g.DrawEllipse(o, px - 5, py - 5, 10, 10);
                                }
                            using (SolidBrush mid = new SolidBrush(Color.FromArgb(250, 200, 40))) g.FillEllipse(mid, cx - 4, cy - 4, 8, 8);
                        }
                        break;
                    case 9: // brkovi
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(60, 40, 30)))
                        using (GraphicsPath m = new GraphicsPath())
                        {
                            m.AddBezier(46, 21, 42, 19, 36, 20, 33, 26);
                            m.AddBezier(33, 26, 38, 24, 42, 25, 46, 24);
                            m.CloseFigure();
                            m.AddBezier(47, 21, 51, 19, 56, 20, 58, 25);
                            m.AddBezier(58, 25, 54, 24, 51, 25, 47, 24);
                            m.CloseFigure();
                            g.FillPath(b, m);
                        }
                        break;
                    case 10: // leptir-mašna
                        {
                            Color c = Color.FromArgb(200, 40, 60);
                            PointF[] l = { new PointF(26, 42), new PointF(16, 36), new PointF(16, 48) };
                            PointF[] r = { new PointF(28, 42), new PointF(38, 36), new PointF(38, 48) };
                            using (SolidBrush b = new SolidBrush(c)) { g.FillPolygon(b, l); g.FillPolygon(b, r); }
                            g.DrawPolygon(o, l); g.DrawPolygon(o, r);
                            using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, 23, 39, 8, 7);
                            g.DrawEllipse(o, 23, 39, 8, 7);
                        }
                        break;
                    case 11: // vještičiji šešir (Noć vještica)
                        {
                            using (GraphicsPath brim = new GraphicsPath())
                            {
                                brim.AddEllipse(-2, -4, 58, 12);
                                using (SolidBrush b = new SolidBrush(Color.FromArgb(55, 40, 80))) g.FillPath(b, brim);
                                g.DrawPath(o, brim);
                            }
                            using (GraphicsPath cone = new GraphicsPath())
                            {
                                cone.AddBezier(12, 1, 16, -14, 22, -30, 34, -40);
                                cone.AddBezier(34, -40, 30, -28, 38, -12, 42, 1);
                                cone.CloseFigure();
                                using (SolidBrush b = new SolidBrush(Color.FromArgb(75, 55, 110))) g.FillPath(b, cone);
                                g.DrawPath(o, cone);
                            }
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 150, 40))) g.FillRectangle(b, 14, -6, 27, 5);
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 220, 90))) g.FillRectangle(b, 24, -7, 7, 7);
                        }
                        break;
                    case 12: // Djed Mrazova kapa (Božić / Nova godina)
                        {
                            using (GraphicsPath cap = new GraphicsPath())
                            {
                                cap.AddBezier(8, 2, 10, -18, 30, -26, 52, -14);
                                cap.AddBezier(52, -14, 58, -8, 54, 0, 48, 2);
                                cap.CloseFigure();
                                using (SolidBrush b = new SolidBrush(Color.FromArgb(214, 40, 50))) g.FillPath(b, cap);
                                g.DrawPath(o, cap);
                            }
                            using (GraphicsPath band = Theme.Round(new RectangleF(4, -2, 48, 9), 4.5f))
                            {
                                using (SolidBrush b = new SolidBrush(Color.FromArgb(248, 248, 248))) g.FillPath(b, band);
                                g.DrawPath(o, band);
                            }
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(248, 248, 248))) g.FillEllipse(b, 49, -10, 11, 11);
                            g.DrawEllipse(o, 49, -10, 11, 11);
                        }
                        break;
                }
            }
            g.Restore(gs);
        }
    }
}

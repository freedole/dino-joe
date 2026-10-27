// Dino Joe - vrste: Triceratops, Brontosaurus, Raptor, Stegosaurus, Pterodaktil (T-Rex je u Dino.cs).
// Isti vektorski stil kao T-Rex: zajednička glava i lice (DrawFace), boje iz palete, mašna i naočale.
// Koordinate: (0,0) = tačka na tlu ispod dina, x desno (pogled), y gore negativno.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DinoJoe
{
    static class Species
    {
        public const int Rex = 0, Trike = 1, Bronto = 2, Raptor = 3, Stego = 4, Ptero = 5;
        public const int Count = 6;   // 4 i 5 se otključavaju dostignućima
        public static readonly string[] Names = { "T-Rex", "Triceratops", "Brontosaurus", "Raptor", "Stegosaurus", "Pterodactyl" };

        public static string Name(int s)
        {
            if (s == Ptero && !Lang.IsEnglish) return "Pterodaktil";
            return Names[Math.Max(0, Math.Min(Count - 1, s))];
        }

        /// Y vrha glave (za oblačić, srca, Zzz) u odnosu na tlo.
        public static float HeadTop(int s)
        {
            switch (s)
            {
                case Trike: return -92;
                case Bronto: return -172;
                case Raptor: return -104;
                case Stego: return -112;
                case Ptero: return -122;
                default: return -108;
            }
        }
    }

    static partial class Painter
    {
        struct Anim
        {
            public bool walking, sitting, sleeping, dragged;
            public float bob, legA, legB, sway, breathe, headDY, armAng, bY;
        }

        static Anim Animate(Pose p)
        {
            Anim a = new Anim();
            State st = p.State;
            double t = p.T;
            a.walking = st == State.Walk;
            a.sitting = st == State.Sit || st == State.Sleep;
            a.sleeping = st == State.Sleep;
            a.dragged = st == State.Drag || st == State.Fall;
            a.armAng = -55;
            a.sway = (float)Math.Sin(t * (a.walking ? 6.0 : 1.6)) * (a.walking ? 5f : 3f);
            a.breathe = (float)Math.Sin(t * (a.sleeping ? 1.6 : 2.4)) * (a.sleeping ? 1.6f : 1.0f);
            if (a.walking)
            {
                double ph = t * 8.5;
                a.legA = (float)Math.Sin(ph) * 22f;
                a.legB = -a.legA;
                a.bob = (float)Math.Abs(Math.Cos(ph)) * 2.5f;
                a.armAng = -55 + (float)Math.Sin(ph) * 15f;
            }
            if (st == State.Happy) a.armAng = -95 + (float)Math.Sin(t * 18) * 30f;
            if (st == State.Roar) a.armAng = -110 + (float)Math.Sin(t * 30) * 8f;
            if (st == State.Stretch) a.armAng = -150 + (float)Math.Sin(t * 2.4) * 12f;
            if (a.dragged) { a.legA = (float)Math.Sin(t * 14) * 18f; a.legB = -a.legA; a.armAng = -120 + (float)Math.Sin(t * 16) * 25f; }
            a.bY = -a.bob + a.breathe * 0.5f;
            if (a.sitting) a.bY = 10 + a.breathe * 0.6f;
            return a;
        }

        // glava T-Rexa (56x40) skalirana i pomjerena: vraća putanju u koordinatama tijela
        static GraphicsPath HeadPath(float hx, float hy, float s)
        {
            GraphicsPath h = RoundRect(0, 0, 56, 40, 18);
            using (Matrix m = new Matrix()) { m.Translate(hx, hy); m.Scale(s, s); h.Transform(m); }
            return h;
        }

        static void HeadDetails(Graphics g, Pose p, float hx, float hy, float s)
        {
            GraphicsState gs = g.Save();
            g.TranslateTransform(hx, hy);
            g.ScaleTransform(s, s);
            using (SolidBrush spot = new SolidBrush(CSpot)) { g.FillEllipse(spot, 10, 6, 9, 6); g.FillEllipse(spot, 18, 3, 5, 4); }
            using (SolidBrush hl = new SolidBrush(Color.FromArgb(60, 255, 255, 255))) g.FillEllipse(hl, 14, 4, 26, 10);
            DrawFace(g, p, 0, 0);
            g.Restore(gs);
            DrawAccessory(g, p.Accessory, hx, hy, s);
        }

        static void BowOnHead(Graphics g, Pose p, float hx, float hy, float s)
        {
            if (!p.Female || Skins.CoversHead(p.Accessory)) return;
            GraphicsState gs = g.Save();
            g.TranslateTransform(hx, hy);
            g.ScaleTransform(s, s);
            DrawBow(g, 20, 1);
            g.Restore(gs);
        }

        static void FillParts(Graphics g, List<GraphicsPath> parts, float top, float outline)
        {
            foreach (GraphicsPath gp in parts) Outline(g, gp, outline);
            using (LinearGradientBrush main = new LinearGradientBrush(new PointF(0, top), new PointF(0, 2), CTop, CBot))
                foreach (GraphicsPath gp in parts) g.FillPath(main, gp);
            DrawPattern(g, parts, top);
        }

        static void FarLeg(Graphics g, GraphicsPath leg)
        {
            using (LinearGradientBrush fb = new LinearGradientBrush(new PointF(0, -40), new PointF(0, 2), CFarTop, CFarBot))
            {
                Outline(g, leg, 4.5f);
                g.FillPath(fb, leg);
            }
        }

        static GraphicsPath ThickLeg(float hipX, float hipY, float ang, float len, float w)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddPath(RoundRect(-w / 2, -4, w, len + 4, w / 2 - 1), false);
            gp.AddPath(RoundRect(-w / 2 - 1, len - 9, w + 6, 10, 5), false);
            using (Matrix m = new Matrix()) { m.Translate(hipX, hipY); m.Rotate(ang); gp.Transform(m); }
            return gp;
        }

        static void Belly(Graphics g, float x, float y, float w, float h)
        {
            using (LinearGradientBrush bb = new LinearGradientBrush(new PointF(0, y), new PointF(0, y + h + 1), CBellyTop, CBellyBot))
                g.FillEllipse(bb, x, y, w, h);
        }

        static void Dispose(List<GraphicsPath> parts) { foreach (GraphicsPath gp in parts) gp.Dispose(); }

        // ==================================================================
        //  TRICERATOPS
        // ==================================================================
        static void DrawTrike(Graphics g, Pose p)
        {
            Anim a = Animate(p);
            float bY = a.bY;
            float headDY = a.sleeping ? 12 : 0;

            // daleke noge
            if (!a.sitting)
            {
                using (GraphicsPath l = ThickLeg(-24, -24 + bY, a.legB, 20, 15)) FarLeg(g, l);
                using (GraphicsPath l = ThickLeg(16, -24 + bY, a.legA, 20, 15)) FarLeg(g, l);
            }

            List<GraphicsPath> parts = new List<GraphicsPath>();
            GraphicsPath tail = new GraphicsPath();
            tail.AddBezier(-38, -44 + bY, -52, -44 + bY, -62, -30 + a.sway * 0.4f, -70, -18 + bY + a.sway * 0.4f);
            tail.AddBezier(-70, -18 + bY + a.sway * 0.4f, -60, -16 + bY, -46, -22 + bY, -34, -26 + bY);
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(-44, -66 + bY - a.breathe * 0.4f, 82, 50 + a.breathe * 0.8f);
            parts.Add(body);

            float s = 0.92f;
            float hx = 26, hy = -78 + bY + headDY;
            if (p.State == State.Roar) hy -= 3;

            // krila (frill) iza glave - boja bodlji
            using (GraphicsPath frill = new GraphicsPath())
            {
                frill.AddEllipse(hx - 14, hy - 22, 40, 52);
                Outline(g, frill, 4.5f);
                using (LinearGradientBrush fb = new LinearGradientBrush(new PointF(0, hy - 22), new PointF(0, hy + 30), CSpikeTop, CSpike))
                    g.FillPath(fb, frill);
                using (SolidBrush dots = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                    for (int i = 0; i < 5; i++)
                    {
                        double ang = Math.PI * (0.65 + i * 0.17);
                        g.FillEllipse(dots, hx + 6 + (float)Math.Cos(ang) * 16 - 3, hy + 4 - (float)Math.Sin(ang) * 20 - 3, 6, 6);
                    }
            }

            GraphicsPath neck = new GraphicsPath();
            neck.AddEllipse(14, -66 + bY + headDY * 0.5f, 26, 30);
            parts.Add(neck);
            parts.Add(HeadPath(hx, hy, s));

            if (a.sitting)
            {
                GraphicsPath thigh = new GraphicsPath();
                thigh.AddEllipse(-30, -30, 32, 26);
                thigh.AddPath(RoundRect(-14, -10, 26, 10, 5), false);
                parts.Add(thigh);
                GraphicsPath front = new GraphicsPath();
                front.AddPath(RoundRect(14, -12, 28, 12, 6), false);
                parts.Add(front);
            }
            else
            {
                parts.Add(ThickLeg(-14, -22 + bY, a.legA, 20, 16));
                parts.Add(ThickLeg(24, -22 + bY, a.legB, 20, 16));
            }

            FillParts(g, parts, -96, 5f);
            Belly(g, -8, -50 + bY, 34, 26);
            using (SolidBrush spot = new SolidBrush(CSpot))
            {
                g.FillEllipse(spot, -30, -58 + bY, 11, 7);
                g.FillEllipse(spot, -14, -64 + bY, 8, 5);
                g.FillEllipse(spot, 2, -60 + bY, 7, 5);
            }

            // rogovi (kost)
            Color bone = Color.FromArgb(246, 238, 214), boneOut = Mix(COut, Color.FromArgb(120, 100, 70), 0.5f);
            using (GraphicsPath h1 = new GraphicsPath())
            using (GraphicsPath h2 = new GraphicsPath())
            using (GraphicsPath h3 = new GraphicsPath())
            {
                float bx = hx + 22 * s, by = hy + 3 * s;
                h1.AddPolygon(new PointF[] { new PointF(bx - 4, by + 2), new PointF(bx + 14, by - 22), new PointF(bx + 4, by + 2) });
                h2.AddPolygon(new PointF[] { new PointF(bx + 6, by + 3), new PointF(bx + 22, by - 16), new PointF(bx + 13, by + 4) });
                float nx = hx + 48 * s, ny = hy + 9 * s;
                h3.AddPolygon(new PointF[] { new PointF(nx - 4, ny + 2), new PointF(nx + 4, ny - 11), new PointF(nx + 5, ny + 4) });
                using (Pen o = new Pen(boneOut, 2.4f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, h2); g.DrawPath(o, h1); g.DrawPath(o, h3); }
                using (SolidBrush b = new SolidBrush(bone)) { g.FillPath(b, h2); g.FillPath(b, h1); g.FillPath(b, h3); }
            }

            BowOnHead(g, p, hx - 4, hy - 6, s);
            HeadDetails(g, p, hx, hy, s);
            Dispose(parts);
        }

        // ==================================================================
        //  BRONTOSAURUS
        // ==================================================================
        static void DrawBronto(Graphics g, Pose p)
        {
            Anim a = Animate(p);
            float bY = a.bY;
            float nod = (float)Math.Sin(p.T * 1.3) * 3f;
            float headDY = a.sleeping ? 92 : (a.sitting ? 20 : 0);

            if (!a.sitting)
            {
                using (GraphicsPath l = ThickLeg(-28, -28 + bY, a.legB * 0.8f, 26, 18)) FarLeg(g, l);
                using (GraphicsPath l = ThickLeg(20, -28 + bY, a.legA * 0.8f, 26, 18)) FarLeg(g, l);
            }

            List<GraphicsPath> parts = new List<GraphicsPath>();
            GraphicsPath tail = new GraphicsPath();
            float tipY = (a.sitting ? -22 : -26) + bY + a.sway * 0.8f;
            tail.AddBezier(-40, -56 + bY, -66, -56 + bY, -88, -40 + bY, -106, tipY);
            tail.AddBezier(-106, tipY, -88, -30 + bY, -66, -30 + bY, -38, -32 + bY);
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(-50, -74 + bY - a.breathe * 0.4f, 96, 56 + a.breathe * 0.8f);
            parts.Add(body);

            // vrat: traka od tijela do glave
            float s = 0.58f;
            float hx = 44 + (a.sleeping ? 18 : 0), hy = -170 + bY + headDY + nod;
            if (p.State == State.Roar) hy -= 4;
            float ntx = hx + 8, nty = hy + 18;     // vrh vrata (ispod glave)
            GraphicsPath neck = new GraphicsPath();
            neck.AddBezier(22, -66 + bY, 40, -86 + bY, ntx - 14, nty + 40, ntx - 6, nty);
            neck.AddLine(ntx - 6, nty, ntx + 12, nty + 4);
            neck.AddBezier(ntx + 12, nty + 4, ntx + 6, nty + 44, 54, -76 + bY, 42, -40 + bY);
            neck.CloseFigure();
            parts.Add(neck);
            parts.Add(HeadPath(hx, hy, s));

            if (a.sitting)
            {
                GraphicsPath thigh = new GraphicsPath();
                thigh.AddEllipse(-36, -32, 36, 30);
                thigh.AddPath(RoundRect(-20, -12, 30, 12, 6), false);
                parts.Add(thigh);
                GraphicsPath front = new GraphicsPath();
                front.AddPath(RoundRect(10, -14, 32, 14, 7), false);
                parts.Add(front);
            }
            else
            {
                parts.Add(ThickLeg(-18, -26 + bY, a.legA * 0.8f, 26, 19));
                parts.Add(ThickLeg(28, -26 + bY, a.legB * 0.8f, 26, 19));
            }

            FillParts(g, parts, -180, 5f);
            Belly(g, -14, -54 + bY, 44, 30);
            using (SolidBrush spot = new SolidBrush(CSpot))
            {
                g.FillEllipse(spot, -36, -66 + bY, 12, 8);
                g.FillEllipse(spot, -18, -72 + bY, 9, 6);
                g.FillEllipse(spot, 0, -68 + bY, 8, 5);
                g.FillEllipse(spot, -70, -44 + bY, 7, 5);
            }
            // male bodlje (boja bodlji) duž leđa
            for (int i = 0; i < 5; i++)
            {
                float x = -40 + i * 13, y = -72 + bY + Math.Abs(i - 2) * 2.2f;
                using (GraphicsPath sp = Spike(x, y, -90 - (i - 2) * 8, 7))
                using (SolidBrush sb = new SolidBrush(CSpike))
                {
                    Outline(g, sp, 3f);
                    g.FillPath(sb, sp);
                }
            }

            BowOnHead(g, p, hx, hy, s);
            HeadDetails(g, p, hx, hy, s);
            Dispose(parts);
        }

        // ==================================================================
        //  RAPTOR
        // ==================================================================
        static void DrawRaptor(Graphics g, Pose p)
        {
            Anim a = Animate(p);
            float bY = a.bY;
            float headDY = a.sleeping ? 18 : 0;
            float lift = a.sitting ? 0 : -8;      // duže noge

            if (!a.sitting)
                using (GraphicsPath far = Leg(-6, -36 + bY, a.legB, 30)) FarLeg(g, far);

            // perje na glavi (iza glave)
            float s = 0.78f;
            float hx = 18, hy = -96 + bY + lift + headDY;
            if (p.State == State.Roar) hy -= 3;
            for (int i = 0; i < 3; i++)
            {
                using (GraphicsPath f = new GraphicsPath())
                {
                    float fx = hx + 4 + i * 6, fy = hy + 4;
                    f.AddBezier(fx, fy, fx - 10 - i * 2, fy - 10, fx - 16 - i * 3, fy - 4, fx - 14 - i * 2, fy + 4);
                    f.CloseFigure();
                    using (Pen o = new Pen(COut, 3f)) g.DrawPath(o, f);
                    using (SolidBrush b = new SolidBrush(i == 1 ? CSpikeTop : CSpike)) g.FillPath(b, f);
                }
            }

            List<GraphicsPath> parts = new List<GraphicsPath>();
            GraphicsPath tail = new GraphicsPath();
            float tipY = (a.sitting ? -30 : -58) + bY + lift + a.sway * 0.6f;
            tail.AddBezier(-22, -60 + bY + lift, -50, -62 + bY + lift, -80, -60 + bY + lift, -100, tipY);
            tail.AddBezier(-100, tipY, -80, -50 + bY + lift, -50, -46 + bY + lift, -20, -46 + bY + lift);
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(-30, -70 + bY + lift - a.breathe * 0.3f, 56, 34 + a.breathe * 0.6f);
            parts.Add(body);

            GraphicsPath neck = Capsule(18, -60 + bY + lift, 18, 16, -150);
            parts.Add(neck);
            parts.Add(HeadPath(hx, hy, s));

            GraphicsPath nearLeg;
            if (a.sitting)
            {
                nearLeg = new GraphicsPath();
                nearLeg.AddEllipse(-16, -30, 28, 26);
                nearLeg.AddPath(RoundRect(0, -10, 26, 10, 5), false);
            }
            else nearLeg = Leg(4, -36 + bY, a.legA, 30);
            parts.Add(nearLeg);

            FillParts(g, parts, -110, 4.5f);
            Belly(g, -6, -56 + bY + lift, 26, 20);

            // pruge na leđima
            using (Pen stripe = new Pen(Color.FromArgb(Math.Min(255, CSpot.A + 40), CSpot.R, CSpot.G, CSpot.B), 3f))
            {
                stripe.StartCap = LineCap.Round; stripe.EndCap = LineCap.Round;
                for (int i = 0; i < 4; i++)
                {
                    float x = -24 + i * 11;
                    g.DrawLine(stripe, x, -68 + bY + lift, x + 4, -60 + bY + lift);
                }
                g.DrawLine(stripe, -48, -58 + bY + lift, -44, -52 + bY + lift);
                g.DrawLine(stripe, -64, -57 + bY + lift, -60, -52 + bY + lift);
            }

            // srpasta kandža na nozi
            if (!a.sitting)
            {
                using (Matrix m = new Matrix())
                using (GraphicsPath claw = new GraphicsPath())
                {
                    claw.AddBezier(4, 28, 10, 18, 16, 20, 14, 30);
                    m.Translate(4, -36 + bY); m.Rotate(a.legA); claw.Transform(m);
                    using (Pen o = new Pen(COut, 2f)) g.DrawPath(o, claw);
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(246, 238, 214))) g.FillPath(b, claw);
                }
            }

            // duža ruka sa kandžama
            using (GraphicsPath arm = Capsule(16, -52 + bY + lift, 16, 7f, a.armAng))
            using (LinearGradientBrush ab = new LinearGradientBrush(new PointF(0, -60), new PointF(0, -20), CTop, CBot))
            {
                Outline(g, arm, 4f);
                g.FillPath(ab, arm);
            }

            BowOnHead(g, p, hx, hy, s);
            HeadDetails(g, p, hx, hy, s);
            Dispose(parts);
        }
    
        // ==================================================================
        //  STEGOSAURUS (otključava se dostignućem)
        // ==================================================================
        static void DrawStego(Graphics g, Pose p)
        {
            Anim a = Animate(p);
            float bY = a.bY;
            float headDY = a.sleeping ? 14 : 0;

            if (!a.sitting)
            {
                using (GraphicsPath l = ThickLeg(-28, -30 + bY, a.legB * 0.8f, 26, 17)) FarLeg(g, l);
                using (GraphicsPath l = ThickLeg(18, -26 + bY, a.legA * 0.8f, 20, 14)) FarLeg(g, l);
            }

            // ploče na leđima (iza tijela): dva reda, naizmjenično
            float cxB = -6, cyB = -50 + bY, rx = 46, ry = 30;
            for (int row = 0; row < 2; row++)
                for (int i = 0; i < 6; i++)
                {
                    float x = cxB - 38 + i * 14 + row * 7;
                    if (x > cxB + 34) continue;
                    double u = (x - cxB) / rx;
                    float y = cyB - ry * (float)Math.Sqrt(Math.Max(0, 1 - u * u)) + 3;
                    float ang = -90 + (float)(u * 38);
                    float size = (row == 0 ? 30 : 25) * (1f - (float)Math.Abs(u) * 0.4f);
                    using (GraphicsPath pl = new GraphicsPath())
                    {
                        pl.AddBezier(-size * 0.42f, 2, -size * 0.55f, -size * 0.55f, -size * 0.2f, -size, 0, -size);
                        pl.AddBezier(0, -size, size * 0.2f, -size, size * 0.55f, -size * 0.55f, size * 0.42f, 2);
                        pl.CloseFigure();
                        using (Matrix m = new Matrix()) { m.Translate(x, y); m.Rotate(ang + 90); pl.Transform(m); }
                        Outline(g, pl, 3.6f);
                        using (LinearGradientBrush pb = new LinearGradientBrush(new PointF(0, y - size), new PointF(0, y + 2), row == 0 ? CSpikeTop : CSpike, CSpike))
                            g.FillPath(pb, pl);
                    }
                }

            // šiljci na repu
            float tipY = (a.sitting ? -18 : -30) + bY + a.sway * 0.6f;
            float[,] ts = { { -78, -46, -120, 16 }, { -86, -42, -145, 15 }, { -92, -38, -125, 14 }, { -98, -34, -155, 13 } };
            for (int i = 0; i < 4; i++)
            {
                float tx = ts[i, 0], ty = ts[i, 1] + bY + a.sway * 0.5f + (a.sitting ? 8 : 0);
                using (GraphicsPath sp = Spike(tx, ty, ts[i, 2], ts[i, 3]))
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(246, 238, 214)))
                {
                    Outline(g, sp, 3f);
                    g.FillPath(sb, sp);
                }
            }

            List<GraphicsPath> parts = new List<GraphicsPath>();
            GraphicsPath tail = new GraphicsPath();
            tail.AddBezier(-40, -60 + bY, -64, -58 + bY, -84, -44 + bY, -100, tipY);
            tail.AddBezier(-100, tipY, -84, -28 + bY, -62, -28 + bY, -38, -34 + bY);
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(cxB - rx, cyB - ry - a.breathe * 0.4f, rx * 2, ry * 2 + 2 + a.breathe * 0.8f);
            parts.Add(body);

            float s = 0.6f;
            float hx = 38, hy = -48 + bY + headDY;
            if (p.State == State.Roar) hy -= 3;
            GraphicsPath neck = new GraphicsPath();
            neck.AddEllipse(26, -54 + bY + headDY * 0.5f, 24, 22);
            parts.Add(neck);
            parts.Add(HeadPath(hx, hy, s));

            if (a.sitting)
            {
                GraphicsPath thigh = new GraphicsPath();
                thigh.AddEllipse(-38, -32, 36, 30);
                thigh.AddPath(RoundRect(-22, -12, 30, 12, 6), false);
                parts.Add(thigh);
                GraphicsPath front = new GraphicsPath();
                front.AddPath(RoundRect(14, -12, 28, 12, 6), false);
                parts.Add(front);
            }
            else
            {
                parts.Add(ThickLeg(-18, -30 + bY, a.legA * 0.8f, 26, 18));
                parts.Add(ThickLeg(26, -26 + bY, a.legB * 0.8f, 20, 15));
            }

            FillParts(g, parts, -84, 5f);
            Belly(g, -20, -40 + bY, 44, 22);
            using (SolidBrush spot = new SolidBrush(CSpot))
            {
                g.FillEllipse(spot, -34, -62 + bY, 11, 7);
                g.FillEllipse(spot, -14, -70 + bY, 9, 6);
                g.FillEllipse(spot, 8, -64 + bY, 8, 5);
                g.FillEllipse(spot, -64, -44 + bY, 7, 5);
            }

            BowOnHead(g, p, hx, hy, s);
            HeadDetails(g, p, hx, hy, s);
            Dispose(parts);
        }

        // ==================================================================
        //  PTERODAKTIL (otključava se dostignućem)
        // ==================================================================
        static void DrawPtero(Graphics g, Pose p)
        {
            Anim a = Animate(p);
            float bY = a.bY;
            float headDY = a.sleeping ? 16 : 0;
            float sitDY = a.sitting ? 6 : 0;

            // krila: 0 sklopljena, 1 raširena
            double open = 0, flap = 0;
            if (p.State == State.Happy || p.State == State.Roar) { open = 0.85; flap = Math.Sin(p.T * 14) * 0.25; }
            else if (a.dragged) { open = 1; flap = Math.Sin(p.T * 18) * 0.3; }
            else if (a.walking) { open = 0.12; flap = Math.Sin(p.T * 8.5) * 0.08; }
            float w = (float)Math.Max(0, Math.Min(1.1, open + flap));

            if (!a.sitting)
                using (GraphicsPath far = Leg(-4, -30 + bY, a.legB, 24)) FarLeg(g, far);

            // dalje krilo
            Wing(g, w, bY + sitDY, true);

            List<GraphicsPath> parts = new List<GraphicsPath>();
            GraphicsPath tail = new GraphicsPath();
            tail.AddBezier(-12, -46 + bY + sitDY, -24, -44 + bY + sitDY, -32, -36 + bY + sitDY, -38, -30 + bY + sitDY + a.sway * 0.3f);
            tail.AddBezier(-38, -30 + bY + sitDY + a.sway * 0.3f, -28, -34 + bY + sitDY, -20, -36 + bY + sitDY, -10, -36 + bY + sitDY);
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(-22, -78 + bY + sitDY - a.breathe * 0.4f, 40, 50 + a.breathe * 0.8f);
            parts.Add(body);

            float s = 0.7f;
            float hx = 10, hy = -112 + bY + headDY + sitDY;
            if (p.State == State.Roar) hy -= 3;
            GraphicsPath neck = Capsule(6, -72 + bY + sitDY, 22, 14, -170);
            parts.Add(neck);

            // greben (iza glave)
            using (GraphicsPath crest = new GraphicsPath())
            {
                crest.AddPolygon(new PointF[] { new PointF(hx + 6, hy + 8), new PointF(hx - 30, hy - 10), new PointF(hx - 26, hy - 2), new PointF(hx + 8, hy + 18) });
                Outline(g, crest, 3.6f);
                using (LinearGradientBrush cb = new LinearGradientBrush(new PointF(hx - 30, 0), new PointF(hx + 8, 0), CSpikeTop, CSpike)) g.FillPath(cb, crest);
            }
            // kljun (ispred glave)
            using (GraphicsPath beak = new GraphicsPath())
            {
                float bx = hx + 50 * s, by = hy + 12 * s;
                beak.AddPolygon(new PointF[] { new PointF(bx - 4, by), new PointF(bx + 34, by + 9), new PointF(bx - 4, by + 18) });
                Outline(g, beak, 3.6f);
                using (LinearGradientBrush kb = new LinearGradientBrush(new PointF(0, by), new PointF(0, by + 18), Color.FromArgb(255, 214, 110), Color.FromArgb(225, 150, 50))) g.FillPath(kb, beak);
            }
            parts.Add(HeadPath(hx, hy, s));

            GraphicsPath nearLeg;
            if (a.sitting)
            {
                nearLeg = new GraphicsPath();
                nearLeg.AddEllipse(-14, -26, 24, 22);
                nearLeg.AddPath(RoundRect(-2, -9, 22, 9, 4), false);
            }
            else nearLeg = Leg(6, -30 + bY, a.legA, 24);
            parts.Add(nearLeg);

            FillParts(g, parts, -116, 4.5f);
            Belly(g, -6, -66 + bY + sitDY, 20, 32);

            // bliže krilo
            Wing(g, w, bY + sitDY, false);

            BowOnHead(g, p, hx, hy, s);
            HeadDetails(g, p, hx, hy, s);
            Dispose(parts);
        }

        static void Wing(Graphics g, float w, float bY, bool far)
        {
            float shx = far ? 0 : 6, shy = -68 + bY;
            float ex = -14 + (-34 + 14) * w, ey = -60 + bY + (-108 + 60) * w;  // lakat (sklopljeno: uz leđa)
            float tx = -22 + (-92 + 22) * w, ty = -28 + bY + (-82 + 28) * w;   // vrh krila
            if (far) { ex -= 10 * w; tx -= 14 * w; ey += 6; ty += 4; }
            using (GraphicsPath m = new GraphicsPath())
            {
                m.AddLine(shx, shy, ex, ey);
                m.AddLine(ex, ey, tx, ty);
                m.AddBezier(tx, ty, tx + 10, ty + 4 + 8 * w, -4, -40 + bY, -2, -44 + bY);
                m.CloseFigure();
                Color top = far ? CFarTop : CSpikeTop, bot = far ? CFarBot : CSpike;
                Outline(g, m, 3.6f);
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, Math.Min(ey, ty) - 1), new PointF(0, -36 + bY), top, bot)) g.FillPath(b, m);
                using (Pen bone = new Pen(Color.FromArgb(120, COut), 2.2f)) { bone.StartCap = LineCap.Round; bone.EndCap = LineCap.Round; g.DrawLine(bone, shx, shy, ex, ey); g.DrawLine(bone, ex, ey, tx, ty); }
            }
            // kandžica na laktu
            using (SolidBrush cl = new SolidBrush(Color.FromArgb(246, 238, 214))) g.FillEllipse(cl, ex - 3, ey - 3, 6, 6);
        }
    }
}

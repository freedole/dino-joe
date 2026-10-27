// Dino Joe - Desktop Pet (ODjordje) - desktop dinosaurus ljubimac sa opcionim AI chatom (vidi AI.cs)
// https://github.com/freedole/dino-joe - MIT licenca
// Vektorski render (GDI+ antialiasing) + per-pixel alpha layered window (UpdateLayeredWindow).
// Build (Windows, bez instalacija): build.bat (ili GitHub Actions)  -> ugrađeni .NET Framework 4.x csc.exe. C# 5 kompatibilno.
// Dodatne fore: fore.txt pored exe-a, jedna fora po liniji (# = komentar).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("Dino Joe - Desktop Pet")]
[assembly: System.Reflection.AssemblyProduct("Dino Joe")]
[assembly: System.Reflection.AssemblyVersion("1.1.0.0")]

namespace DinoJoe
{
    static class Program
    {
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

        public static bool JustSetUp;   // upravo završen čarobnjak -> pozdravna poruka

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 2 && args[0] == "--preview") { Preview.Run(args[1]); return; }
            if (args.Length >= 1 && args[0] == "--profile")
            {
                Profile.Load();
                Console.WriteLine(Profile.Id + " | " + Profile.Name + " | sa " + Profile.NameWith + " | tray: " + Profile.TrayText + " | fora: " + Profile.JokeCount);
                foreach (JokeCat c in Profile.Cats) Console.WriteLine("  " + c.Name + " w=" + c.Weight + " n=" + c.Lines.Count + " keys=" + c.Keys.Length);
                Console.WriteLine("random: " + Profile.RandomJoke());
                Console.WriteLine("printer: " + OfflineBrain.Reply("printer mi ne radi"));
                Console.WriteLine("pile: " + Profile.Special("chick", Jokes.Chick));
                return;
            }
            if (args.Length >= 1 && args[0] == "--packs")
            {
                Profile.Load(); Settings.Load(); Settings.Apply();
                Application.EnableVisualStyles();
                using (PackManagerForm f = new PackManagerForm()) f.ShowDialog();
                return;
            }
            if (args.Length >= 2 && args[0] == "--import")
            {
                // uvoz paketa iz komandne linije (kasnije: dupli klik na .dinopack)
                Profile.Load(); Settings.Load();
                try { Pack p = Packs.Import(args[1]); Console.WriteLine("OK " + p.Id + " " + p.JokeCount); }
                catch (Exception ex) { Console.WriteLine("GRESKA " + ex.Message); }
                return;
            }
            if (args.Length >= 2 && args[0] == "--aitest")
            {
                AiConfig c = new AiConfig(Path.GetTempPath());
                c.Url = args[1];
                List<ChatMsg> h = new List<ChatMsg>();
                h.Add(new ChatMsg("user", "Ko si ti?"));
                AiResult r = AiClient.Ask(c, "test-key", c.Persona(), h, 50);
                Console.WriteLine(r.Ok + " " + r.Status + " " + r.Text);
                Console.WriteLine("offline: " + OfflineBrain.Reply("a šta misliš o komasaciji?"));
                return;
            }

            bool created;
            Profile.Load();
            using (Mutex m = new Mutex(true, Profile.Id + "_DesktopPet_SingleInstance", out created))
            {
                if (!created) return;
                try { SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Settings.Load();
                Settings.Apply();
                Life.Load();
                Achievements.Load();
                bool firstRun = !Settings.SetupDone;
                if (!Settings.SetupDone || (args.Length >= 1 && args[0] == "--setup"))
                {
                    string dir0 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id);
                    try { Directory.CreateDirectory(dir0); } catch { }
                    using (SetupWizard w = new SetupWizard(true, new AiConfig(dir0))) w.ShowDialog();
                    JustSetUp = true;
                    if (firstRun && !Life.Exists) Life.StartEgg();   // novi dino počinje kao jaje
                }
                Settings.Apply();
                Application.Run(new PetForm());
            }
        }
    }

    // ======================================================================
    //  FORE
    // ======================================================================
    static class Jokes
    {
        public static readonly string[] Dino = {
            "All my friends are dead.",
            "All my friends are dead. Osim tebe. I kokošaka.",
            "Svi moji drugari su izumrli. Ostali smo ja i Excel.",
            "Meteor? Nikad nisam vjerovao vremenskoj prognozi.",
            "Nisam ja star. Ja sam vintage.",
            "Kratke ruke, dug uptime.",
            "66 miliona godina ne mogu da se počešem po leđima.",
            "Ne mogu da te zagrlim, ruke su mi kratke. Ali mislim na tebe.",
            "Ja sam fosil, ti si samo legacy.",
            "Misliš da je tvoj server star? Haha.",
            "Kreda je bila bolja. Nije se čekalo na šalteru.",
            "Dinosaurusi nisu izumrli. Samo su prešli na kokoške.",
            "Moje zadnje riječi su bile: 'Šta je ono svijetlo?'",
            "Izumiranje je samo duži godišnji odmor.",
            "ROAAAR! ...izvini, refleks.",
            "Pangea je bila open-space kancelarija.",
            "Ne bojim se ničega. Osim meteora. I usisivača.",
            "Kad me vidiš u muzeju, pravi se da me ne poznaješ.",
            "Moj dentalni karton: 60 zuba, 0 zubara.",
            "Nisam debeo, imam jake kosti. Doslovno, fosilizovane."
        };

        public static readonly string[] IT = {
            "ping... pong!",
            "sudo nahrani me",
            "Jesi li uradio backup?",
            "Backup koji nije testiran je samo nada.",
            "Radi na mojoj mašini.",
            "Ne diraj produkciju u petak.",
            "DNS je kriv. Uvijek.",
            "Kad sve radi - sumnjivo.",
            "Restartuj pa javi.",
            "Nije bug, to je feature iz Jure.",
            "Docker? Ja sam bio kontejnerizovan u ćilibaru.",
            "Ja nisam legacy sistem. Ja sam baština.",
            "Group Policy mi zabranjuje da jedem kokoške.",
            "Ko je dao Domain Admin praktikantu?",
            "Lozinka123 nije lozinka.",
            "SQL upit traje duže od moje evolucije.",
            "Uptime mi je 66 miliona godina. Bez patcha.",
            "VPN pao? Nisam ja, ja sam samo šetao.",
            "Ko je ugasio server?",
            "RAID nije backup. Ni ja nisam kokoška. Iako...",
            "Ticket zatvoren: 'Korisnik izumro'.",
            "Hyper-V? Ja sam Hyper-Rex."
        };

        public static readonly string[] Farm = {
            "Kokoške su mi praunuke. Ozbiljno, pitaj biologe.",
            "Šta je bilo prije, kokoška ili jaje? JA.",
            "Uzgajaš piliće? To su moji rođaci, budi fin.",
            "Tovni pilić za 42 dana. Ja sam rastao 20 godina.",
            "Inkubator na 37,7 °C. Moja jaja nisu imala ni termometar.",
            "Pijetao misli da je T-rex. Pustite ga, neka sanja.",
            "'Kukuriku' je loš prevod od 'ROAAAR'.",
            "Brojler od 2,5 kg? Sitnež. Moj praunuk je imao 8 tona.",
            "Hranilice pune? Praunuci su gladni.",
            "Ventilacija u peradarniku je bitna. Pitaj mene za promjene atmosfere.",
            "Kokoš snese jaje i viče. Ja snesem jaje i dođe meteor.",
            "Ne sipaj mi rođake u traktor. (nafta, kontaš?)",
            "Kombajn je jedini koga se bojim.",
            "Traktor je glasniji od mene. Ne volim ga.",
            "Dobra godina za kukuruz. Loša za meteore.",
            "Silaža miriše bolje od vulkanskog pepela.",
            "Kad budeš žnjeo, ostavi mi jedan red.",
            "Ja bih paprat, ali ajde, može i kukuruz.",
            "Nosilje nose jaja. Ja nosim teret istorije.",
            "Pilići piju više kad je vruće. I ja.",
            "Lisica u kokošinjcu? Pusti mene da porazgovaram s njom.",
            "Kokoške me gledaju sa poštovanjem. Znaju ko je đed."
        };

        public static readonly string[] Koma = {
            "Pangea - prva i jedina uspješna komasacija.",
            "Prije komasacije: 47 parcela. Poslije: 1 parcela i 46 prigovora.",
            "Komšija opet pomjerio međaš. Ja ne mogu, ruke kratke.",
            "Komasaciona masa? Ja sam 7 tona komasacione mase.",
            "Bonitiranje: moja parcela I klasa, komšijina VIII. Zna se.",
            "Prigovor na raspodjelu! Dobio sam parcelu pored kanala.",
            "Šta je brže: komasacija ili kontinentalni drift?",
            "Meteor nije imao rješenje o komasaciji. Nelegalno.",
            "Novi premjer, stari komšija.",
            "Geodeti su opet bili ovdje. Pojeli mi sendvič.",
            "Iskolčenje završeno. Kolac sam pojeo.",
            "Svi kažu da su dobili lošiju zemlju. SVI.",
            "Zajedničke potrebe: put, kanal i moj rep.",
            "Odbitak za zajedničke potrebe? Meni su odbili cijelu vrstu.",
            "Djed mi je imao 1/16 suvlasništva na Pangei.",
            "Ne stoji mi rep na međi. Mjerite ponovo.",
            "Komisija za komasaciju zasjeda. Ja ležim.",
            "Stari posjedovni list, nova parcela, isti komšija.",
            "Ako komasacija potraje, izumrijeću opet.",
            "U komasacionoj masi nađene kosti. Moje. Ne diraj.",
            "Žalba na rješenje o raspodjeli: rok 15 dana. Moja traje 66 miliona godina.",
            "Arondacija, komasacija... samo da ne bude ekstinkcija.",
            "Parcela 1/2 pa 1/3 pa 1/4... ovo je razlomak, ne posjed.",
            "Procjena vrijednosti zemljišta: kamenje se ne broji, kosti se broje.",
            "Ja ne prigovaram. Ja ričem."
        };

        public static readonly string[] Glasses = {
            "Četvorooki? Da, i šta?",
            "Four eyes, two arms. Kratke.",
            "Sad vidim i tvoje bugove.",
            "Naočale +5: dioptrija ili samopouzdanje.",
            "Četvorooki, ali čitam log bez zumiranja.",
            "Sa naočalama izgledam kao viši stručni saradnik.",
            "Nije mi do vida, samo da izgledam pametno."
        };

        public static readonly string[] Chick = {
            "Ovo je moj praunuk. Pozdravi ga.",
            "Pile, ne trči! Pazi na kombajn!",
            "Vidi ga, isti đed.",
            "Pile kaže 'piju'. Ja kažem 'ROAR'. Ista stvar.",
            "Praunuk mi je došao u posjetu.",
            "Nemoj ga pojesti. Ozbiljno."
        };

        public static readonly string[] Feed = {
            "Njam njam!", Lang.Get("s.njam"), "Ovo nije piletina, je l' da?",
            "Hvala! Sad mogu još 66 miliona godina.", "Mmm, paprat sa kečapom."
        };

        public static readonly string[] Poke = {
            "Hej!", "Ne bockaj, imam ljuske.", "Rawr!", "Šta je bilo?", "Prrr...", "Golica!"
        };
    }

    // ======================================================================
    //  CRTANJE
    // ======================================================================
    enum State { Walk, Idle, Sit, Sleep, Happy, Roar, Drag, Fall }

    class Pose
    {
        public State State;
        public double T;          // sekunde
        public int Dir = 1;       // 1 desno, -1 lijevo
        public bool Blink;
        public bool Glasses;
        public bool Female;       // ODjordjina: trepavice + mašna
        public int Species;       // 0 T-Rex, 1 Triceratops, 2 Brontosaurus, 3 Raptor
        public int Pattern;       // šara (Skins.PatternKeys)
        public int Accessory;     // dodatak (Skins.AccessoryKeys)
        public double JumpY;      // jedinice, pozitivno = gore
        public double HeartsT = -1; // 0..1 dok traju srca
        public bool Chick;
        public double ChickPhase;
        public string Bubble;
        public double BubbleAlpha = 1;
        public int Stage = Life.Adult;  // jaje, beba, mladunče, odrasli
        public double Warmth;           // jaje: 0..100
        public double HatchT = -1;      // jaje: animacija izlijeganja 0..1
    }

    static partial class Painter
    {
        public const float CanvasW = 250, CanvasH = 265, GroundPad = 6;

        // Boje se postavljaju preko SetColors (vidi Skins.cs). Podrazumijevano: klasična zelena.
        static Color CTop = Color.FromArgb(150, 220, 120);
        static Color CBot = Color.FromArgb(56, 150, 70);
        static Color CFarTop = Color.FromArgb(70, 150, 70);
        static Color CFarBot = Color.FromArgb(40, 110, 50);
        static Color COut = Color.FromArgb(24, 70, 36);
        static Color CBellyTop = Color.FromArgb(235, 248, 205);
        static Color CBellyBot = Color.FromArgb(200, 228, 160);
        static Color CSpot = Color.FromArgb(55, 20, 90, 40);
        static Color CNose = Color.FromArgb(150, 20, 60, 30);
        static Color CMouth = Color.FromArgb(30, 60, 35);
        static Color CSpike = Color.FromArgb(38, 120, 72);
        static Color CSpikeTop = Color.FromArgb(70, 170, 100);

        static Color Mix(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public static void SetClassic()
        {
            CTop = Color.FromArgb(150, 220, 120); CBot = Color.FromArgb(56, 150, 70);
            CFarTop = Color.FromArgb(70, 150, 70); CFarBot = Color.FromArgb(40, 110, 50);
            COut = Color.FromArgb(24, 70, 36);
            CBellyTop = Color.FromArgb(235, 248, 205); CBellyBot = Color.FromArgb(200, 228, 160);
            CSpot = Color.FromArgb(55, 20, 90, 40); CNose = Color.FromArgb(150, 20, 60, 30); CMouth = Color.FromArgb(30, 60, 35);
            CSpike = Color.FromArgb(38, 120, 72); CSpikeTop = Color.FromArgb(70, 170, 100);
        }

        // primary = tijelo, secondary = bodlje
        public static void SetColors(Color primary, Color secondary)
        {
            Color w = Color.White, k = Color.Black;
            CTop = Mix(primary, w, 0.32f);
            CBot = Mix(primary, k, 0.18f);
            CFarTop = Mix(primary, k, 0.2f);
            CFarBot = Mix(primary, k, 0.4f);
            COut = Mix(primary, k, 0.68f);
            CBellyTop = Mix(primary, w, 0.82f);
            CBellyBot = Mix(primary, w, 0.62f);
            Color sp = Mix(primary, k, 0.5f);
            CSpot = Color.FromArgb(55, sp.R, sp.G, sp.B);
            Color ns = Mix(primary, k, 0.7f);
            CNose = Color.FromArgb(150, ns.R, ns.G, ns.B);
            CMouth = Mix(primary, k, 0.72f);
            CSpike = Mix(secondary, k, 0.15f);
            CSpikeTop = Mix(secondary, w, 0.25f);
        }

        public static void Render(Graphics g, float k, Pose p)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.Clear(Color.Transparent);

            GraphicsState gs0 = g.Save();
            g.ScaleTransform(k, k);

            float cx = CanvasW / 2f, gy = CanvasH - GroundPad;

            // senka
            float sc = p.Stage == Life.Egg ? 0.7f : Life.StageScale[Math.Max(0, Math.Min(3, p.Stage))];
            float shW = (86f - (float)p.JumpY * 0.8f) * sc;
            using (SolidBrush b = new SolidBrush(Color.FromArgb(55, 0, 0, 0)))
                g.FillEllipse(b, cx - shW / 2f, gy - 4, shW, 8);

            // pile iza
            if (p.Chick) DrawChick(g, cx - p.Dir * 92f, gy, p);

            if (p.Stage == Life.Egg)
            {
                DrawDesktopEgg(g, cx, gy - (float)p.JumpY, p);
                if (p.HeartsT >= 0 && p.HeartsT <= 1) DrawHearts(g, cx, gy - 120, p.HeartsT);
                if (!string.IsNullOrEmpty(p.Bubble)) DrawBubble(g, p.Bubble, cx + 10, gy - 112 - (float)p.JumpY, p.BubbleAlpha);
                g.Restore(gs0);
                return;
            }

            GraphicsState gs = g.Save();
            g.TranslateTransform(cx, gy - (float)p.JumpY);
            if (p.Dir < 0) g.ScaleTransform(-1, 1);
            if (sc != 1f) g.ScaleTransform(sc, sc);
            DrawDino(g, p);
            g.Restore(gs);

            // Zzz
            float top = DinoJoe.Species.HeadTop(p.Species);
            if (p.State == State.Sleep) DrawZzz(g, cx + p.Dir * 34f * sc - 6, gy + (p.Species == DinoJoe.Species.Bronto ? -44 : top + 24) * sc, p.T);

            // srca
            if (p.HeartsT >= 0 && p.HeartsT <= 1) DrawHearts(g, cx - p.Dir * 14f * sc, gy + (top - 4) * sc, p.HeartsT);

            if (!string.IsNullOrEmpty(p.Bubble)) DrawBubble(g, p.Bubble, cx + p.Dir * 26f * sc, gy + top * sc - (float)p.JumpY, p.BubbleAlpha);

            g.Restore(gs0);
        }

        // ------------------------------------------------------------------
        static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
        {
            GraphicsPath gp = new GraphicsPath();
            float d = r * 2;
            gp.AddArc(x, y, d, d, 180, 90);
            gp.AddArc(x + w - d, y, d, d, 270, 90);
            gp.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            gp.AddArc(x, y + h - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        static GraphicsPath Capsule(float px, float py, float len, float w, float angDeg)
        {
            GraphicsPath gp = RoundRect(-w / 2f, -w / 2f, w, len + w, w / 2f);
            Matrix m = new Matrix();
            m.Translate(px, py);
            m.Rotate(angDeg);
            gp.Transform(m);
            return gp;
        }

        static GraphicsPath Leg(float hipX, float hipY, float ang, float len)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddPath(RoundRect(-8, -4, 16, len + 4, 7), false);
            gp.AddPath(RoundRect(-8, len - 9, 21, 10, 5), false); // stopalo
            Matrix m = new Matrix();
            m.Translate(hipX, hipY);
            m.Rotate(ang);
            gp.Transform(m);
            return gp;
        }

        static GraphicsPath Spike(float x, float y, float normalDeg, float size)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] {
                new PointF(-size * 0.55f, 2), new PointF(0, -size), new PointF(size * 0.55f, 2) });
            Matrix m = new Matrix();
            m.Translate(x, y);
            m.Rotate(normalDeg + 90);
            gp.Transform(m);
            return gp;
        }

        static void Outline(Graphics g, GraphicsPath gp, float w)
        {
            using (Pen pen = new Pen(COut, w))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, gp);
            }
        }

        static void DrawDino(Graphics g, Pose p)
        {
            curPattern = p.Pattern;
            if (p.Species == DinoJoe.Species.Trike) { DrawTrike(g, p); return; }
            if (p.Species == DinoJoe.Species.Bronto) { DrawBronto(g, p); return; }
            if (p.Species == DinoJoe.Species.Raptor) { DrawRaptor(g, p); return; }
            if (p.Species == DinoJoe.Species.Stego) { DrawStego(g, p); return; }
            if (p.Species == DinoJoe.Species.Ptero) { DrawPtero(g, p); return; }
            State st = p.State;
            double t = p.T;
            bool walking = st == State.Walk;
            bool sitting = st == State.Sit || st == State.Sleep;
            bool sleeping = st == State.Sleep;
            bool dragged = st == State.Drag || st == State.Fall;

            float bob = 0, legA = 0, legB = 0, sway, breathe, headDY = 0, armAng = -55;
            sway = (float)Math.Sin(t * (walking ? 6.0 : 1.6)) * (walking ? 5f : 3f);
            breathe = (float)Math.Sin(t * (sleeping ? 1.6 : 2.4)) * (sleeping ? 1.6f : 1.0f);
            if (walking)
            {
                double ph = t * 8.5;
                legA = (float)Math.Sin(ph) * 26f;
                legB = -legA;
                bob = (float)Math.Abs(Math.Cos(ph)) * 2.5f;
                armAng = -55 + (float)Math.Sin(ph) * 15f;
            }
            if (st == State.Happy) { armAng = -95 + (float)Math.Sin(t * 18) * 30f; }
            if (st == State.Roar) { armAng = -110 + (float)Math.Sin(t * 30) * 8f; }
            if (dragged) { legA = (float)Math.Sin(t * 14) * 20f; legB = -legA; armAng = -120 + (float)Math.Sin(t * 16) * 25f; }

            float bY = -bob + breathe * 0.5f;          // pomak tijela
            if (sitting) bY = 12 + breathe * 0.6f;
            if (sleeping) headDY = 14;

            // ---------- stražnja noga ----------
            if (!sitting)
            {
                using (GraphicsPath far = Leg(-12, -26 + bY, legB, 22))
                using (LinearGradientBrush fb = new LinearGradientBrush(new PointF(0, -30), new PointF(0, 2), CFarTop, CFarBot))
                {
                    Outline(g, far, 4.5f);
                    g.FillPath(fb, far);
                }
            }

            // ---------- bodlje ----------
            float tailTipY = sitting ? -6 : (-22 + sway);
            float[,] sp = sitting ? new float[,] {
                    { 8, -96 + headDY, -150, 11 }, { 3, -76 + headDY * 0.5f, 180, 11 },
                    { -6, -61, -110, 12 }, { -19, -59, -100, 12 }, { -31, -51, -125, 11 },
                    { -44, -38, -115, 9 }, { -56, -24, -105, 7 } }
                : new float[,] {
                    { 8, -96, -150, 11 }, { 3, -76, 180, 11 },
                    { -6, -61, -110, 12 }, { -19, -59, -100, 12 }, { -31, -51, -125, 11 },
                    { -44, -42 + sway * 0.3f, -120, 9 }, { -56, -33 + sway * 0.6f, -115, 7 } };
            for (int i = 0; i < sp.GetLength(0); i++)
            {
                float yy = sp[i, 1] + (i < 2 ? bY : bY);
                using (GraphicsPath s = Spike(sp[i, 0], yy, sp[i, 2], sp[i, 3]))
                using (LinearGradientBrush sb = new LinearGradientBrush(new PointF(0, yy - 12), new PointF(0, yy + 2), CSpikeTop, CSpike))
                {
                    Outline(g, s, 4f);
                    g.FillPath(sb, s);
                }
            }

            // ---------- silueta ----------
            List<GraphicsPath> parts = new List<GraphicsPath>();

            GraphicsPath tail = new GraphicsPath();
            if (sitting)
            {
                tail.AddBezier(-24, -40 + bY, -44, -40 + bY, -58, -16, -70, -6);
                tail.AddBezier(-70, -6, -66, 0, -40, 0, -20, -10 + bY * 0.3f);
            }
            else
            {
                tail.AddBezier(-22, -44 + bY, -40, -46 + bY, -56, -30 + bY + sway * 0.5f, -70, tailTipY + bY);
                tail.AddBezier(-70, tailTipY + bY, -60, -12 + bY + sway * 0.3f, -40, -12 + bY, -20, -20 + bY);
            }
            tail.CloseFigure();
            parts.Add(tail);

            GraphicsPath body = new GraphicsPath();
            body.AddEllipse(-34, -62 + bY - breathe * 0.4f, 62, 50 + breathe * 0.8f);
            parts.Add(body);

            GraphicsPath neck = new GraphicsPath();
            neck.AddEllipse(4, -76 + bY + headDY * 0.5f, 28, 34);
            parts.Add(neck);

            float hx = 6, hy = -100 + bY + headDY;
            if (st == State.Roar) hy -= 3;
            GraphicsPath head = RoundRect(hx, hy, 56, 40, 18);
            parts.Add(head);

            GraphicsPath nearLeg;
            if (sitting)
            {
                nearLeg = new GraphicsPath();
                nearLeg.AddEllipse(-20, -30, 32, 28);       // bedro
                nearLeg.AddPath(RoundRect(-2, -10, 26, 10, 5), false); // stopalo naprijed
            }
            else nearLeg = Leg(6, -26 + bY, legA, 22);
            parts.Add(nearLeg);

            foreach (GraphicsPath gp in parts) Outline(g, gp, 5f);
            using (LinearGradientBrush main = new LinearGradientBrush(new PointF(0, -104), new PointF(0, 2), CTop, CBot))
                foreach (GraphicsPath gp in parts) g.FillPath(main, gp);
            DrawPattern(g, parts, -104);

            // stomak + šare (clip na tijelo)
            using (LinearGradientBrush bb = new LinearGradientBrush(new PointF(0, -52 + bY), new PointF(0, -16 + bY), CBellyTop, CBellyBot))
                g.FillEllipse(bb, -3, -52 + bY, 25, 35);
            using (SolidBrush spot = new SolidBrush(CSpot))
            {
                g.FillEllipse(spot, -24, -56 + bY, 10, 7);
                g.FillEllipse(spot, -12, -60 + bY, 7, 5);
                g.FillEllipse(spot, -40, -40 + bY + (sitting ? 4 : sway * 0.25f), 6, 4);
                g.FillEllipse(spot, hx + 10, hy + 6, 9, 6);
                g.FillEllipse(spot, hx + 18, hy + 3, 5, 4);
            }
            // sjaj na glavi
            using (SolidBrush hl = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillEllipse(hl, hx + 14, hy + 4, 26, 10);

            // mašna (ženski dino)
            if (p.Female && !Skins.CoversHead(p.Accessory)) DrawBow(g, hx + 20, hy + 1);

            // ---------- ruka ----------
            using (GraphicsPath arm = Capsule(20, -45 + bY, 10, 7.5f, armAng))
            using (LinearGradientBrush ab = new LinearGradientBrush(new PointF(0, -50), new PointF(0, -24), CTop, CBot))
            {
                Outline(g, arm, 4f);
                g.FillPath(ab, arm);
            }

            // ---------- lice ----------
            DrawFace(g, p, hx, hy);
            DrawAccessory(g, p.Accessory, hx, hy, 1f);

            foreach (GraphicsPath gp in parts) gp.Dispose();
        }

        // jaje na desktopu: boja tijela kao tufne, pukotine po toplini, ljuljanje
        static void DrawDesktopEgg(Graphics g, float cx, float gy, Pose p)
        {
            float w = 74, h = 94;
            double t = p.T;
            double wob = p.HatchT >= 0 ? Math.Sin(p.HatchT * 60) * (6 + p.HatchT * 14)
                       : (p.State == State.Happy || p.State == State.Drag ? Math.Sin(t * 16) * 9 : Math.Sin(t * 2.2) * (1.5 + p.Warmth / 40.0));
            GraphicsState gs = g.Save();
            g.TranslateTransform(cx, gy);
            g.RotateTransform((float)wob);

            // toplina: blagi narandžasti sjaj
            if (p.Warmth > 5)
            {
                int al = (int)Math.Min(90, p.Warmth * 0.9);
                using (GraphicsPath glow = new GraphicsPath())
                {
                    glow.AddEllipse(-w * 0.75f, -h * 1.12f, w * 1.5f, h * 1.2f);
                    using (PathGradientBrush pg = new PathGradientBrush(glow))
                    {
                        pg.CenterColor = Color.FromArgb(al, 255, 190, 90);
                        pg.SurroundColors = new Color[] { Color.FromArgb(0, 255, 190, 90) };
                        g.FillPath(pg, glow);
                    }
                }
            }

            using (GraphicsPath egg = new GraphicsPath())
            {
                egg.AddBezier(0, -h, w * 0.62f, -h, w * 0.58f, 0, 0, 0);
                egg.AddBezier(0, 0, -w * 0.58f, 0, -w * 0.62f, -h, 0, -h);
                egg.CloseFigure();
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(-w / 2, -h), new PointF(w / 2, 0), Color.FromArgb(255, 252, 238), Color.FromArgb(232, 220, 190)))
                    g.FillPath(b, egg);
                Region old = g.Clip;
                g.SetClip(egg, CombineMode.Intersect);
                using (SolidBrush sp = new SolidBrush(Color.FromArgb(170, CBot)))
                {
                    g.FillEllipse(sp, -24, -74, 15, 11);
                    g.FillEllipse(sp, 8, -58, 18, 13);
                    g.FillEllipse(sp, -18, -36, 12, 9);
                    g.FillEllipse(sp, 14, -24, 10, 8);
                    g.FillEllipse(sp, -5, -88, 8, 7);
                }
                using (SolidBrush hl = new SolidBrush(Color.FromArgb(110, 255, 255, 255)))
                    g.FillEllipse(hl, -20, -78, 13, 24);
                g.Clip = old;
                using (Pen o = new Pen(Color.FromArgb(150, 120, 80), 2.5f)) g.DrawPath(o, egg);

                using (Pen c = new Pen(Color.FromArgb(110, 80, 50), 2f))
                {
                    c.LineJoin = LineJoin.Round;
                    if (p.Warmth >= 50)
                        g.DrawLines(c, new PointF[] { new PointF(6, -h * 0.82f), new PointF(1, -h * 0.72f), new PointF(8, -h * 0.64f), new PointF(3, -h * 0.55f) });
                    if (p.Warmth >= 80)
                        g.DrawLines(c, new PointF[] { new PointF(-w * 0.42f, -h * 0.5f), new PointF(-w * 0.28f, -h * 0.57f), new PointF(-w * 0.16f, -h * 0.47f), new PointF(-w * 0.05f, -h * 0.55f) });
                    if (p.HatchT >= 0)
                    {
                        float y = -h * 0.55f;
                        g.DrawLines(c, new PointF[] {
                            new PointF(-w * 0.45f, y), new PointF(-w * 0.3f, y - 8), new PointF(-w * 0.15f, y + 4),
                            new PointF(0, y - 9), new PointF(w * 0.15f, y + 3), new PointF(w * 0.3f, y - 7), new PointF(w * 0.45f, y) });
                    }
                }
            }
            g.Restore(gs);

            // traka topline ispod jajeta
            if (p.HatchT < 0)
            {
                float bw = 60, bx = cx - bw / 2, by = gy + 1;
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(120, 255, 255, 255))) g.FillRectangle(bg, bx, by, bw, 4);
                using (SolidBrush fg = new SolidBrush(Color.FromArgb(220, 255, 150, 60))) g.FillRectangle(fg, bx, by, bw * (float)Math.Min(1, p.Warmth / 100.0), 4);
            }
        }

        public static Color BowColor = Color.FromArgb(232, 62, 140);

        static void DrawBow(Graphics g, float x, float y)
        {
            Color c = BowColor, d = Mix(BowColor, Color.Black, 0.45f), l = Mix(BowColor, Color.White, 0.35f);
            using (GraphicsPath left = new GraphicsPath())
            using (GraphicsPath right = new GraphicsPath())
            {
                left.AddPolygon(new PointF[] { new PointF(x, y), new PointF(x - 13, y - 9), new PointF(x - 14, y + 6) });
                right.AddPolygon(new PointF[] { new PointF(x, y), new PointF(x + 13, y - 9), new PointF(x + 14, y + 6) });
                using (Pen o = new Pen(d, 2.6f)) { o.LineJoin = LineJoin.Round; g.DrawPath(o, left); g.DrawPath(o, right); }
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, y - 10), new PointF(0, y + 7), l, c))
                { g.FillPath(b, left); g.FillPath(b, right); }
            }
            using (SolidBrush k = new SolidBrush(c)) g.FillEllipse(k, x - 4, y - 4, 8, 8);
            using (Pen o = new Pen(d, 1.6f)) g.DrawEllipse(o, x - 4, y - 4, 8, 8);
        }

        static void DrawFace(Graphics g, Pose p, float hx, float hy)
        {
            State st = p.State;
            float ex = hx + 30, ey = hy + 16;
            bool happy = st == State.Happy || st == State.Drag;
            float eyeK = p.Stage == Life.Baby ? 1.3f : (p.Stage == Life.Juvenile ? 1.12f : 1f);
            bool sleeping = st == State.Sleep;

            using (Pen dark = new Pen(Color.FromArgb(30, 30, 30), 2.4f))
            {
                dark.StartCap = LineCap.Round; dark.EndCap = LineCap.Round;

                if (sleeping)
                    g.DrawArc(dark, ex - 6, ey - 4, 12, 9, 10, 160);
                else if (happy)
                    g.DrawArc(dark, ex - 6, ey - 2, 12, 10, 200, 140);
                else if (p.Blink)
                    g.DrawLine(dark, ex - 6, ey + 1, ex + 6, ey + 1);
                else
                {
                    float q = eyeK;
                    g.FillEllipse(Brushes.White, ex - 7 * q, ey - 8.5f * q, 14 * q, 17 * q);
                    using (Pen ep = new Pen(Color.FromArgb(60, 30, 30, 30), 1f)) g.DrawEllipse(ep, ex - 7 * q, ey - 8.5f * q, 14 * q, 17 * q);
                    float lx = st == State.Roar ? 0 : 1.8f;
                    using (SolidBrush pupil = new SolidBrush(Color.FromArgb(25, 25, 35)))
                        g.FillEllipse(pupil, ex - 4.5f * q + lx, ey - 5.5f * q, 9.5f * q, 11.5f * q);
                    g.FillEllipse(Brushes.White, ex + lx + 0.5f, ey - 4.5f * q, 3.6f * q, 3.6f * q);
                    g.FillEllipse(Brushes.White, ex + lx - 3f * q, ey + 2f * q, 1.8f * q, 1.8f * q);
                }

                // trepavice (ženski dino)
                if (p.Female && !sleeping)
                {
                    using (Pen lp = new Pen(Color.FromArgb(30, 30, 30), 1.8f))
                    {
                        lp.StartCap = LineCap.Round; lp.EndCap = LineCap.Round;
                        float top = (happy || p.Blink) ? ey - 2 : ey - 8;
                        g.DrawLine(lp, ex - 2, top, ex - 4, top - 5);
                        g.DrawLine(lp, ex + 2, top - 0.5f, ex + 2, top - 6);
                        g.DrawLine(lp, ex + 5.5f, top + 0.5f, ex + 8, top - 4);
                    }
                }

                // obrve kad riče
                if (st == State.Roar) g.DrawLine(dark, ex - 7, ey - 13, ex + 6, ey - 9);

                // rumenilo
                using (SolidBrush blush = new SolidBrush(Color.FromArgb(150, 255, 150, 170)))
                    g.FillEllipse(blush, hx + 33, hy + 25, 11, 6);

                // nozdrva
                using (SolidBrush n = new SolidBrush(CNose))
                    g.FillEllipse(n, hx + 49, hy + 10, 3.5f, 2.5f);

                // usta
                if (st == State.Roar || (st == State.Happy && ((int)(p.T * 6)) % 2 == 0))
                {
                    float mw = st == State.Roar ? 18 : 12, mh = st == State.Roar ? 12 : 8;
                    using (GraphicsPath mouth = new GraphicsPath())
                    {
                        mouth.AddEllipse(hx + 52 - mw, hy + 27 - mh / 2f, mw, mh);
                        using (SolidBrush mb = new SolidBrush(Color.FromArgb(120, 30, 40)))
                            g.FillPath(mb, mouth);
                        Region o = g.Clip;
                        g.SetClip(mouth, CombineMode.Intersect);
                        using (SolidBrush tg = new SolidBrush(Color.FromArgb(240, 110, 130)))
                            g.FillEllipse(tg, hx + 52 - mw * 0.75f, hy + 27 + mh * 0.05f, mw * 0.6f, mh * 0.6f);
                        if (st == State.Roar)
                            for (int i = 0; i < 3; i++)
                                g.FillPolygon(Brushes.White, new PointF[] {
                                    new PointF(hx + 52 - mw + 4 + i * 5, hy + 27 - mh / 2f),
                                    new PointF(hx + 52 - mw + 6.5f + i * 5, hy + 27 - mh / 2f + 4),
                                    new PointF(hx + 52 - mw + 9 + i * 5, hy + 27 - mh / 2f) });
                        g.Clip = o;
                    }
                }
                else
                {
                    using (Pen mp = new Pen(CMouth, 2f))
                    {
                        mp.StartCap = LineCap.Round; mp.EndCap = LineCap.Round;
                        g.DrawBezier(mp, hx + 36, hy + 30, hx + 41, hy + 34, hx + 47, hy + 33, hx + 52, hy + 28);
                    }
                }

                // naočale
                if (p.Glasses)
                {
                    using (SolidBrush lens = new SolidBrush(Color.FromArgb(45, 140, 200, 255)))
                        g.FillEllipse(lens, ex - 10.5f, ey - 10.5f, 21, 21);
                    using (Pen gp = new Pen(Color.FromArgb(35, 35, 40), 2.6f))
                    {
                        g.DrawEllipse(gp, ex - 10.5f, ey - 10.5f, 21, 21);
                        g.DrawLine(gp, ex - 10.5f, ey - 2, hx + 2, ey - 4);           // ručica
                        g.DrawArc(gp, ex + 12, ey - 9, 12, 18, -70, 140);              // drugo staklo (iza)
                        g.DrawLine(gp, ex + 10.3f, ey - 3, ex + 15, ey - 4);           // most
                    }
                    using (Pen sh = new Pen(Color.FromArgb(150, 255, 255, 255), 1.6f))
                        g.DrawArc(sh, ex - 7, ey - 7, 14, 14, 200, 50);
                }
            }
        }

        static void DrawChick(Graphics g, float x, float gy, Pose p)
        {
            double ph = p.ChickPhase;
            float hop = (float)Math.Abs(Math.Sin(ph * 7)) * 4f;
            GraphicsState s = g.Save();
            g.TranslateTransform(x, gy - hop);
            if (p.Dir < 0) g.ScaleTransform(-1, 1);
            Color yTop = Color.FromArgb(255, 236, 130), yBot = Color.FromArgb(255, 196, 50), o = Color.FromArgb(190, 130, 20);
            using (Pen leg = new Pen(Color.FromArgb(240, 140, 30), 2f))
            {
                float l = (float)Math.Sin(ph * 14) * 3f;
                g.DrawLine(leg, -3, -4, -3 + l, hop > 0 ? 0 + hop * 0 : 0);
                g.DrawLine(leg, 3, -4, 3 - l, 0);
            }
            using (GraphicsPath b = new GraphicsPath())
            {
                b.AddEllipse(-11, -22, 22, 19);
                b.AddEllipse(-2, -32, 15, 15);
                using (Pen op = new Pen(o, 3f)) g.DrawPath(op, b);
                using (LinearGradientBrush yb = new LinearGradientBrush(new PointF(0, -32), new PointF(0, -3), yTop, yBot))
                    g.FillPath(yb, b);
            }
            using (SolidBrush beak = new SolidBrush(Color.FromArgb(250, 140, 30)))
                g.FillPolygon(beak, new PointF[] { new PointF(12, -27), new PointF(18, -24.5f), new PointF(12, -22) });
            using (SolidBrush eye = new SolidBrush(Color.FromArgb(30, 30, 30)))
                g.FillEllipse(eye, 6, -28, 3.5f, 3.5f);
            using (SolidBrush w = new SolidBrush(Color.FromArgb(120, 230, 160, 40)))
                g.FillEllipse(w, -8, -17, 11, 7);
            g.Restore(s);
        }

        static void DrawZzz(Graphics g, float x, float y, double t)
        {
            for (int i = 0; i < 3; i++)
            {
                double ph = (t * 0.6 + i / 3.0) % 1.0;
                int a = (int)(255 * Math.Sin(ph * Math.PI));
                float sz = 12 + (float)ph * 11;
                using (Font f = new Font(FontFamily.GenericSansSerif, sz, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(Math.Max(0, Math.Min(255, a)), 60, 90, 170)))
                    g.DrawString("z", f, b, x + (float)ph * 22, y - (float)ph * 34);
            }
        }

        static void DrawHearts(Graphics g, float x, float y, double h)
        {
            float[] ox = { -24, 6, 30 };
            float[] delay = { 0, 0.15f, 0.3f };
            for (int i = 0; i < 3; i++)
            {
                double ph = Math.Max(0, Math.Min(1, (h - delay[i]) / 0.7));
                if (ph <= 0 || ph >= 1) continue;
                int a = (int)(255 * Math.Sin(ph * Math.PI));
                float s = 9 + (float)ph * 5;
                float hx = x + ox[i] + (float)Math.Sin(ph * 9) * 3, hy = y - (float)ph * 40;
                using (GraphicsPath hp = new GraphicsPath())
                {
                    hp.AddBezier(hx, hy + s * 0.3f, hx - s * 0.1f, hy - s * 0.3f, hx - s, hy - s * 0.2f, hx - s * 0.9f, hy + s * 0.3f);
                    hp.AddBezier(hx - s * 0.9f, hy + s * 0.3f, hx - s * 0.8f, hy + s * 0.7f, hx - s * 0.2f, hy + s, hx, hy + s * 1.2f);
                    hp.AddBezier(hx, hy + s * 1.2f, hx + s * 0.2f, hy + s, hx + s * 0.8f, hy + s * 0.7f, hx + s * 0.9f, hy + s * 0.3f);
                    hp.AddBezier(hx + s * 0.9f, hy + s * 0.3f, hx + s, hy - s * 0.2f, hx + s * 0.1f, hy - s * 0.3f, hx, hy + s * 0.3f);
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(a, 240, 70, 100))) g.FillPath(b, hp);
                }
            }
        }

        static void DrawBubble(Graphics g, string text, float anchorX, float anchorY, double alpha)
        {
            int A = (int)(255 * Math.Max(0, Math.Min(1, alpha)));
            using (Font f = new Font("Segoe UI", 12.5f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                float maxW = 200;
                SizeF sz = g.MeasureString(text, f, (int)maxW, sf);
                float w = Math.Min(CanvasW - 6, sz.Width + 20), h = sz.Height + 14;
                float x = Math.Max(3, Math.Min(CanvasW - w - 3, anchorX - w / 2f));
                float y = Math.Max(2, anchorY - h - 10);
                float tailX = Math.Max(x + 14, Math.Min(x + w - 14, anchorX));

                using (GraphicsPath gp = RoundRect(x, y, w, h, 11))
                {
                    gp.AddPolygon(new PointF[] { new PointF(tailX - 7, y + h - 1), new PointF(tailX + 2, y + h + 9), new PointF(tailX + 7, y + h - 1) });
                    using (Matrix m = new Matrix()) { }
                    // senka
                    using (GraphicsPath shp = (GraphicsPath)gp.Clone())
                    {
                        using (Matrix m = new Matrix()) { m.Translate(1.5f, 2.5f); shp.Transform(m); }
                        using (SolidBrush sb = new SolidBrush(Color.FromArgb(A * 40 / 255, 0, 0, 0))) g.FillPath(sb, shp);
                    }
                    g.FillPath(new SolidBrush(Color.FromArgb(A * 250 / 255, 255, 255, 255)), gp);
                    using (Pen pen = new Pen(Color.FromArgb(A, 46, 125, 60), 1.8f)) { pen.LineJoin = LineJoin.Round; g.DrawPath(pen, RoundRect(x, y, w, h, 11)); }
                    // ponovo popuni rep da pokrije ivicu
                    using (SolidBrush wb = new SolidBrush(Color.FromArgb(A, 255, 255, 255)))
                        g.FillPolygon(wb, new PointF[] { new PointF(tailX - 6, y + h - 2), new PointF(tailX + 2, y + h + 7.5f), new PointF(tailX + 6, y + h - 2) });
                    using (Pen pen = new Pen(Color.FromArgb(A, 46, 125, 60), 1.8f))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawLines(pen, new PointF[] { new PointF(tailX - 7, y + h), new PointF(tailX + 2, y + h + 9), new PointF(tailX + 7, y + h) });
                    }
                }
                using (SolidBrush tb = new SolidBrush(Color.FromArgb(A, 30, 40, 35)))
                    g.DrawString(text, f, tb, new RectangleF(x + 4, y + 2, w - 8, h - 4), sf);
            }
        }

        public static Icon MakeIcon()
        {
            using (Bitmap bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    g.ScaleTransform(0.55f, 0.55f);
                    g.TranslateTransform(-4, 4);
                    using (GraphicsPath head = RoundRect(2, 8, 54, 40, 18))
                    {
                        using (Pen p = new Pen(COut, 5)) g.DrawPath(p, head);
                        using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, 8), new PointF(0, 48), CTop, CBot)) g.FillPath(b, head);
                    }
                    g.FillEllipse(Brushes.White, 24, 15, 14, 17);
                    g.FillEllipse(Brushes.Black, 28, 18, 9.5f, 11.5f);
                    g.FillEllipse(Brushes.White, 32, 19, 3.6f, 3.6f);
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }
    }

    // ======================================================================
    //  LAYERED WINDOW
    // ======================================================================
    static class Native
    {
        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int x, y; public POINT(int a, int b) { x = a; y = b; } }
        [StructLayout(LayoutKind.Sequential)] public struct SIZE { public int cx, cy; public SIZE(int a, int b) { cx = a; cy = b; } }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        public const int ULW_ALPHA = 2;
        public const byte AC_SRC_OVER = 0, AC_SRC_ALPHA = 1;
    }

    // ======================================================================
    //  FORMA
    // ======================================================================
    class PetForm : Form
    {
        static string AppName { get { return Profile.Id; } }
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const double Dt = 1.0 / 30.0;

        readonly Random rnd = new Random();
        readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        NotifyIcon tray;
        ContextMenuStrip menu;
        ToolStripMenuItem miSleep, miAutostart, miGlasses, miLike, miBlock, miProfiles;
        string lastJoke;   // zadnja ispričana fora (za 👍 / 👎)
        readonly Needs needs = new Needs();
        readonly Triggers triggers = new Triggers();
        StatusForm statusForm;
        double needsSaveT;
        ToolStripMenuItem[] miSize, miTalk;

        float dpiK, sizeMul = 1f, k;
        int cw, ch;
        Bitmap canvas;

        readonly Pose pose = new Pose();
        double stateT, stateDur;
        double fx, fy, vy;
        double nextBlink = 2, blinkUntil;
        double bubbleLeft, bubbleTotal;
        double sinceTalk, nextTalk = 25;
        double heartsStart = -10;
        double chickLeft, nextChick;
        double glassesLeft;
        bool forcedSleep;
        int talkLevel = 1; // 0 tih, 1 normalno, 2 brbljivac
        int lastHourGreeted = -1;

        bool mouseDown, dragging;
        Point downScreen; double downFx, downFy;

        DateTime lastFed;
        string statePath;
        readonly List<string> custom = new List<string>();
        readonly Queue<string> recent = new Queue<string>();

        // AI
        AiConfig ai;
        ChatForm chat;
        readonly List<ChatMsg> history = new List<ChatMsg>();
        bool thinking;
        DateTime aiPauseUntil = DateTime.MinValue;

        // evolucija i dostignuća
        double growAcc, lifeSaveT, dayCheckT, eggTalkT;
        bool veteranNew;

        public PetForm()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) dpiK = g.DpiX / 96f;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Text = Profile.Name;

            string dir0 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
            try { Directory.CreateDirectory(dir0); } catch { }
            statePath = Path.Combine(dir0, "state.txt");
            LoadState();
            needs.Load();
            pose.Female = Settings.Female;
            pose.Glasses = Settings.Glasses;
            pose.Species = Settings.Species;
            pose.Pattern = Settings.Pattern;
            pose.Accessory = Settings.Accessory;
            if (!Life.Exists) { Life.StartAdult(); veteranNew = true; }   // korisnik iz v1.0 ostaje odrasli
            else if (Life.CatchUp(needs.Vacation || needs.RanAway))
            {
                Achievements.Unlock("juvenile");
                if (Life.Stage == Life.Adult) Achievements.Unlock("adult");
            }
            pose.Stage = Life.Stage;
            pose.Warmth = Life.Warmth;
            Achievements.Unlocked += OnAchievement;
            ai = new AiConfig(dir0);
            ai.Persona();
            ApplySize();

            BuildMenu();
            ContextMenuStrip = menu;
            tray = new NotifyIcon();
            tray.Icon = Painter.MakeIcon();
            Icon = tray.Icon;
            tray.Text = Profile.TrayText.Length > 63 ? Profile.TrayText.Substring(0, 63) : Profile.TrayText;
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.MouseClick += delegate(object s, MouseEventArgs e) { if (e.Button == MouseButtons.Left) OpenChat(); };

            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            fx = wa.Left + rnd.Next(Math.Max(1, wa.Width - cw));
            fy = GroundFor(wa);
            nextChick = 240 + rnd.Next(400);

            MouseDown += OnDown;
            MouseMove += OnMove;
            MouseUp += OnUp;
            MouseDoubleClick += delegate(object s, MouseEventArgs e) { if (e.Button == MouseButtons.Left) Feed(); };

            timer.Interval = 33;
            timer.Tick += delegate { Step(); };

            SetState(State.Happy, 2.5);
            if (Life.Stage == Life.Egg) { SetState(State.Idle, 3); Say(Lang.Get("egg.hello")); }
            else Say(Program.JustSetUp ? Lang.Get("pet.hello") : Greeting());
            Achievements.TouchDay();
            if (veteranNew) Achievements.Unlock("veteran");
        }

        // ---------------- evolucija i dostignuća ----------------
        void OnAchievement(Achievement a)
        {
            try
            {
                string r = Achievements.RewardName(a.Reward);
                string msg = r == null ? string.Format(Lang.Get("ach.got"), a.Name) : string.Format(Lang.Get("ach.gotreward"), a.Name, r);
                if (Life.Stage == Life.Egg) return;
                Say(msg);
                heartsStart = pose.T;
                SetState(State.Happy, 2);
                tray.ShowBalloonTip(5000, Profile.Name, msg, ToolTipIcon.Info);
            }
            catch { }
        }

        bool IsEgg { get { return Life.Stage == Life.Egg; } }

        /// Akcije koje jaje ne može: kaže "prvo me izlegni".
        bool EggBlock()
        {
            if (!IsEgg) return false;
            Say(Lang.Get("egg.notyet"));
            SetState(State.Happy, 0.6);
            return true;
        }

        void WarmEgg(double amount, bool talk)
        {
            if (!IsEgg || pose.HatchT >= 0) return;
            double before = Life.Warmth;
            bool ready = Life.Warm(amount);
            if ((before < 50 && Life.Warmth >= 50) || (before < 80 && Life.Warmth >= 80)) { Say(Lang.Get("egg.crack")); SetState(State.Happy, 0.8); }
            else if (talk) { Say(Pick(Lang.Get("egg.click").Split('|'))); SetState(State.Happy, 0.6); }
            if (ready) { pose.HatchT = 0; pose.Bubble = null; bubbleLeft = 0; SetState(State.Idle, 1e9); }
        }

        void LifeStep()
        {
            if (IsEgg)
            {
                if (pose.HatchT >= 0)
                {
                    pose.HatchT += Dt / 2.2;
                    if (pose.HatchT >= 1)
                    {
                        pose.HatchT = -1;
                        Life.Hatched();
                        pose.Stage = Life.Stage;
                        for (int i = 0; i < Needs.Count; i++) needs.Add((Need)i, 100);
                        needs.Save();
                        heartsStart = pose.T;
                        SetState(State.Happy, 3);
                        Say(string.Format(Lang.Get("egg.hatched"), Profile.Name));
                        Achievements.Unlock("hatch");
                        if (Life.Eggs >= 2) Achievements.Unlock("newegg");
                    }
                    return;
                }
                // pasivno grijanje (~3 min samo), miš iznad jajeta grije brže
                bool hover = Bounds.Contains(Cursor.Position);
                WarmEgg(Dt * (hover ? 1.6 : 0.5), false);
                lifeSaveT += Dt;
                if (lifeSaveT > 15) { lifeSaveT = 0; Life.Save(); }
                return;
            }

            // rast: računa se svakih 10 s, brže uz dobru brigu
            if (Life.Stage == Life.Baby || Life.Stage == Life.Juvenile)
            {
                if (!needs.Vacation && !needs.RanAway) growAcc += Dt;
                if (growAcc >= 10)
                {
                    if (Life.Grow(growAcc, needs.Happiness))
                    {
                        pose.Stage = Life.Stage;
                        heartsStart = pose.T;
                        SetState(State.Happy, 3);
                        if (Life.Stage == Life.Juvenile) { Say(Lang.Get("life.grew1")); Achievements.Unlock("juvenile"); }
                        else { Say(Lang.Get("life.grew2")); Achievements.Unlock("adult"); }
                    }
                    growAcc = 0;
                }
            }
            lifeSaveT += Dt;
            if (lifeSaveT > 60)
            {
                lifeSaveT = 0;
                Life.Save();
                if (needs.Happiness >= 95) Achievements.Unlock("happy");
                if (needs.Vacation) Achievements.Unlock("vacation");
            }
            dayCheckT += Dt;
            if (dayCheckT > 300) { dayCheckT = 0; Achievements.TouchDay(); }
        }

        void NewEgg()
        {
            if (MessageBox.Show(Lang.Get("newegg.confirm"), Profile.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Life.StartEgg();
            pose.Stage = Life.Stage; pose.Warmth = 0; pose.HatchT = -1;
            forcedSleep = false;
            SetState(State.Idle, 3);
            Say(Lang.Get("egg.hello"));
        }

        void OpenAchievements()
        {
            using (AchievementsForm f = new AchievementsForm()) f.ShowDialog();
        }

        // ---------------- potrebe / status ----------------
        string NeedLine(Need n) { return Pick(Lang.Gender(Lang.Get("need.say." + Needs.Keys[(int)n])).Split('|')); }

        void OpenStatus()
        {
            if (statusForm != null && !statusForm.IsDisposed) { statusForm.Activate(); return; }
            statusForm = new StatusForm(needs, Profile.Name);
            statusForm.ActionClicked += delegate(string a) { DoAction(a); };
            statusForm.FormClosed += delegate { statusForm = null; needs.Save(); };
            statusForm.Show();
        }

        void DoAction(string a)
        {
            if (IsEgg)
            {
                if (a == "pet") { WarmEgg(12, true); heartsStart = pose.T; }
                else EggBlock();
                return;
            }
            switch (a)
            {
                case "feed": Feed(); break;
                case "water": GiveWater(); break;
                case "pet": Pet(); break;
                case "play": Play(); break;
                case "sleep": if (pose.State != State.Sleep) ToggleSleep(); break;
                case "bath": Bath(); break;
            }
            needs.Save();
        }

        void GiveWater()
        {
            if (EggBlock()) return;
            forcedSleep = false;
            needs.Add(Need.Water, 40);
            Say(Lang.Get("say.water"));
            SetState(State.Happy, 1.6);
        }

        void Play()
        {
            if (EggBlock()) return;
            forcedSleep = false;
            if (needs[Need.Energy] < 10) { Say(Lang.Get("say.tired")); return; }
            needs.Add(Need.Fun, 30); needs.Add(Need.Energy, -6); needs.Add(Need.Love, 5); needs.Add(Need.Hygiene, -4);
            heartsStart = pose.T;
            Say(rnd.Next(2) == 0 ? Lang.Get("say.play") : PickJoke());
            SetState(rnd.Next(2) == 0 ? State.Happy : State.Roar, 2);
        }

        void Bath()
        {
            if (EggBlock()) return;
            forcedSleep = false;
            needs.Add(Need.Hygiene, 100);
            Say(Lang.Get("say.bath"));
            SetState(State.Happy, 2);
        }

        void RunAway()
        {
            needs.RanAway = true;
            needs.Save();
            Hide();
            tray.ShowBalloonTip(6000, Profile.Name, Lang.Get("say.ranaway"), ToolTipIcon.Warning);
        }

        void OpenPacks()
        {
            using (PackManagerForm f = new PackManagerForm())
            {
                f.ShowDialog();
                if (!f.Changed) return;
            }
            Profile.LoadJokes();
            recent.Clear();
            Say(string.Format(Lang.Get("pk.loaded"), Profile.JokeCount));
        }

        void OpenCatalog()
        {
            using (CatalogForm f = new CatalogForm())
            {
                f.ShowDialog();
                if (!f.Changed) return;
            }
            Achievements.Unlock("pack");
            Profile.LoadJokes();
            recent.Clear();
            Say(string.Format(Lang.Get("pk.loaded"), Profile.JokeCount));
        }

        void BuildProfilesMenu()
        {
            miProfiles.DropDownItems.Clear();
            List<Packs.SavedProfile> list = Packs.LoadProfiles();
            miProfiles.Enabled = list.Count > 0;
            foreach (Packs.SavedProfile sp in list)
            {
                Packs.SavedProfile p = sp;
                ToolStripMenuItem it = new ToolStripMenuItem(p.Name, null, delegate
                {
                    Packs.Activate(p);
                    Profile.LoadJokes();
                    recent.Clear();
                    Say(string.Format(Lang.Get("pk.switched"), p.Name));
                });
                it.Checked = p.Name.Equals(Settings.ActiveProfile, StringComparison.OrdinalIgnoreCase);
                miProfiles.DropDownItems.Add(it);
            }
        }

        void OpenSettings()
        {
            string look0 = Settings.Species + "/" + Settings.Color1 + "/" + Settings.Color2 + "/" + Settings.Pattern + "/" + Settings.Accessory + "/" + Settings.Glasses;
            using (SetupWizard w = new SetupWizard(false, ai))
            {
                if (w.ShowDialog() != DialogResult.OK) return;
            }
            pose.Female = Settings.Female;
            pose.Glasses = Settings.Glasses;
            pose.Species = Settings.Species;
            pose.Pattern = Settings.Pattern;
            pose.Accessory = Settings.Accessory;
            // jezik se možda promijenio: napravi meni ponovo
            ContextMenuStrip old = menu;
            BuildMenu();
            ContextMenuStrip = menu;
            tray.ContextMenuStrip = menu;
            if (old != null) old.Dispose();
            Text = Profile.Name;
            tray.Text = Profile.TrayText.Length > 63 ? Profile.TrayText.Substring(0, 63) : Profile.TrayText;
            SaveState();
            SetState(State.Happy, 2.5);
            Say(IsEgg ? Lang.Get("egg.hello") : Lang.Get("pet.updated"));
            string look1 = Settings.Species + "/" + Settings.Color1 + "/" + Settings.Color2 + "/" + Settings.Pattern + "/" + Settings.Accessory + "/" + Settings.Glasses;
            if (look1 != look0) Achievements.Unlock("stylist");
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000 | 0x80; // WS_EX_LAYERED | WS_EX_TOOLWINDOW
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Present();
            timer.Start();
            if (needs.RanAway) { Hide(); tray.ShowBalloonTip(5000, Profile.Name, Lang.Get("say.ranaway"), ToolTipIcon.Warning); }
        }

        void ApplySize()
        {
            k = dpiK * sizeMul;
            cw = (int)Math.Ceiling(Painter.CanvasW * k);
            ch = (int)Math.Ceiling(Painter.CanvasH * k);
            if (canvas != null) canvas.Dispose();
            canvas = new Bitmap(cw, ch, PixelFormat.Format32bppArgb);
            Size = new Size(cw, ch);
        }

        // ---------------- meni ----------------
        void BuildMenu()
        {
            menu = new ContextMenuStrip();
            ToolStripMenuItem miChat = new ToolStripMenuItem(Lang.Get("m.chat"), null, delegate { OpenChat(); });
            miChat.Font = new Font(miChat.Font, FontStyle.Bold);
            menu.Items.Add(miChat);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Lang.Get("m.joke"), null, delegate { TellJoke(true); });
            menu.Items.Add(Lang.Get("st.menu"), null, delegate { OpenStatus(); });
            menu.Items.Add(Lang.Get("act.feed"), null, delegate { DoAction("feed"); });
            menu.Items.Add(Lang.Get("act.water"), null, delegate { DoAction("water"); });
            menu.Items.Add(Lang.Get("act.pet"), null, delegate { DoAction("pet"); });
            menu.Items.Add(Lang.Get("act.play"), null, delegate { DoAction("play"); });
            menu.Items.Add(Lang.Get("act.bath"), null, delegate { DoAction("bath"); });
            menu.Items.Add(Lang.Get("m.roar"), null, delegate { Roar(); });
            menu.Items.Add(Lang.Get("m.chick"), null, delegate { CallChick(); });
            miGlasses = new ToolStripMenuItem(Lang.Get("m.glasses"), null, delegate { ToggleGlasses(); });
            menu.Items.Add(miGlasses);
            miSleep = new ToolStripMenuItem(Lang.Get("m.sleep"), null, delegate { ToggleSleep(); });
            menu.Items.Add(miSleep);
            menu.Items.Add(Lang.Get("m.summon"), null, delegate { Summon(); });
            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem size = new ToolStripMenuItem(Lang.Get("m.size"));
            string[] sn = Lang.Get("m.sizes").Split('|');
            float[] sv = { 0.75f, 1f, 1.4f };
            miSize = new ToolStripMenuItem[3];
            for (int i = 0; i < 3; i++)
            {
                float v = sv[i];
                miSize[i] = new ToolStripMenuItem(sn[i], null, delegate { sizeMul = v; ApplySize(); SaveState(); });
                size.DropDownItems.Add(miSize[i]);
            }
            menu.Items.Add(size);

            ToolStripMenuItem talk = new ToolStripMenuItem(Lang.Get("m.talk"));
            string[] tn = Lang.Get("m.talks").Split('|');
            miTalk = new ToolStripMenuItem[3];
            for (int i = 0; i < 3; i++)
            {
                int v = i;
                miTalk[i] = new ToolStripMenuItem(tn[i], null, delegate { talkLevel = v; nextTalk = TalkInterval(); SaveState(); });
                talk.DropDownItems.Add(miTalk[i]);
            }
            menu.Items.Add(talk);

            ToolStripMenuItem aiMenu = new ToolStripMenuItem("AI");
            aiMenu.DropDownItems.Add(Lang.Get("m.aiset"), null, delegate { OpenAiSettings(); });
            aiMenu.DropDownItems.Add(Lang.Get("m.persona"), null, delegate { EditPersona(); });
            aiMenu.DropDownItems.Add(Lang.Get("m.clear"), null, delegate { history.Clear(); if (chat != null) chat.AppendNote(Lang.Get("s.cleared")); Say(Lang.Get("s.forgot")); });
            menu.Items.Add(aiMenu);
            ToolStripMenuItem fun = new ToolStripMenuItem(Lang.Get("m.jokes"));
            miLike = new ToolStripMenuItem(Lang.Get("pk.like"), null, delegate { if (lastJoke != null) { Packs.Like(lastJoke); Say(Lang.Get("pk.liked")); Profile.LoadJokes(); Achievements.Add("like", 1); } });
            miBlock = new ToolStripMenuItem(Lang.Get("pk.block"), null, delegate { if (lastJoke != null) { Packs.Block(lastJoke); lastJoke = null; recent.Clear(); Profile.LoadJokes(); Say(Lang.Get("pk.blocked")); } });
            fun.DropDownItems.Add(miLike);
            fun.DropDownItems.Add(miBlock);
            fun.DropDownItems.Add(new ToolStripSeparator());
            fun.DropDownItems.Add(Lang.Get("pk.menu"), null, delegate { OpenPacks(); });
            fun.DropDownItems.Add(Lang.Get("cat.menu"), null, delegate { OpenCatalog(); });
            miProfiles = new ToolStripMenuItem(Lang.Get("pk.profiles"));
            fun.DropDownItems.Add(miProfiles);
            fun.DropDownItems.Add(new ToolStripSeparator());
            fun.DropDownItems.Add(Lang.Get("m.jokefolder"), null, delegate { OpenCustom(); });
            fun.DropDownItems.Add(Lang.Get("m.reload"), null, delegate { Profile.LoadJokes(); recent.Clear(); Say(string.Format(Lang.Get("pk.loaded"), Profile.JokeCount)); });
            menu.Items.Add(fun);
            menu.Items.Add(Lang.Get("m.achievements"), null, delegate { OpenAchievements(); });
            menu.Items.Add(Lang.Get("menu.settings"), null, delegate { OpenSettings(); });
            miAutostart = new ToolStripMenuItem(Lang.Get("s.autostart"), null, delegate { ToggleAutostart(); });
            menu.Items.Add(miAutostart);
            menu.Items.Add(Lang.Get("m.newegg"), null, delegate { NewEgg(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Lang.Get("m.exit"), null, delegate { Close(); });
            menu.Opening += delegate
            {
                miSleep.Text = pose.State == State.Sleep ? Lang.Get("m.wake") : Lang.Get("m.sleep");
                miGlasses.Checked = pose.Glasses;
                miLike.Enabled = miBlock.Enabled = lastJoke != null;
                BuildProfilesMenu();
                miAutostart.Checked = IsAutostart();
                for (int i = 0; i < 3; i++) miTalk[i].Checked = talkLevel == i;
                miSize[0].Checked = sizeMul < 0.9f; miSize[1].Checked = sizeMul >= 0.9f && sizeMul < 1.2f; miSize[2].Checked = sizeMul >= 1.2f;
            };
        }

        bool IsAutostart()
        {
            if (Packaged.IsPackaged) return false;
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(RunKey, false))
                    return rk != null && rk.GetValue(AppName) != null;
            }
            catch { return false; }
        }

        void ToggleAutostart()
        {
            if (Packaged.IsPackaged) { Packaged.OpenStartupSettings(); Say(Lang.Get("s.autostart.store")); return; }
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (IsAutostart()) { rk.DeleteValue(AppName, false); Say(Lang.Get("s.noauto")); }
                    else { rk.SetValue(AppName, "\"" + Application.ExecutablePath + "\""); Say(Lang.Get("s.auto")); }
                }
            }
            catch (Exception ex) { Say(Lang.Get("s.error") + ex.Message); }
        }

        // Moje fore = korisnički paket "my-jokes" u %APPDATA%\DinoJoe\packs\my-jokes
        static string MyPackDir { get { return Path.Combine(Packs.UserDir, "my-jokes"); } }
        static string MyJokesDir { get { return Path.Combine(MyPackDir, "jokes"); } }
        string CustomPath() { return Path.Combine(MyJokesDir, "my-jokes.txt"); }

        void LoadCustom()
        {
            custom.Clear();
            try
            {
                string p = CustomPath();
                if (!File.Exists(p)) return;
                foreach (string line in File.ReadAllLines(p, Encoding.UTF8))
                {
                    string l = line.Trim();
                    if (l.Length > 0 && !l.StartsWith("#")) custom.Add(l);
                }
            }
            catch { }
        }

        void OpenCustom()
        {
            try
            {
                string p = CustomPath();
                Directory.CreateDirectory(MyJokesDir);
                string ini = Path.Combine(MyPackDir, "pack.ini");
                if (!File.Exists(ini))
                    File.WriteAllText(ini, "# Dino Joe joke pack (UTF-8)\r\nid=my-jokes\r\nname=" + Lang.Get("my.name") + "\r\nlang=" + Settings.LangCode + "\r\nauthor=" + Environment.UserName + "\r\nversion=1.0\r\ndescription=" + Lang.Get("my.desc") + "\r\n", new UTF8Encoding(true));
                if (!File.Exists(p))
                    File.WriteAllText(p, Lang.Get("my.header"), new UTF8Encoding(true));
                if (!Packs.IsEnabled("my-jokes")) { Packs.SetEnabled("my-jokes", true); Settings.Save(); }
                System.Diagnostics.Process.Start("explorer.exe", "\"" + MyJokesDir + "\"");
                Say(Lang.Get("s.write"));
            }
            catch (Exception ex) { Say(Lang.Get("s.error") + ex.Message); }
        }

        // ---------------- stanje ----------------
        void LoadState()
        {
            lastFed = DateTime.Now;
            try
            {
                if (!File.Exists(statePath)) return;
                foreach (string line in File.ReadAllLines(statePath))
                {
                    string[] kv = line.Split(new char[] { '=' }, 2);
                    if (kv.Length == 1) { long tt; if (long.TryParse(kv[0].Trim(), out tt)) lastFed = new DateTime(tt); continue; }
                    string key = kv[0].Trim(), v = kv[1].Trim();
                    long l; float f; int i;
                    if (key == "fed" && long.TryParse(v, out l)) lastFed = new DateTime(l);
                    else if (key == "size" && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f)) sizeMul = f;
                    else if (key == "talk" && int.TryParse(v, out i)) talkLevel = Math.Max(0, Math.Min(2, i));
                    else if (key == "glasses") pose.Glasses = v == "1";
                }
            }
            catch { }
        }

        void SaveState()
        {
            try
            {
                File.WriteAllText(statePath,
                    "fed=" + lastFed.Ticks + "\r\n" +
                    "size=" + sizeMul.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n" +
                    "talk=" + talkLevel + "\r\n" +
                    "glasses=" + (pose.Glasses ? "1" : "0") + "\r\n");
            }
            catch { }
        }

        int Hunger() { return (int)Math.Min(100, Math.Max(0, (DateTime.Now - lastFed).TotalMinutes / 3.0)); }

        double TalkInterval()
        {
            if (talkLevel == 0) return 300 + rnd.Next(300);
            if (talkLevel == 2) return 25 + rnd.Next(35);
            return 70 + rnd.Next(110);
        }

        // ---------------- akcije ----------------
        string Pick(string[] arr) { return arr[rnd.Next(arr.Length)]; }

        string PickJoke()
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                int r = rnd.Next(100);
                string s;
                if (pose.Glasses && r < 20) s = Profile.Special("glasses", Jokes.Glasses);
                else s = Profile.RandomJoke();
                if (!recent.Contains(s) || attempt == 7)
                {
                    Achievements.Add("jokes", 1);
                    lastJoke = s;
                    recent.Enqueue(s);
                    while (recent.Count > 25) recent.Dequeue();
                    return s;
                }
            }
            return Profile.RandomJoke();
        }

        /// Pozdrav: aktivni paket (greeting.<key>), pa dino.ini pored exe-a, pa podrazumijevani. Varijante sa |.
        string G(string packKey, string iniKey, string def)
        {
            string v = Packs.ActiveValue("greeting." + packKey);
            if (v == null) return Profile.GetPick(iniKey, def);
            string[] parts = Lang.Gender(v).Split('|');
            return parts[rnd.Next(parts.Length)].Trim();
        }

        string Greeting()
        {
            DateTime n = DateTime.Now;
            int h = n.Hour;
            if (n.DayOfWeek == DayOfWeek.Monday && h < 12) return G("monday", "pozdrav_ponedjeljak", Lang.Get("g.mon"));
            if (n.DayOfWeek == DayOfWeek.Friday && h >= 12) return G("friday", "pozdrav_petak", Lang.Get("g.fri"));
            if (h < 10) return G("morning", "pozdrav_jutro", Lang.Get("g.morning"));
            if (h < 17) return G("day", "pozdrav_dan", string.Format(Lang.Get("g.day"), Profile.Name));
            if (h < 22) return G("evening", "pozdrav_vece", Lang.Get("g.eve"));
            return G("night", "pozdrav_noc", Lang.Get("g.night"));
        }

        void TellJoke(bool manual)
        {
            if (IsEgg) { if (manual) EggBlock(); return; }
            if (pose.State == State.Sleep) { forcedSleep = false; SetState(State.Idle, 2); }
            Say(PickJoke());
            needs.Add(Need.Fun, manual ? 4 : 1.5);
            if (manual || rnd.Next(3) == 0) SetState(State.Happy, 1.6);
        }

        void Feed()
        {
            if (IsEgg) { WarmEgg(6, true); return; }
            int h = 100 - (int)needs[Need.Food];
            needs.Add(Need.Food, 35);
            Achievements.Add("feed", 1);
            lastFed = DateTime.Now; SaveState();
            forcedSleep = false;
            heartsStart = pose.T;
            Say(h > 60 ? Lang.Get("s.njam") : (h > 15 ? Profile.Special("food", Jokes.Feed) : Lang.Get("s.full")));
            SetState(State.Happy, 2.2);
        }

        void Pet()
        {
            forcedSleep = false;
            needs.Add(Need.Love, 25);
            Achievements.Add("pet", 1);
            heartsStart = pose.T;
            Say("Prrr... <3");
            SetState(State.Happy, 2);
        }

        void Roar()
        {
            if (EggBlock()) return;
            forcedSleep = false;
            Say("ROAAAAAR!");
            SetState(State.Roar, 1.6);
        }

        void CallChick()
        {
            chickLeft = 50 + rnd.Next(40);
            Say(Profile.Special("chick", Jokes.Chick));
        }

        void ToggleGlasses()
        {
            pose.Glasses = !pose.Glasses;
            Settings.Glasses = pose.Glasses; Settings.Save();
            glassesLeft = 0;
            SaveState();
            Say(pose.Glasses ? Profile.Special("glasses", Jokes.Glasses) : Lang.Get("s.noglasses"));
        }

        void ToggleSleep()
        {
            if (EggBlock()) return;
            if (pose.State == State.Sleep) { forcedSleep = false; Say(Lang.Get("s.morning")); SetState(State.Idle, 2); }
            else { forcedSleep = true; Say(Lang.Get("s.night")); SetState(State.Sleep, 1e9); }
        }

        void Summon()
        {
            Point p = Cursor.Position;
            Rectangle wa = Screen.FromPoint(p).WorkingArea;
            fx = p.X - cw / 2.0; fy = wa.Top; vy = 0;
            forcedSleep = false;
            SetState(State.Fall, 0);
            if (needs.RanAway)
            {
                needs.RanAway = false;
                needs.Add(Need.Food, 25); needs.Add(Need.Water, 25); needs.Add(Need.Health, 20);
                needs.Save();
                Show();
                Say(Lang.Get("say.back"));
                return;
            }
            Say(Lang.Get("s.here"));
        }

        // ---------------- AI chat ----------------
        bool AiActive() { return ai.Ready && DateTime.Now >= aiPauseUntil; }

        void OpenChat()
        {
            if (chat == null)
            {
                chat = new ChatForm(Ask);
                chat.AppendNote(ai.Ready ? string.Format(Lang.Get("s.aion"), ai.Model)
                                         : Lang.Get("s.aioff"));
            }
            if (pose.State == State.Sleep) { forcedSleep = false; SetState(State.Idle, 3); }
            chat.ShowNear(new Rectangle((int)fx, (int)fy, cw, ch));
        }

        void Ask(string q)
        {
            chat.Append(Lang.Get("s.you"), q, false);
            if (pose.State == State.Sleep) { forcedSleep = false; SetState(State.Idle, 3); }

            if (!AiActive())
            {
                string a = OfflineBrain.Reply(q);
                chat.Append(Profile.Name, a, true);
                SayAnswer(a);
                return;
            }

            history.Add(new ChatMsg("user", q));
            while (history.Count > 10) history.RemoveAt(0);
            if (history.Count > 0 && history[0].Role == "assistant") history.RemoveAt(0);
            List<ChatMsg> snap = new List<ChatMsg>(history);
            string key = ai.Key, persona = ai.Persona();

            thinking = true;
            chat.SetBusy(true);
            SetState(State.Idle, 60);
            ThreadPool.QueueUserWorkItem(delegate
            {
                AiResult r = AiClient.Ask(ai, key, persona, snap, 350);
                try { BeginInvoke((MethodInvoker)delegate { OnAnswer(r, q); }); } catch { }
            });
        }

        void OnAnswer(AiResult r, string q)
        {
            thinking = false;
            pose.Bubble = null; bubbleLeft = 0;
            if (chat != null) chat.SetBusy(false);

            if (r.Ok)
            {
                history.Add(new ChatMsg("assistant", r.Text));
                Achievements.Unlock("ai");
                chat.Append(Profile.Name, r.Text, true);
                SayAnswer(r.Text);
                SetState(State.Happy, 1.2);
                return;
            }

            if (history.Count > 0 && history[history.Count - 1].Role == "user") history.RemoveAt(history.Count - 1);
            string note;
            if (r.Status == 401 || r.Status == 403) note = Lang.Get("s.badkey");
            else if (r.Status == 429) { note = Lang.Get("s.limit"); aiPauseUntil = DateTime.Now.AddMinutes(5); }
            else if (r.Status == 0) { note = Lang.Get("s.nonet"); aiPauseUntil = DateTime.Now.AddMinutes(1); }
            else note = Lang.Get("s.aierr");
            chat.AppendNote(note + "  [" + r.Text + "]");
            string a = OfflineBrain.Reply(q);
            chat.Append(Profile.Name, a, true);
            SayAnswer(a);
        }

        void SayAnswer(string text)
        {
            string t = text.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Trim();
            if (t.Length > 170)
            {
                int cut = t.LastIndexOf(' ', 165);
                t = t.Substring(0, cut > 100 ? cut : 165) + Lang.Get("s.seechat");
            }
            Say(t);
        }

        void OpenAiSettings()
        {
            using (AiSettingsForm f = new AiSettingsForm(ai)) f.ShowDialog();
            ai.Prompted = true;
            ai.Save();
            aiPauseUntil = DateTime.MinValue;
            if (ai.Ready) Say(Lang.Get("s.connected"));
            if (chat != null) chat.AppendNote(ai.Ready ? string.Format(Lang.Get("s.aion2"), ai.Model) : Lang.Get("s.ai.off"));
        }

        void EditPersona()
        {
            try
            {
                ai.Persona();
                System.Diagnostics.Process.Start("notepad.exe", "\"" + ai.PersonaPath + "\"");
                Say(Lang.Get("s.persona"));
            }
            catch (Exception ex) { Say(Lang.Get("s.error") + ex.Message); }
        }

        void Say(string text)
        {
            text = Lang.Gender(text ?? "");
            pose.Bubble = text;
            bubbleTotal = bubbleLeft = 2.2 + text.Length * 0.065;
            sinceTalk = 0;
            nextTalk = TalkInterval();
        }

        void SetState(State s, double dur)
        {
            pose.State = s; stateT = 0; stateDur = dur;
            if (s == State.Walk && rnd.Next(3) == 0) pose.Dir = -pose.Dir;
        }

        /// Postoji li ekran odmah lijevo (dir -1) ili desno (dir 1) od ekrana na kojem je dino, sa vertikalnim preklapanjem.
        bool HasNeighbor(int dir)
        {
            Screen cur = Screen.FromPoint(new Point((int)(fx + cw / 2), (int)(fy + ch / 2)));
            Rectangle b = cur.Bounds;
            foreach (Screen sc in Screen.AllScreens)
            {
                if (sc.Equals(cur)) continue;
                Rectangle o = sc.Bounds;
                bool touches = dir > 0 ? Math.Abs(o.Left - b.Right) <= 8 : Math.Abs(o.Right - b.Left) <= 8;
                bool overlap = o.Top < b.Bottom && o.Bottom > b.Top;
                if (touches && overlap) return true;
            }
            return false;
        }

        Rectangle CurrentArea() { return Screen.FromPoint(new Point((int)(fx + cw / 2), (int)(fy + ch / 2))).WorkingArea; }
        double GroundFor(Rectangle wa) { return wa.Bottom - ch + 2 * k; }

        void NextRandomState()
        {
            if (IsEgg) { SetState(State.Idle, 3); return; }
            int hour = DateTime.Now.Hour;
            bool night = hour >= 23 || hour < 6;
            int r = rnd.Next(100);
            if (night && r < 55) { SetState(State.Sleep, 40 + rnd.Next(80)); return; }
            if (needs[Need.Energy] < 20 && r < 60) { SetState(State.Sleep, 60 + rnd.Next(120)); return; }
            if (needs.Sick && r < 45) { SetState(State.Sit, 8 + rnd.Next(10)); return; }
            if (r < 52) SetState(State.Walk, 3 + rnd.Next(7));
            else if (r < 70) SetState(State.Idle, 2 + rnd.Next(4));
            else if (r < 85) SetState(State.Sit, 4 + rnd.Next(10));
            else if (r < 96) SetState(State.Sleep, 15 + rnd.Next(30));
            else { SetState(State.Roar, 1.4); Say(rnd.Next(2) == 0 ? "ROAAAR!" : Lang.Get("s.roarsorry")); }
        }

        // ---------------- petlja ----------------
        void Step()
        {
            // potrebe
            if (!IsEgg) needs.Tick(Dt, pose.State == State.Sleep);
            needsSaveT += Dt;
            if (needsSaveT > 60) { needsSaveT = 0; needs.Save(); }
            if (needs.RanAway) return;
            if (!IsEgg && needs.ShouldRunAway && pose.Bubble == null) { RunAway(); return; }
            LifeStep();
            pose.Warmth = Life.Warmth;

            pose.T += Dt; stateT += Dt; sinceTalk += Dt;
            Rectangle wa = CurrentArea();
            double ground = GroundFor(wa);
            State st = pose.State;

            // treptanje
            if (pose.T >= nextBlink) { blinkUntil = pose.T + 0.13; nextBlink = pose.T + 2 + rnd.NextDouble() * 4; }
            pose.Blink = pose.T < blinkUntil;

            // razmišlja (čeka AI)
            if (thinking)
            {
                pose.Bubble = "Hmm" + new string('.', 1 + ((int)(pose.T * 3)) % 3);
                bubbleTotal = 10; bubbleLeft = 5; pose.BubbleAlpha = 1;
            }

            // oblačić
            if (bubbleLeft > 0)
            {
                bubbleLeft -= Dt;
                double elapsed = bubbleTotal - bubbleLeft;
                pose.BubbleAlpha = Math.Min(1, Math.Min(elapsed / 0.15, bubbleLeft / 0.3));
                if (bubbleLeft <= 0) pose.Bubble = null;
            }

            // srca
            double hT = (pose.T - heartsStart) / 1.8;
            pose.HeartsT = hT >= 0 && hT <= 1 ? hT : -1;

            // pile
            if (chickLeft > 0) { chickLeft -= Dt; pose.Chick = true; if (st == State.Walk) pose.ChickPhase += Dt; }
            else
            {
                pose.Chick = false;
                if (pose.T > nextChick && st != State.Sleep && !IsEgg)
                {
                    nextChick = pose.T + 600 + rnd.Next(900);
                    CallChick();
                }
            }

            // povremene naočale
            if (glassesLeft > 0) { glassesLeft -= Dt; if (glassesLeft <= 0) pose.Glasses = Settings.Glasses; }
            else if (!pose.Glasses && st != State.Sleep && !IsEgg && rnd.Next(30 * 60 * 20) == 0)
            {
                pose.Glasses = true; glassesLeft = 90 + rnd.Next(120);
                Say(Profile.Special("glasses", Jokes.Glasses));
            }

            // fizika i kretanje
            pose.JumpY = 0;
            switch (st)
            {
                case State.Walk:
                    fx += pose.Dir * 1.7 * k;
                    // više monitora: na ivici se okreće samo ako sa te strane nema susjednog ekrana
                    if (fx < wa.Left - 40 * k && !HasNeighbor(-1)) { fx = wa.Left - 40 * k; pose.Dir = 1; }
                    if (fx > wa.Right - cw + 40 * k && !HasNeighbor(1)) { fx = wa.Right - cw + 40 * k; pose.Dir = -1; }
                    if (fy < ground - 2) { vy = 0; SetState(State.Fall, 0); }   // prešao na viši/niži ekran: padne na njegovo tlo
                    else fy = ground;
                    break;
                case State.Fall:
                    vy += 0.9 * k;
                    fy += vy;
                    if (fy >= ground)
                    {
                        fy = ground; vy = 0;
                        Say(rnd.Next(2) == 0 ? "Uf!" : Lang.Get("s.floor"));
                        SetState(State.Idle, 1.2);
                    }
                    break;
                case State.Drag:
                    break;
                case State.Happy:
                    pose.JumpY = Math.Abs(Math.Sin(stateT * 7)) * 10;
                    fy = ground;
                    break;
                case State.Roar:
                    fy = ground;
                    break;
                default:
                    if (fy < ground - 2) { vy = 0; SetState(State.Fall, 0); }
                    else fy = ground;
                    break;
            }

            if (st != State.Drag && st != State.Fall && stateT >= stateDur && !forcedSleep)
            {
                if (st == State.Sleep) Say(Lang.Get("s.yawn"));
                NextRandomState();
            }

            // pričanje
            if (IsEgg)
            {
                eggTalkT += Dt;
                if (pose.Bubble == null && pose.HatchT < 0 && eggTalkT > 45) { eggTalkT = 0; Say(Pick(Lang.Get("egg.click").Split('|'))); }
            }
            else if (pose.Bubble == null && !thinking && !(chat != null && chat.Visible) && st != State.Sleep && st != State.Drag && st != State.Fall)
            {
                DateTime n = DateTime.Now;
                string trig;
                if (n.Hour != lastHourGreeted && n.Minute == 0)
                {
                    lastHourGreeted = n.Hour;
                    if (n.Hour == 12) Say(G("noon", "podne", Lang.Get("g.noon")));
                    else if (n.Hour == 15 && n.DayOfWeek != DayOfWeek.Saturday && n.DayOfWeek != DayOfWeek.Sunday) Say(G("endwork", "kraj_radnog", Lang.Get("g.endwork")));
                }
                else if ((trig = triggers.Step(Dt)) != null)
                {
                    Say(trig); SetState(State.Happy, 1.5);
                    if (triggers.LastKey == "trg.midnight") Achievements.Unlock("night");
                    else if (triggers.LastKey == "trg.monday") Achievements.Unlock("monday");
                }
                else if (needs[needs.Lowest] < 30 && !needs.Vacation && sinceTalk > (Needs.Hardcore ? 45 : 100) && rnd.Next(200) == 0) Say(NeedLine(needs.Lowest));
                else if (sinceTalk > nextTalk) TellJoke(false);
            }

            if ((int)(pose.T * 30) % 150 == 0) Native.SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010);

            Present();
        }

        void Present()
        {
            if (!dragging) { }
            using (Graphics g = Graphics.FromImage(canvas))
                Painter.Render(g, k, pose);

            IntPtr screenDc = Native.GetDC(IntPtr.Zero);
            IntPtr memDc = Native.CreateCompatibleDC(screenDc);
            IntPtr hBmp = IntPtr.Zero, oldBmp = IntPtr.Zero;
            try
            {
                hBmp = canvas.GetHbitmap(Color.FromArgb(0));
                oldBmp = Native.SelectObject(memDc, hBmp);
                Native.SIZE sz = new Native.SIZE(cw, ch);
                Native.POINT src = new Native.POINT(0, 0);
                Native.POINT dst = new Native.POINT((int)fx, (int)fy);
                Native.BLENDFUNCTION bf = new Native.BLENDFUNCTION();
                bf.BlendOp = Native.AC_SRC_OVER; bf.BlendFlags = 0; bf.SourceConstantAlpha = 255; bf.AlphaFormat = Native.AC_SRC_ALPHA;
                Native.UpdateLayeredWindow(Handle, screenDc, ref dst, ref sz, memDc, ref src, 0, ref bf, Native.ULW_ALPHA);
            }
            finally
            {
                Native.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBmp != IntPtr.Zero) { Native.SelectObject(memDc, oldBmp); Native.DeleteObject(hBmp); }
                Native.DeleteDC(memDc);
            }
        }

        // ---------------- miš ----------------
        void OnDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            mouseDown = true; dragging = false;
            downScreen = Cursor.Position; downFx = fx; downFy = fy;
        }

        void OnMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            Point p = Cursor.Position;
            int dx = p.X - downScreen.X, dy = p.Y - downScreen.Y;
            if (!dragging && (Math.Abs(dx) > 4 || Math.Abs(dy) > 4))
            {
                dragging = true; forcedSleep = false;
                SetState(State.Drag, 0);
                if (pose.HatchT < 0) Say("Wiii!");
            }
            if (dragging) { fx = downFx + dx; fy = downFy + dy; }
        }

        void OnUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            bool wasDrag = dragging;
            mouseDown = false; dragging = false;
            if (wasDrag) { vy = 0; SetState(State.Fall, 0); }
            else if (IsEgg) WarmEgg(6, true);
            else if (pose.State == State.Sleep) { forcedSleep = false; Say(Lang.Get("s.notsleep")); SetState(State.Idle, 2); }
            else
            {
                if (rnd.Next(3) == 0) Say(Profile.Special("poke", Jokes.Poke)); else Say(PickJoke());
                SetState(State.Happy, 1.2);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveState();
            needs.Save();
            Life.Save();
            Achievements.Save();
            timer.Stop();
            if (tray != null) { tray.Visible = false; tray.Dispose(); }
            if (chat != null) chat.Dispose();
            base.OnFormClosing(e);
        }
    }

    // ======================================================================
    //  PREVIEW (dev): ODjordje.exe --preview <folder>
    // ======================================================================
    static class Preview
    {
        public static void Run(string dir)
        {
            Directory.CreateDirectory(dir);
            float k = 2f;
            int w = (int)(Painter.CanvasW * k), h = (int)(Painter.CanvasH * k);
            object[][] shots = {
                new object[] { "walk1", State.Walk, 0.0, false, false, null },
                new object[] { "walk2", State.Walk, 0.18, false, true, null },
                new object[] { "idle", State.Idle, 0.5, true, false, "Pangea - prva i jedina uspješna komasacija." },
                new object[] { "sit", State.Sit, 0.5, false, false, null },
                new object[] { "sleep", State.Sleep, 0.7, false, false, null },
                new object[] { "happy", State.Happy, 0.0, false, false, "Njam njam!" },
                new object[] { "roar", State.Roar, 0.1, false, false, "ROAAAAAR!" },
            };
            using (Bitmap sheet = new Bitmap(w * 4, h * 2))
            using (Graphics sg = Graphics.FromImage(sheet))
            {
                sg.Clear(Color.FromArgb(70, 90, 120));
                for (int i = 0; i < shots.Length; i++)
                {
                    object[] s = shots[i];
                    Pose p = new Pose();
                    p.State = (State)s[1]; p.T = (double)s[2]; p.Glasses = (bool)s[3]; p.Chick = (bool)s[4];
                    p.Bubble = (string)s[5];
                    if (p.State == State.Happy) { p.HeartsT = 0.45; p.JumpY = 6; }
                    if (i == 1) p.Dir = -1;
                    using (Bitmap b = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(b)) Painter.Render(g, k, p);
                        b.Save(Path.Combine(dir, s[0] + ".png"), ImageFormat.Png);
                        sg.DrawImage(b, (i % 4) * w, (i / 4) * h, w, h);
                    }
                }
                sheet.Save(Path.Combine(dir, "sheet.png"), ImageFormat.Png);
            }
        }
    }
}

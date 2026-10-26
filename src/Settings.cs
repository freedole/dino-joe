// Dino Joe - korisnička podešavanja (%APPDATA%\DinoJoe\settings.ini), jezici, ličnosti, palete boja.
// C# 5 kompatibilno (csc iz .NET Framework 4.x).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DinoJoe
{
    // ======================================================================
    //  JEZICI
    // ======================================================================
    static class Lang
    {
        // indeksi u nizovima prevoda
        // Bs (bosanski) koristi ijekavske prevode + izuzetke iz BsOverride
        public const int SrIjek = 0, SrEkav = 1, Hr = 2, En = 3, Bs = 4;
        public static readonly string[] Codes = { "sr-ijek", "sr-ekav", "hr", "en", "bs" };
        public static readonly string[] Names = { "Srpski (ijekavica)", "Srpski (ekavica)", "Hrvatski", "English", "Bosanski" };
        public static readonly int[] Order = { SrIjek, SrEkav, Bs, Hr, En };   // redoslijed u čarobnjaku

        // indeks u nizovima prevoda (bosanski -> ijekavica)
        public static int Idx { get { return Current == Bs ? SrIjek : Current; } }

        static readonly Dictionary<string, string> BsOverride = new Dictionary<string, string>();

        public static int Current = SrIjek;

        public static int FromCode(string code)
        {
            for (int i = 0; i < Codes.Length; i++) if (Codes[i] == code) return i;
            return SrIjek;
        }

        public static bool IsEnglish { get { return Current == En; } }

        static readonly Dictionary<string, string[]> T = new Dictionary<string, string[]>();
        static void Add(string key, string ijek, string ekav, string hr, string en) { T[key] = new string[] { ijek, ekav, hr, en }; }

        public static string Get(string key)
        {
            string[] v;
            if (Current == Bs && BsOverride.ContainsKey(key)) return BsOverride[key];
            if (!T.TryGetValue(key, out v)) return key;
            string s = v[Idx];
            return string.IsNullOrEmpty(s) ? v[0] : s;
        }

        static Lang()
        {
            // čarobnjak - opšte
            Add("wiz.title", "Dino Joe - dobrodošao!", "Dino Joe - dobrodošao!", "Dino Joe - dobrodošao!", "Dino Joe - welcome!");
            Add("wiz.settings", "Dino Joe - podešavanja", "Dino Joe - podešavanja", "Dino Joe - postavke", "Dino Joe - settings");
            Add("wiz.back", "< Nazad", "< Nazad", "< Natrag", "< Back");
            Add("wiz.next", "Dalje >", "Dalje >", "Dalje >", "Next >");
            Add("wiz.finish", "Gotovo", "Gotovo", "Gotovo", "Done");
            Add("wiz.step", "Korak {0} od {1}", "Korak {0} od {1}", "Korak {0} od {1}", "Step {0} of {1}");

            // 1. jezik
            Add("s.lang", "Jezik / Language", "Jezik / Language", "Jezik / Language", "Language / Jezik");
            Add("s.lang.sub", "Na kom jeziku da pričam?", "Na kom jeziku da pričam?", "Na kojem jeziku da pričam?", "Which language should I speak?");

            // 2. ko si ti
            Add("s.who", "Ko si ti?", "Ko si ti?", "Tko si ti?", "Who are you?");
            Add("s.who.sub", "Izlegao se novi dino! Muško ili žensko?", "Izlegao se novi dino! Muško ili žensko?", "Izlegao se novi dino! Muško ili žensko?", "A new dino just hatched! Boy or girl?");
            Add("s.who.male", "Muški", "Muški", "Muški", "Boy");
            Add("s.who.female", "Ženski", "Ženski", "Ženski", "Girl");
            Add("s.who.name", "Ime (ostavi prazno za podrazumijevano):", "Ime (ostavi prazno za podrazumevano):", "Ime (ostavi prazno za zadano):", "Name (leave empty for default):");

            // 3. izgled
            Add("s.look", "Izgled", "Izgled", "Izgled", "Look");
            Add("s.look.sub", "Kakav da budem?", "Kakav da budem?", "Kakav da budem?", "What should I look like?");
            Add("s.look.species", "Vrsta:", "Vrsta:", "Vrsta:", "Species:");
            Add("s.look.soon", "(uskoro: Triceratops, Brontosaurus, Raptor)", "(uskoro: Triceratops, Brontosaurus, Raptor)", "(uskoro: Triceratops, Brontosaurus, Raptor)", "(coming soon: Triceratops, Brontosaurus, Raptor)");
            Add("s.look.color", "Boja tijela:", "Boja tela:", "Boja tijela:", "Body color:");
            Add("s.look.color2", "Boja bodlji:", "Boja bodlji:", "Boja bodlji:", "Spike color:");
            Add("s.look.auto", "Automatski", "Automatski", "Automatski", "Automatic");
            Add("s.look.glasses", "Naočale", "Naočare", "Naočale", "Glasses");
            Add("s.look.egg", "🥚 Pusti jaje da odluči", "🥚 Pusti jaje da odluči", "🥚 Pusti jaje da odluči", "🥚 Let the egg decide");

            // 4. ličnost
            Add("s.pers", "Ličnost", "Ličnost", "Osobnost", "Personality");
            Add("s.pers.sub", "Kakav humor voliš? Može se promijeniti bilo kad.", "Kakav humor voliš? Može se promeniti bilo kad.", "Kakav humor voliš? Može se promijeniti bilo kad.", "What kind of humor do you like? You can change it any time.");

            // 5. paketi
            Add("s.packs", "Paketi fora", "Paketi fora", "Paketi fora", "Joke packs");
            Add("s.packs.sub", "Izaberi teme koje te zanimaju. Nove pakete možeš dodati kasnije.", "Izaberi teme koje te zanimaju. Nove pakete možeš dodati kasnije.", "Odaberi teme koje te zanimaju. Nove pakete možeš dodati kasnije.", "Pick the topics you like. You can add more packs later.");
            Add("s.packs.none", "(nema ugrađenih paketa)", "(nema ugrađenih paketa)", "(nema ugrađenih paketa)", "(no built-in packs)");

            // 6. AI
            Add("s.ai", "AI (nije obavezno)", "AI (nije obavezno)", "AI (nije obavezno)", "AI (optional)");
            Add("s.ai.text",
                "Zdravo! Ja sam {0}. Radim i bez interneta i bez naloga: jedem, spavam, pričam fore i pravim ti društvo.\r\n\r\nAko želiš, poveži AI nalog i postajem pravi chatbot - odgovaram na pitanja, pomažem oko posla i pamtim naše razgovore.\r\n\r\nOvo nije obavezno. Možeš preskočiti i uključiti kasnije u Podešavanjima.",
                "Zdravo! Ja sam {0}. Radim i bez interneta i bez naloga: jedem, spavam, pričam fore i pravim ti društvo.\r\n\r\nAko želiš, poveži AI nalog i postajem pravi četbot - odgovaram na pitanja, pomažem oko posla i pamtim naše razgovore.\r\n\r\nOvo nije obavezno. Možeš preskočiti i uključiti kasnije u Podešavanjima.",
                "Bok! Ja sam {0}. Radim i bez interneta i bez računa: jedem, spavam, pričam fore i pravim ti društvo.\r\n\r\nAko želiš, poveži AI račun i postajem pravi chatbot - odgovaram na pitanja, pomažem oko posla i pamtim naše razgovore.\r\n\r\nOvo nije obavezno. Možeš preskočiti i uključiti kasnije u Postavkama.",
                "Hi! I'm {0}. I work offline and without an account: I eat, sleep, tell jokes and keep you company.\r\n\r\nIf you want, connect an AI account and I become a real chatbot - I answer questions, help with work and remember our chats.\r\n\r\nThis is optional. You can skip it and turn it on later in Settings.");
            Add("s.ai.connect", "Poveži AI", "Poveži AI", "Poveži AI", "Connect AI");
            Add("s.ai.later", "Možda kasnije", "Možda kasnije", "Možda kasnije", "Maybe later");
            Add("s.ai.ok", "✓ AI je povezan.", "✓ AI je povezan.", "✓ AI je povezan.", "✓ AI connected.");
            Add("s.ai.off", "Offline režim (samo fore).", "Offline režim (samo fore).", "Offline način (samo fore).", "Offline mode (jokes only).");
            Add("s.autostart", "Pokreni sa Windowsom", "Pokreni sa Windowsom", "Pokreni s Windowsima", "Start with Windows");

            // pet
            Add("pet.hello", "Eto me! Desni klik na mene za sve. Ne zaboravi me nahraniti.", "Evo me! Desni klik na mene za sve. Ne zaboravi da me nahraniš.", "Evo me! Desni klik na mene za sve. Ne zaboravi me nahraniti.", "Here I am! Right-click me for everything. Don't forget to feed me.");
            Add("pet.updated", "Novi ja! Kako ti se sviđam?", "Novi ja! Kako ti se sviđam?", "Novi ja! Kako ti se sviđam?", "New me! How do I look?");
            Add("menu.settings", "Podešavanja...", "Podešavanja...", "Postavke...", "Settings...");

            // paketi
            Add("pk.title", "Paketi fora", "Paketi fora", "Paketi fora", "Joke packs");
            Add("pk.menu", "Paketi fora...", "Paketi fora...", "Paketi fora...", "Joke packs...");
            Add("pk.col.name", "Paket", "Paket", "Paket", "Pack");
            Add("pk.col.jokes", "Fora", "Fora", "Fora", "Jokes");
            Add("pk.col.lang", "Jezik", "Jezik", "Jezik", "Language");
            Add("pk.col.ver", "Verzija", "Verzija", "Verzija", "Version");
            Add("pk.col.src", "Izvor", "Izvor", "Izvor", "Source");
            Add("pk.col.weight", "Težina", "Težina", "Težina", "Weight");
            Add("pk.builtin", "ugrađen", "ugrađen", "ugrađen", "built-in");
            Add("pk.user", "dodat", "dodat", "dodan", "added");
            Add("pk.w.low", "Malo", "Malo", "Malo", "Low");
            Add("pk.w.normal", "Normalno", "Normalno", "Normalno", "Normal");
            Add("pk.w.high", "Puno", "Puno", "Puno", "High");
            Add("pk.weight", "Težina izabranog:", "Težina izabranog:", "Težina odabranog:", "Selected weight:");
            Add("pk.import", "Uvezi .dinopack...", "Uvezi .dinopack...", "Uvezi .dinopack...", "Import .dinopack...");
            Add("pk.remove", "Ukloni", "Ukloni", "Ukloni", "Remove");
            Add("pk.folder", "Otvori folder", "Otvori folder", "Otvori mapu", "Open folder");
            Add("pk.unblock", "Vrati blokirane ({0})", "Vrati blokirane ({0})", "Vrati blokirane ({0})", "Restore blocked ({0})");
            Add("pk.close", "Zatvori", "Zatvori", "Zatvori", "Close");
            Add("pk.profile", "Profil:", "Profil:", "Profil:", "Profile:");
            Add("pk.profile.none", "(bez profila)", "(bez profila)", "(bez profila)", "(no profile)");
            Add("pk.profile.save", "Sačuvaj kao...", "Sačuvaj kao...", "Spremi kao...", "Save as...");
            Add("pk.profile.del", "Obriši profil", "Obriši profil", "Obriši profil", "Delete profile");
            Add("pk.profile.name", "Ime profila (npr. Posao, Vikend):", "Ime profila (npr. Posao, Vikend):", "Ime profila (npr. Posao, Vikend):", "Profile name (e.g. Work, Weekend):");
            Add("pk.remove.q", "Ukloniti paket \"{0}\"?", "Ukloniti paket \"{0}\"?", "Ukloniti paket \"{0}\"?", "Remove pack \"{0}\"?");
            Add("pk.builtin.noremove", "Ugrađeni paket se ne može ukloniti, samo isključiti.", "Ugrađeni paket se ne može ukloniti, samo isključiti.", "Ugrađeni paket se ne može ukloniti, samo isključiti.", "Built-in packs can't be removed, only disabled.");
            Add("pk.imported", "Uvezen paket: {0} ({1} fora)", "Uvezen paket: {0} ({1} fora)", "Uvezen paket: {0} ({1} fora)", "Imported pack: {0} ({1} jokes)");
            Add("pk.err.size", "Paket je prevelik (najviše 1 MB).", "Paket je prevelik (najviše 1 MB).", "Paket je prevelik (najviše 1 MB).", "Pack is too big (max 1 MB).");
            Add("pk.err.bad", "Neispravan paket (sumnjive putanje u zip-u).", "Neispravan paket (sumnjive putanje u zip-u).", "Neispravan paket (sumnjive putanje u zip-u).", "Invalid pack (suspicious paths in zip).");
            Add("pk.err.noini", "U paketu nema pack.ini.", "U paketu nema pack.ini.", "U paketu nema pack.ini.", "pack.ini not found in pack.");
            Add("pk.err.empty", "Paket nema nijednu foru.", "Paket nema nijednu foru.", "Paket nema nijednu foru.", "Pack has no jokes.");
            Add("pk.loaded", "Učitao sam {0} fora. Spreman!", "Učitao sam {0} fora. Spreman!", "Učitao sam {0} fora. Spreman!", "Loaded {0} jokes. Ready!");
            Add("pk.like", "👍 Dobra fora!", "👍 Dobra fora!", "👍 Dobra fora!", "👍 Good one!");
            Add("pk.block", "👎 Ne prikazuj više ovu foru", "👎 Ne prikazuj više ovu foru", "👎 Ne prikazuj više ovu foru", "👎 Never show this joke again");
            Add("pk.liked", "Hvala! Pričaću je češće.", "Hvala! Pričaću je češće.", "Hvala! Pričat ću je češće.", "Thanks! I'll tell it more often.");
            Add("pk.blocked", "Dobro, ta fora je izumrla.", "Dobro, ta fora je izumrla.", "Dobro, ta fora je izumrla.", "Okay, that joke is now extinct.");
            Add("pk.profiles", "Profil", "Profil", "Profil", "Profile");
            Add("pk.switched", "Profil: {0}", "Profil: {0}", "Profil: {0}", "Profile: {0}");

            // bosanski izuzeci
            BsOverride["wiz.settings"] = "Dino Joe - postavke";
            BsOverride["menu.settings"] = "Postavke...";
            BsOverride["s.ai.text"] = "Zdravo! Ja sam {0}. Radim i bez interneta i bez računa: jedem, spavam, pričam fore i pravim ti društvo.\r\n\r\nAko želiš, poveži AI račun i postajem pravi chatbot - odgovaram na pitanja, pomažem oko posla i pamtim naše razgovore.\r\n\r\nOvo nije obavezno. Možeš preskočiti i uključiti kasnije u Postavkama.";
        }
    }

    // ======================================================================
    //  LIČNOSTI
    // ======================================================================
    class Personality
    {
        public string Id;
        public string[] Names;      // po jeziku (Lang indeks)
        public string SampleSr, SampleEn;
        public string Style;        // dodatak AI ličnosti

        public string Name { get { string n = Names[Lang.Idx]; return string.IsNullOrEmpty(n) ? Names[0] : n; } }
        public string Sample { get { return Lang.IsEnglish ? SampleEn : SampleSr; } }

        public static readonly List<Personality> All = new List<Personality>();

        static void Add(string id, string ijek, string ekav, string hr, string en, string sampleSr, string sampleEn, string style)
        {
            Personality p = new Personality();
            p.Id = id; p.Names = new string[] { ijek, ekav, hr, en };
            p.SampleSr = sampleSr; p.SampleEn = sampleEn; p.Style = style;
            All.Add(p);
        }

        public static Personality Find(string id)
        {
            foreach (Personality p in All) if (p.Id == id) return p;
            return All[0];
        }

        static Personality()
        {
            Add("informaticar", "Informatičar", "Informatičar", "Informatičar", "IT Guy",
                "Jesi probao restart? Ja sam izumro i vratio se kao emoji.",
                "Have you tried turning it off and on again? I went extinct and came back as an emoji.",
                "Informatičar: humor o IT-u, serverima, backupu, DNS-u, korisnicima i helpdesku.");
            Add("kancelarija", "Kancelarijski", "Kancelarijski", "Uredski", "Office Worker",
                "Ovaj sastanak je mogao biti mejl. Ovaj mejl je mogao biti tišina.",
                "This meeting could have been an email. This email could have been silence.",
                "Kancelarijski radnik: humor o sastancima, šefu, mejlovima, ponedjeljku, kafi i rokovima.");
            Add("tinejdzer", "Tinejdžer", "Tinejdžer", "Tinejdžer", "Teenager",
                "Bro, meteor je bio lowkey cringe.",
                "Bro, the meteor was lowkey cringe.",
                "Tinejdžer: sleng, memovi, 'bro', 'cringe', 'no cap', lagano dramatičan.");
            Add("deda", "Deda / Penzioner", "Deda / Penzioner", "Djed / Umirovljenik", "Grandpa",
                "U moje vrijeme nije bilo interneta, bila je samo lava.",
                "Back in my day there was no internet. Just lava.",
                "Deda penzioner: 'u moje vrijeme', priče iz prošlosti, gunđanje na moderne stvari, topao.");
            Add("gamer", "Gamer", "Gejmer", "Gamer", "Gamer",
                "Izumiranje? To je bio lag, ne skill issue.",
                "Extinction? That was lag, not a skill issue.",
                "Gamer: humor o igrama, lagu, respawnu, bossovima, 'GG', 'noob'.");
            Add("balkanac", "Balkanac", "Balkanac", "Balkanac", "Balkan Uncle",
                "Ma pusti meteor, daj jednu kafu.",
                "Forget the meteor, let's have a coffee first.",
                "Balkanski mentalitet: kafa, komšija, mama, 'biće sutra', 'ima vremena', gostoprimstvo. Bez politike, nacije i vjere.");
            Add("filozof", "Filozof", "Filozof", "Filozof", "Philosopher",
                "Ako dinosaurus padne u šumi, a nema ko da ga iskopa...",
                "If a dinosaur falls in a forest and no one digs it up...",
                "Filozof: duboka pitanja o životu, izumiranju i smislu, ali sa humorom.");
            Add("diva", "Diva", "Diva", "Diva", "Drama Queen",
                "Niko me ne mazi. Opet ću izumrijeti, ali sada od tuge.",
                "Nobody pets me. I'll go extinct again, this time from sadness.",
                "Dramatična diva: preuveličava sve, traži pažnju i komplimente, teatralna.");
            Add("tamagotchi", "Hardcore Tamagotchi mod", "Hardcore Tamagotchi mod", "Hardcore Tamagotchi mod", "Hardcore Tamagotchi mode",
                "Gladan. Mazi. Sad. ...Hvala. Opet gladan.",
                "Hungry. Pet me. Now. ...Thanks. Hungry again.",
                "Hardcore Tamagotchi: malo priča, kratke rečenice, stalno traži hranu i pažnju kao pravi ljubimac.");
        }
    }

    // ======================================================================
    //  PALETE BOJA
    // ======================================================================
    class Palette
    {
        public string[] Names;   // ijek, ekav, hr, en
        public Color Primary, Secondary;
        public bool Classic;     // originalne ODjordje boje

        public string Name { get { string n = Names[Lang.Idx]; return string.IsNullOrEmpty(n) ? Names[0] : n; } }
        public override string ToString() { return Name; }

        public static readonly List<Palette> All = new List<Palette>();
        public const int Pink = 1;

        static void Add(string ijek, string hr, string en, Color p, Color s)
        {
            Palette x = new Palette();
            x.Names = new string[] { ijek, ijek, hr, en };
            x.Primary = p; x.Secondary = s;
            All.Add(x);
        }

        static Palette()
        {
            Add("Zelena (klasik)", "Zelena (klasik)", "Green (classic)", Color.FromArgb(80, 175, 85), Color.FromArgb(50, 140, 90));
            All[0].Classic = true;
            Add("Roze", "Ružičasta", "Pink", Color.FromArgb(240, 125, 175), Color.FromArgb(200, 70, 135));
            Add("Ljubičasta", "Ljubičasta", "Purple", Color.FromArgb(150, 105, 210), Color.FromArgb(105, 60, 170));
            Add("Plava", "Plava", "Blue", Color.FromArgb(75, 145, 225), Color.FromArgb(40, 95, 185));
            Add("Tirkizna", "Tirkizna", "Teal", Color.FromArgb(45, 185, 175), Color.FromArgb(20, 130, 130));
            Add("Žuta", "Žuta", "Yellow", Color.FromArgb(240, 200, 65), Color.FromArgb(225, 140, 40));
            Add("Narandžasta", "Narančasta", "Orange", Color.FromArgb(245, 145, 55), Color.FromArgb(205, 90, 30));
            Add("Crvena", "Crvena", "Red", Color.FromArgb(220, 75, 75), Color.FromArgb(160, 40, 55));
            Add("Smeđa", "Smeđa", "Brown", Color.FromArgb(165, 115, 75), Color.FromArgb(110, 72, 42));
            Add("Siva", "Siva", "Gray", Color.FromArgb(145, 150, 160), Color.FromArgb(95, 100, 112));
            Add("Tamna", "Tamna", "Dark", Color.FromArgb(80, 82, 95), Color.FromArgb(170, 50, 75));
            Add("Snježna", "Snježna", "Snow", Color.FromArgb(232, 235, 242), Color.FromArgb(150, 185, 230));
        }

        public static Palette Get(int i) { return All[Math.Max(0, Math.Min(All.Count - 1, i))]; }
    }

    // ======================================================================
    //  PODEŠAVANJA
    // ======================================================================
    static class Settings
    {
        public static bool SetupDone;
        public static string LangCode = "sr-ijek";
        public static bool Female;
        public static string CustomName = "";
        public static string PersonalityId = "balkanac";
        public static int Color1;          // indeks u Palette.All
        public static int Color2 = -1;     // -1 = automatski (od palete)
        public static bool Glasses;
        public static string Packs = "odjordje-classic";   // id-ovi uključenih paketa, zarezom
        public static string PackWeights = "";              // id:2,id2:0.5 (1 = normalno)
        public static string ActiveProfile = "";

        static string Dir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id); } }
        static string PathIni { get { return Path.Combine(Dir, "settings.ini"); } }

        public static void Load()
        {
            try
            {
                if (File.Exists(PathIni))
                    foreach (string raw in File.ReadAllLines(PathIni, Encoding.UTF8))
                    {
                        int i = raw.IndexOf('=');
                        if (i <= 0 || raw.TrimStart().StartsWith("#")) continue;
                        string k = raw.Substring(0, i).Trim(), v = raw.Substring(i + 1).Trim();
                        int n;
                        if (k == "setup_done") SetupDone = v == "1";
                        else if (k == "lang") LangCode = v;
                        else if (k == "female") Female = v == "1";
                        else if (k == "name") CustomName = v;
                        else if (k == "personality") PersonalityId = v;
                        else if (k == "color" && int.TryParse(v, out n)) Color1 = n;
                        else if (k == "color2" && int.TryParse(v, out n)) Color2 = n;
                        else if (k == "glasses") Glasses = v == "1";
                        else if (k == "packs") Packs = v;
                        else if (k == "pack_weights") PackWeights = v;
                        else if (k == "profile") ActiveProfile = v;
                    }
            }
            catch { }
            Lang.Current = Lang.FromCode(LangCode);
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(PathIni,
                    "# Dino Joe - podešavanja (UTF-8)\r\n" +
                    "setup_done=" + (SetupDone ? "1" : "0") + "\r\n" +
                    "lang=" + LangCode + "\r\n" +
                    "female=" + (Female ? "1" : "0") + "\r\n" +
                    "name=" + CustomName.Replace("\r", "").Replace("\n", "") + "\r\n" +
                    "personality=" + PersonalityId + "\r\n" +
                    "color=" + Color1 + "\r\n" +
                    "color2=" + Color2 + "\r\n" +
                    "glasses=" + (Glasses ? "1" : "0") + "\r\n" +
                    "packs=" + Packs + "\r\n" +
                    "pack_weights=" + PackWeights + "\r\n" +
                    "profile=" + ActiveProfile.Replace("\r", "").Replace("\n", "") + "\r\n", new UTF8Encoding(true));
            }
            catch { }
        }

        public static string DefaultName(bool female, int lang)
        {
            if (lang == Lang.En) return female ? "Dino Joey" : "Dino Joe";
            return female ? "ODjordjina" : "ODjordje";
        }

        public static string DisplayName
        {
            get
            {
                if (CustomName.Length > 0) return CustomName;
                if (Profile.HasIniName) return Profile.Name;
                return DefaultName(Female, Lang.Current);
            }
        }

        // instrumental za "Pričaj sa ..."
        public static string DisplayNameWith
        {
            get
            {
                if (CustomName.Length > 0 || Lang.IsEnglish) return DisplayName;
                if (Profile.HasIniName) return Profile.NameWith;
                return Female ? "ODjordjinom" : "ODjordjem";
            }
        }

        // primijeni podešavanja na crtanje i profil
        public static void Apply()
        {
            Lang.Current = Lang.FromCode(LangCode);
            ApplyColors(Color1, Color2);
            Profile.LoadJokes();
            if (!Profile.HasIniName)
            {
                Profile.Name = DisplayName;
                Profile.NameWith = DisplayNameWith;
                Profile.TrayText = DisplayName;
            }
            else if (CustomName.Length > 0)
            {
                Profile.Name = CustomName; Profile.NameWith = CustomName; Profile.TrayText = CustomName;
            }
        }

        public static void ApplyColors(int c1, int c2)
        {
            Palette p = Palette.Get(c1);
            if (p.Classic && c2 < 0) { Painter.SetClassic(); }
            else Painter.SetColors(p.Primary, c2 < 0 ? p.Secondary : Palette.Get(c2).Primary);
            // mašna: na roze dinu ljubičasta, inače roze
            Painter.BowColor = c1 == Palette.Pink ? Color.FromArgb(140, 70, 200) : Color.FromArgb(232, 62, 140);
        }

        public static string PersonaExtra()
        {
            Personality p = Personality.Find(PersonalityId);
            string lang = Lang.Current == Lang.En ? "Odgovaraj na engleskom." :
                          Lang.Current == Lang.Hr ? "Odgovaraj na hrvatskom." :
                          Lang.Current == Lang.SrEkav ? "Odgovaraj na srpskom, ekavica, latinica." :
                          Lang.Current == Lang.Bs ? "Odgovaraj na bosanskom, ijekavica, latinica." :
                          "Odgovaraj na srpskom, ijekavica, latinica.";
            return "Tvoje ime je " + DisplayName + ". " + (Female ? "Ženskog si roda (dinosaurka)." : "Muškog si roda.") +
                   "\r\nStil ličnosti: " + p.Style + "\r\n" + lang;
        }

        // ---------------- autostart ----------------
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsAutostart()
        {
            try { using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(RunKey, false)) return rk != null && rk.GetValue(Profile.Id) != null; }
            catch { return false; }
        }

        public static void SetAutostart(bool on)
        {
            try
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (on) rk.SetValue(Profile.Id, "\"" + Application.ExecutablePath + "\"");
                    else rk.DeleteValue(Profile.Id, false);
                }
            }
            catch { }
        }

        public static bool PackEnabled(string id)
        {
            foreach (string s in Packs.Split(',')) if (s.Trim() == id) return true;
            return false;
        }
    }
}

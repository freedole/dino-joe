// Profil dinosaurusa: sve što razlikuje jednog dinu od drugog čita se iz foldera pored exe-a.
//   dino.ini        - ime, id, pozdravi, težine kategorija, ključne riječi za offline chat
//   fore\*.txt      - fore; ime fajla = kategorija. Specijalni: pile, hrana, bockanje, naocale (nisu u random izboru)
//   licnost.txt     - AI ličnost (ako ne postoji, koristi se %APPDATA%\<id>\licnost.txt)
// Bez profila radi kao ODjordje (ugrađene fore). Podaci: %APPDATA%\DinoJoe

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DinoJoe
{
    class JokeCat
    {
        public string Name;
        public List<string> Lines = new List<string>();
        public int Weight = 10;
        public string[] Keys = new string[0];
    }

    static class Profile
    {
        public static readonly string[] SpecialNames = { "pile", "hrana", "bockanje", "naocale" };

        public static string Dir;
        public static string Id = "DinoJoe";
        public static string Name = "ODjordje";
        public static string NameWith = "ODjordjem";   // instrumental: "Pričaj sa ODjordjem"
        public static string TrayText = "Dino Joe";
        public static bool HasIniName;   // dino.ini definiše ime (personalizovani dino)

        static readonly Dictionary<string, string> ini = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static readonly List<JokeCat> Cats = new List<JokeCat>();
        static readonly Dictionary<string, List<string>> special = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        static readonly Random rnd = new Random();

        public static string FunDir { get { return Path.Combine(Dir, "fore"); } }
        public static string IniPath { get { return Path.Combine(Dir, "dino.ini"); } }

        public static void Load()
        {
            Dir = Path.GetDirectoryName(Application.ExecutablePath);
            ini.Clear();
            try
            {
                if (File.Exists(IniPath))
                    foreach (string raw in File.ReadAllLines(IniPath, Encoding.UTF8))
                    {
                        string l = raw.Trim();
                        if (l.Length == 0 || l.StartsWith("#") || l.StartsWith(";") || l.StartsWith("[")) continue;
                        int i = l.IndexOf('=');
                        if (i <= 0) continue;
                        ini[l.Substring(0, i).Trim()] = l.Substring(i + 1).Trim();
                    }
            }
            catch { }

            Id = Safe(Get("id", "DinoJoe"));
            HasIniName = ini.ContainsKey("ime");
            Name = Get("ime", "ODjordje");
            NameWith = Get("ime_sa", Name);
            TrayText = Get("tray", Name + " AI");
            LoadJokes();
        }

        static string Safe(string s)
        {
            StringBuilder b = new StringBuilder();
            foreach (char c in s) if (char.IsLetterOrDigit(c) || c == '-' || c == '_') b.Append(c);
            return b.Length > 0 ? b.ToString() : "DinoPet";
        }

        public static string Get(string key, string def)
        {
            string v;
            return ini.TryGetValue(key, out v) && v.Length > 0 ? v : def;
        }

        /// Vrijednost može imati više varijanti odvojenih sa " | " - bira se nasumično.
        public static string GetPick(string key, string def)
        {
            string v = Get(key, def);
            string[] parts = v.Split('|');
            return parts[rnd.Next(parts.Length)].Trim();
        }

        public static void LoadJokes()
        {
            Cats.Clear();
            special.Clear();
            Dictionary<string, List<string>> files = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // 1) profil pored exe-a (personalizovani dino): fore\*.txt + fore*.txt
            try
            {
                if (Directory.Exists(FunDir))
                    foreach (string f in Directory.GetFiles(FunDir, "*.txt"))
                        AddFile(files, Path.GetFileNameWithoutExtension(f), f);
                foreach (string f in Directory.GetFiles(Dir, "fore*.txt"))
                {
                    string n = Path.GetFileNameWithoutExtension(f);
                    n = n.Equals("fore", StringComparison.OrdinalIgnoreCase) ? "moje" : n.Substring(4).TrimStart('-', '_', ' ');
                    if (n.Length == 0) n = "moje";
                    AddFile(files, n, f);
                }
            }
            catch { }
            foreach (KeyValuePair<string, List<string>> kv in files)
                AddCat(kv.Key, kv.Value, Get("tezina." + kv.Key, "10"), Get("kljucne." + kv.Key, ""), 1.0);

            // 2) uključeni paketi
            try
            {
                foreach (Pack p in Packs.All())
                {
                    if (!p.Enabled || !Directory.Exists(p.FunDir)) continue;
                    double mul = p.Weight;
                    foreach (string f in Directory.GetFiles(p.FunDir, "*.txt"))
                    {
                        string cat = Path.GetFileNameWithoutExtension(f);
                        Dictionary<string, List<string>> one = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                        AddFile(one, cat, f);
                        string w, k;
                        if (!p.Ini.TryGetValue("tezina." + cat, out w)) w = "10";
                        if (!p.Ini.TryGetValue("kljucne." + cat, out k)) k = "";
                        AddCat(p.Id + "/" + cat, one[cat], w, k, mul);
                    }
                }
            }
            catch { }

            // 3) ništa -> ugrađene ODjordje fore
            if (Cats.Count == 0)
            {
                AddCat("dino", new List<string>(Jokes.Dino), "22", "meteor,izumr,rex,dinosaur", 1.0);
                AddCat("it", new List<string>(Jokes.IT), "21", "server,backup,dns,mrež,mrez,windows,sql,vpn,hyper,docker,linux,lozink,admin", 1.0);
                AddCat("poljoprivreda", new List<string>(Jokes.Farm), "15", "pil,kok,jaj,pijet,farm,kukuruz,traktor,njiv", 1.0);
                AddCat("komasacija", new List<string>(Jokes.Koma), "8", "koma,parcel,međa,meda,geodet,katast", 1.0);
            }
        }

        /// Dodaje kategoriju (ili specijalnu). Blokirane fore se izbacuju, omiljene se dupliraju.
        static void AddCat(string name, List<string> raw, string weight, string keys, double mul)
        {
            List<string> lines = new List<string>();
            foreach (string l in raw)
            {
                if (Packs.IsBlocked(l)) continue;
                lines.Add(l);
                if (Packs.IsLiked(l)) { lines.Add(l); lines.Add(l); }
            }
            if (lines.Count == 0) return;

            string baseName = name.Contains("/") ? name.Substring(name.LastIndexOf('/') + 1) : name;
            if (Array.IndexOf(SpecialNames, baseName.ToLowerInvariant()) >= 0)
            {
                List<string> sp;
                if (!special.TryGetValue(baseName, out sp)) { sp = new List<string>(); special[baseName] = sp; }
                sp.AddRange(lines);
                return;
            }
            JokeCat c = new JokeCat();
            c.Name = name;
            c.Lines = lines;
            int w;
            c.Weight = int.TryParse(weight, out w) ? Math.Max(0, (int)Math.Round(w * mul)) : 10;
            List<string> kl = new List<string>();
            foreach (string k in keys.Split(',')) if (k.Trim().Length > 0) kl.Add(k.Trim().ToLowerInvariant());
            c.Keys = kl.ToArray();
            Cats.Add(c);
        }

        static void AddFile(Dictionary<string, List<string>> files, string cat, string path)
        {
            List<string> lines;
            if (!files.TryGetValue(cat, out lines)) { lines = new List<string>(); files[cat] = lines; }
            try
            {
                foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string l = raw.Trim().TrimStart('﻿');
                    if (l.Length > 0 && !l.StartsWith("#")) lines.Add(l);
                }
            }
            catch { }
        }

        public static int JokeCount
        {
            get { int n = 0; foreach (JokeCat c in Cats) n += c.Lines.Count; return n; }
        }

        /// Specijalna kategorija iz fajla, ili ugrađena rezerva.
        public static string Special(string name, string[] fallback)
        {
            List<string> l;
            if (special.TryGetValue(name, out l) && l.Count > 0) return l[rnd.Next(l.Count)];
            return fallback[rnd.Next(fallback.Length)];
        }

        public static bool HasSpecial(string name)
        {
            List<string> l;
            return special.TryGetValue(name, out l) && l.Count > 0;
        }

        /// Ponderisani nasumični izbor kategorije pa fore.
        public static string RandomJoke()
        {
            int total = 0;
            foreach (JokeCat c in Cats) if (c.Lines.Count > 0) total += c.Weight;
            if (total <= 0) return Jokes.Dino[rnd.Next(Jokes.Dino.Length)];
            int r = rnd.Next(total);
            foreach (JokeCat c in Cats)
            {
                if (c.Lines.Count == 0) continue;
                if (r < c.Weight) return c.Lines[rnd.Next(c.Lines.Count)];
                r -= c.Weight;
            }
            return Cats[0].Lines[0];
        }

        /// Offline odgovor: kategorija čija ključna riječ se pojavljuje u poruci.
        public static string JokeForText(string text)
        {
            string s = text.ToLowerInvariant();
            List<JokeCat> hits = new List<JokeCat>();
            foreach (JokeCat c in Cats)
                foreach (string k in c.Keys)
                    if (s.Contains(k)) { hits.Add(c); break; }
            if (hits.Count > 0)
            {
                JokeCat c = hits[rnd.Next(hits.Count)];
                return c.Lines[rnd.Next(c.Lines.Count)];
            }
            return null;
        }
    }
}

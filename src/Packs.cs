// Dino Joe - paketi fora: učitavanje, uključi/isključi, težina, uvoz .dinopack, uklanjanje,
// blokirane i omiljene fore, profili (sačuvane kombinacije paketa + ličnosti).
// Paket = folder sa pack.ini + fore\*.txt (+ opciono kljucne.ini). Samo tekst, nikad kod.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DinoJoe
{
    class Pack
    {
        public string Id, Name, Desc = "", Lang = "", Version = "", Author = "", Dir;
        public bool Builtin;
        public readonly Dictionary<string, string> Ini = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public int JokeCount;

        public string Personality = "";   // paket ličnosti: aktivan samo uz tu ličnost
        public bool Enabled { get { return Packs.IsEnabled(Id); } }
        public bool IsPersonality { get { return Personality.Length > 0; } }
        /// Da li se fore iz paketa trenutno koriste.
        public bool Active
        {
            get
            {
                if (!Packs.LangCompatible(Lang)) return false;
                if (IsPersonality) return Personality == Settings.PersonalityId && !Packs.IsDisabled(Id);
                return Enabled;
            }
        }
        public double Weight { get { return Packs.GetWeight(Id); } }
        // jokes\*.txt (stari format: fore\*.txt)
        public string FunDir
        {
            get
            {
                string j = Path.Combine(Dir, "jokes");
                if (Directory.Exists(j)) return j;
                string f = Path.Combine(Dir, "fore");
                return Directory.Exists(f) ? f : j;
            }
        }

        /// Vrijednost iz pack.ini po engleskom ključu, uz rezervni stari (srpski) ključ.
        public string Get(string key, string oldKey, string def)
        {
            string v;
            if (Ini.TryGetValue(key, out v) && v.Length > 0) return v;
            if (oldKey != null && Ini.TryGetValue(oldKey, out v) && v.Length > 0) return v;
            return def;
        }

        public string CatWeight(string cat) { return Get("weight." + cat, "tezina." + cat, "10"); }
        public string CatKeywords(string cat) { return Get("keywords." + cat, "kljucne." + cat, ""); }
    }

    static class Packs
    {
        public const long MaxPackBytes = 1024 * 1024;   // 1 MB po paketu
        const int MaxEntries = 300;

        public static string UserDir { get { return Path.Combine(DataDir, "packs"); } }
        public static string BuiltinDir { get { return Path.Combine(Profile.Dir ?? "", "packs-builtin"); } }
        static string DataDir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Profile.Id); } }
        static string BlockedPath { get { return Path.Combine(DataDir, "blocked.txt"); } }
        static string LikedPath { get { return Path.Combine(DataDir, "liked.txt"); } }
        static string ProfilesPath { get { return Path.Combine(DataDir, "profiles.txt"); } }

        static readonly HashSet<string> blocked = new HashSet<string>();
        static readonly Dictionary<string, string> blockedText = new Dictionary<string, string>();   // hash -> tekst fore
        static readonly HashSet<string> liked = new HashSet<string>();
        static bool hashesLoaded;

        // ---------------- otkrivanje paketa ----------------
        public static List<Pack> All()
        {
            List<Pack> list = new List<Pack>();
            Dictionary<string, bool> seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            // korisnički paketi imaju prednost (novija verzija ugrađenog)
            Scan(UserDir, false, list, seen);
            Scan(BuiltinDir, true, list, seen);
            return list;
        }

        static void Scan(string root, bool builtin, List<Pack> list, Dictionary<string, bool> seen)
        {
            try
            {
                if (!Directory.Exists(root)) return;
                foreach (string d in Directory.GetDirectories(root))
                {
                    Pack p = Read(d, builtin);
                    if (p == null || seen.ContainsKey(p.Id)) continue;
                    seen[p.Id] = true;
                    list.Add(p);
                }
            }
            catch { }
        }

        public static Pack Read(string dir, bool builtin)
        {
            Pack p = new Pack();
            p.Dir = dir; p.Builtin = builtin;
            p.Id = Path.GetFileName(dir);
            string ini = Path.Combine(dir, "pack.ini");
            ReadIni(ini, p.Ini);
            // keywords.ini (stari naziv: kljucne.ini): kategorija=rijec1,rijec2
            foreach (string kname in new string[] { "keywords.ini", "kljucne.ini" })
            {
                string kl = Path.Combine(dir, kname);
                if (!File.Exists(kl)) continue;
                Dictionary<string, string> k = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ReadIni(kl, k);
                foreach (KeyValuePair<string, string> kv in k)
                {
                    string key = kv.Key;
                    if (key.StartsWith("kljucne.", StringComparison.OrdinalIgnoreCase)) key = key.Substring(8);
                    if (key.StartsWith("keywords.", StringComparison.OrdinalIgnoreCase)) key = key.Substring(9);
                    p.Ini["keywords." + key] = kv.Value;
                }
            }
            string v = p.Get("id", null, "");
            if (v.Length > 0) p.Id = SafeId(v);
            p.Name = p.Get("name", "naziv", p.Id);
            p.Desc = p.Get("description", "opis", "");
            p.Lang = p.Get("lang", "jezik", "");
            p.Version = p.Get("version", "verzija", "");
            p.Author = p.Get("author", "autor", "");
            p.Personality = p.Get("personality", null, "");
            try
            {
                if (Directory.Exists(p.FunDir))
                    foreach (string f in Directory.GetFiles(p.FunDir, "*.txt"))
                        foreach (string raw in File.ReadAllLines(f, Encoding.UTF8))
                        {
                            string l = raw.Trim().TrimStart('﻿');
                            if (l.Length > 0 && !l.StartsWith("#")) p.JokeCount++;
                        }
            }
            catch { }
            if (p.JokeCount == 0 && !File.Exists(ini)) return null;
            return p;
        }

        static void ReadIni(string path, Dictionary<string, string> into)
        {
            try
            {
                if (!File.Exists(path)) return;
                foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string l = raw.Trim().TrimStart('﻿');
                    if (l.Length == 0 || l.StartsWith("#") || l.StartsWith(";") || l.StartsWith("[")) continue;
                    int i = l.IndexOf('=');
                    if (i <= 0) continue;
                    into[l.Substring(0, i).Trim()] = l.Substring(i + 1).Trim();
                }
            }
            catch { }
        }

        public static string SafeId(string s)
        {
            StringBuilder b = new StringBuilder();
            foreach (char c in s.ToLowerInvariant())
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_') b.Append(c);
            string r = b.ToString().Trim('-', '_');
            if (r.Length > 60) r = r.Substring(0, 60);
            return r.Length > 0 ? r : "paket";
        }

        /// Vrijednost iz pack.ini prvog aktivnog paketa koji je ima (dodati paketi imaju prednost).
        /// Koristi se za personalizovane dinose: pet_name, pet_name_with, greeting.*.
        public static string ActiveValue(string key)
        {
            foreach (Pack p in All())
            {
                if (!p.Active) continue;
                string v = p.Get(key, null, "");
                if (v.Length > 0) return v;
            }
            return null;
        }

        /// persona.txt iz aktivnih paketa (dodatak AI ličnosti).
        public static string ActivePersona()
        {
            StringBuilder b = new StringBuilder();
            foreach (Pack p in All())
            {
                if (!p.Active) continue;
                string f = Path.Combine(p.Dir, "persona.txt");
                try { if (File.Exists(f)) b.Append("\r\n\r\n").Append(File.ReadAllText(f, Encoding.UTF8).Trim()); } catch { }
            }
            return b.ToString();
        }

        /// en paketi idu uz engleski, ostali (sr/bs/hr) uz naše jezike; prazno = svi
        public static bool LangCompatible(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return true;
            bool en = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase);
            return en == DinoJoe.Lang.IsEnglish;
        }

        /// Podrazumijevani paketi za trenutni jezik (svi ugrađeni, osim paketa ličnosti).
        public static string DefaultPacks()
        {
            List<string> ids = new List<string>();
            foreach (Pack p in All()) if (p.Builtin && !p.IsPersonality && LangCompatible(p.Lang)) ids.Add(p.Id);
            return string.Join(",", ids.ToArray());
        }

        // ---------------- uključeni paketi i težine ----------------
        public static bool IsEnabled(string id)
        {
            foreach (string s in Settings.Packs.Split(',')) if (s.Trim().Equals(id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// Isključeni paketi ličnosti (podrazumijevano su uključeni uz svoju ličnost).
        public static bool IsDisabled(string id)
        {
            foreach (string s in Settings.DisabledPacks.Split(',')) if (s.Trim().Equals(id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static void SetDisabled(string id, bool off)
        {
            List<string> ids = new List<string>();
            foreach (string s in Settings.DisabledPacks.Split(','))
            {
                string t = s.Trim();
                if (t.Length > 0 && !t.Equals(id, StringComparison.OrdinalIgnoreCase)) ids.Add(t);
            }
            if (off) ids.Add(id);
            Settings.DisabledPacks = string.Join(",", ids.ToArray());
        }

        public static void SetEnabled(string id, bool on)
        {
            List<string> ids = new List<string>();
            foreach (string s in Settings.Packs.Split(','))
            {
                string t = s.Trim();
                if (t.Length > 0 && !t.Equals(id, StringComparison.OrdinalIgnoreCase)) ids.Add(t);
            }
            if (on) ids.Add(id);
            Settings.Packs = string.Join(",", ids.ToArray());
        }

        // težina: 0.5 malo, 1 normalno, 2 puno. Format: id:2,id2:0.5
        public static double GetWeight(string id)
        {
            foreach (string s in Settings.PackWeights.Split(','))
            {
                int i = s.IndexOf(':');
                if (i <= 0) continue;
                if (!s.Substring(0, i).Trim().Equals(id, StringComparison.OrdinalIgnoreCase)) continue;
                double w;
                if (double.TryParse(s.Substring(i + 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out w)) return w;
            }
            return 1.0;
        }

        public static void SetWeight(string id, double w)
        {
            List<string> parts = new List<string>();
            foreach (string s in Settings.PackWeights.Split(','))
            {
                int i = s.IndexOf(':');
                if (i <= 0 || s.Substring(0, i).Trim().Equals(id, StringComparison.OrdinalIgnoreCase)) continue;
                parts.Add(s.Trim());
            }
            if (Math.Abs(w - 1.0) > 0.01) parts.Add(id + ":" + w.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Settings.PackWeights = string.Join(",", parts.ToArray());
        }

        // ---------------- blokirane / omiljene fore ----------------
        public static string Hash(string line)
        {
            using (SHA1 sha = SHA1.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(line.Trim()));
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < 8; i++) b.Append(h[i].ToString("x2"));
                return b.ToString();
            }
        }

        static void LoadHashes()
        {
            if (hashesLoaded) return;
            hashesLoaded = true;
            LoadSet(BlockedPath, blocked);
            LoadSet(LikedPath, liked);
        }

        // format linije: hash<TAB>tekst fore (tekst je opcion, služi za listu "Vrati blokirane")
        static void LoadSet(string path, HashSet<string> set)
        {
            set.Clear();
            bool isBlocked = path == BlockedPath;
            if (isBlocked) blockedText.Clear();
            try
            {
                if (File.Exists(path))
                    foreach (string l in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        string t = l.Trim().TrimStart('\uFEFF');
                        if (t.Length == 0 || t.StartsWith("#")) continue;
                        int tab = t.IndexOf('\t');
                        string h = tab > 0 ? t.Substring(0, tab) : t;
                        set.Add(h);
                        if (isBlocked && tab > 0) blockedText[h] = t.Substring(tab + 1);
                    }
            }
            catch { }
        }

        static void SaveSet(string path, HashSet<string> set)
        {
            try
            {
                Directory.CreateDirectory(DataDir);
                List<string> l = new List<string>();
                l.Add("# Dino Joe - hash fora (SHA1, prvih 8 bajtova)<TAB>tekst");
                bool isBlocked = path == BlockedPath;
                foreach (string h in set)
                {
                    string txt;
                    l.Add(isBlocked && blockedText.TryGetValue(h, out txt) ? h + "\t" + txt : h);
                }
                File.WriteAllLines(path, l.ToArray(), new UTF8Encoding(true));
            }
            catch { }
        }

        /// Lista blokiranih: hash -> tekst (ili null ako tekst nije sačuvan).
        public static List<KeyValuePair<string, string>> BlockedList()
        {
            LoadHashes();
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach (string h in blocked)
            {
                string t;
                list.Add(new KeyValuePair<string, string>(h, blockedText.TryGetValue(h, out t) ? t : null));
            }
            return list;
        }

        public static void Unblock(IEnumerable<string> hashes)
        {
            LoadHashes();
            foreach (string h in hashes) { blocked.Remove(h); blockedText.Remove(h); }
            SaveSet(BlockedPath, blocked);
        }

        public static bool IsBlocked(string line) { LoadHashes(); return blocked.Contains(Hash(line)); }
        public static bool IsLiked(string line) { LoadHashes(); return liked.Contains(Hash(line)); }
        public static int BlockedCount { get { LoadHashes(); return blocked.Count; } }

        public static void Block(string line) { LoadHashes(); string h = Hash(line); blocked.Add(h); blockedText[h] = line.Trim(); liked.Remove(h); SaveSet(BlockedPath, blocked); SaveSet(LikedPath, liked); }
        public static void Like(string line) { LoadHashes(); string h = Hash(line); liked.Add(h); blocked.Remove(h); SaveSet(LikedPath, liked); SaveSet(BlockedPath, blocked); }
        public static void UnblockAll() { LoadHashes(); blocked.Clear(); blockedText.Clear(); SaveSet(BlockedPath, blocked); }

        // ---------------- uvoz .dinopack (zip) ----------------
        /// Vraća instalirani paket ili baca izuzetak sa porukom za korisnika.
        public static Pack Import(string zipPath)
        {
            FileInfo fi = new FileInfo(zipPath);
            if (fi.Length > MaxPackBytes * 2) throw new Exception(Lang.Get("pk.err.size"));

            string tmp = Path.Combine(Path.GetTempPath(), "dinojoe-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            try
            {
                long total = 0;
                using (ZipArchive za = ZipFile.OpenRead(zipPath))
                {
                    if (za.Entries.Count > MaxEntries) throw new Exception(Lang.Get("pk.err.size"));
                    // ako je sve u jednom root folderu, skini ga
                    string prefix = CommonRoot(za);
                    string full = Path.GetFullPath(tmp) + Path.DirectorySeparatorChar;
                    foreach (ZipArchiveEntry e in za.Entries)
                    {
                        string name = e.FullName.Replace('\\', '/');
                        if (name.EndsWith("/")) continue;
                        if (prefix.Length > 0) name = name.Substring(prefix.Length);
                        string ext = Path.GetExtension(name).ToLowerInvariant();
                        if (ext != ".txt" && ext != ".ini") continue;          // samo tekst
                        total += e.Length;
                        if (total > MaxPackBytes) throw new Exception(Lang.Get("pk.err.size"));
                        string dest = Path.GetFullPath(Path.Combine(tmp, name.Replace('/', Path.DirectorySeparatorChar)));
                        if (!dest.StartsWith(full, StringComparison.OrdinalIgnoreCase)) throw new Exception(Lang.Get("pk.err.bad"));   // zip-slip
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        e.ExtractToFile(dest, true);
                    }
                }
                if (!File.Exists(Path.Combine(tmp, "pack.ini"))) throw new Exception(Lang.Get("pk.err.noini"));
                Pack p = Read(tmp, false);
                if (p == null || p.JokeCount == 0) throw new Exception(Lang.Get("pk.err.empty"));

                string target = Path.Combine(UserDir, p.Id);
                Directory.CreateDirectory(UserDir);
                if (Directory.Exists(target)) Directory.Delete(target, true);   // ažuriranje = zamjena
                Directory.Move(tmp, target);
                tmp = null;
                SetEnabled(p.Id, true);
                Settings.Save();
                return Read(target, false);
            }
            finally
            {
                try { if (tmp != null && Directory.Exists(tmp)) Directory.Delete(tmp, true); } catch { }
            }
        }

        static string CommonRoot(ZipArchive za)
        {
            string root = null;
            foreach (ZipArchiveEntry e in za.Entries)
            {
                string n = e.FullName.Replace('\\', '/');
                int i = n.IndexOf('/');
                if (i <= 0) return "";            // fajl u korijenu
                string r = n.Substring(0, i + 1);
                if (root == null) root = r; else if (root != r) return "";
            }
            if (root == null) return "";
            // root folder ima smisla skinuti samo ako pack.ini nije u korijenu
            foreach (ZipArchiveEntry e in za.Entries) if (e.FullName.Replace('\\', '/') == root + "pack.ini") return root;
            return "";
        }

        public static void Remove(Pack p)
        {
            if (p.Builtin) return;
            string full = Path.GetFullPath(p.Dir);
            if (!full.StartsWith(Path.GetFullPath(UserDir), StringComparison.OrdinalIgnoreCase)) return;
            Directory.Delete(full, true);
            SetEnabled(p.Id, false);
            Settings.Save();
        }

        // ---------------- profili ----------------
        public class SavedProfile { public string Name, Packs, Personality, Weights; }

        public static List<SavedProfile> LoadProfiles()
        {
            List<SavedProfile> list = new List<SavedProfile>();
            try
            {
                if (!File.Exists(ProfilesPath)) return list;
                foreach (string raw in File.ReadAllLines(ProfilesPath, Encoding.UTF8))
                {
                    string l = raw.Trim().TrimStart('﻿');
                    if (l.Length == 0 || l.StartsWith("#")) continue;
                    string[] f = l.Split('|');
                    if (f.Length < 3) continue;
                    SavedProfile sp = new SavedProfile();
                    sp.Name = f[0]; sp.Packs = f[1]; sp.Personality = f[2]; sp.Weights = f.Length > 3 ? f[3] : "";
                    list.Add(sp);
                }
            }
            catch { }
            return list;
        }

        public static void SaveProfiles(List<SavedProfile> list)
        {
            try
            {
                Directory.CreateDirectory(DataDir);
                List<string> lines = new List<string>();
                lines.Add("# Dino Joe profili: ime|paketi|licnost|tezine");
                foreach (SavedProfile p in list)
                    lines.Add(p.Name.Replace("|", "/") + "|" + p.Packs + "|" + p.Personality + "|" + p.Weights);
                File.WriteAllLines(ProfilesPath, lines.ToArray(), new UTF8Encoding(true));
            }
            catch { }
        }

        public static void SaveCurrentAs(string name)
        {
            List<SavedProfile> list = LoadProfiles();
            list.RemoveAll(delegate(SavedProfile p) { return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
            SavedProfile sp = new SavedProfile();
            sp.Name = name; sp.Packs = Settings.Packs; sp.Personality = Settings.PersonalityId; sp.Weights = Settings.PackWeights;
            list.Add(sp);
            SaveProfiles(list);
            Settings.ActiveProfile = name;
            Settings.Save();
        }

        public static void Activate(SavedProfile sp)
        {
            Settings.Packs = sp.Packs;
            Settings.PersonalityId = sp.Personality;
            Settings.PackWeights = sp.Weights ?? "";
            Settings.ActiveProfile = sp.Name;
            Settings.Save();
        }

        public static void DeleteProfile(string name)
        {
            List<SavedProfile> list = LoadProfiles();
            list.RemoveAll(delegate(SavedProfile p) { return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
            SaveProfiles(list);
            if (Settings.ActiveProfile.Equals(name, StringComparison.OrdinalIgnoreCase)) { Settings.ActiveProfile = ""; Settings.Save(); }
        }
    }
}

// Dino Joe - katalog paketa sa GitHub-a (freedole/dino-joe-packs/index.json).
// Preuzimanje: svaki fajl se provjerava (SHA256, veličina, samo .txt/.ini, bez ../), pa se paket
// atomski premjesti u %APPDATA%\DinoJoe\packs\<id>. Novija verzija ugrađenog paketa ga zamjenjuje.
// C# 5 kompatibilno.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace DinoJoe
{
    class CatalogPack
    {
        public string Id, Name, Lang, Version, Author, Description, Personality, Dir;
        public int Jokes;
        public readonly List<KeyValuePair<string, string>> Files = new List<KeyValuePair<string, string>>();   // path -> sha256
        public readonly List<long> Sizes = new List<long>();
    }

    static class Catalog
    {
        public const string IndexUrl = "https://raw.githubusercontent.com/freedole/dino-joe-packs/main/index.json";
        public static string Base = "https://raw.githubusercontent.com/freedole/dino-joe-packs/main/";

        static Catalog()
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { }   // TLS 1.2
        }

        static WebClient Client()
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers[HttpRequestHeader.UserAgent] = "DinoJoe";
            IWebProxy px = WebRequest.DefaultWebProxy;
            if (px != null) { px.Credentials = CredentialCache.DefaultCredentials; wc.Proxy = px; }
            return wc;
        }

        public static List<CatalogPack> Fetch()
        {
            string json;
            using (WebClient wc = Client()) json = wc.DownloadString(IndexUrl + "?t=" + DateTime.UtcNow.Ticks);
            Dictionary<string, object> root = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
            List<CatalogPack> list = new List<CatalogPack>();
            if (root == null) return list;
            if (root.ContainsKey("base") && root["base"] is string && ((string)root["base"]).StartsWith("https://raw.githubusercontent.com/")) Base = (string)root["base"];
            IList packs = root.ContainsKey("packs") ? root["packs"] as IList : null;
            if (packs == null) return list;
            foreach (object o in packs)
            {
                Dictionary<string, object> d = o as Dictionary<string, object>;
                if (d == null) continue;
                CatalogPack p = new CatalogPack();
                p.Id = Packs.SafeId(S(d, "id"));
                p.Name = S(d, "name"); p.Lang = S(d, "lang"); p.Version = S(d, "version"); p.Author = S(d, "author");
                p.Description = S(d, "description"); p.Personality = S(d, "personality"); p.Dir = S(d, "dir");
                int j; int.TryParse(S(d, "jokes"), out j); p.Jokes = j;
                IList files = d.ContainsKey("files") ? d["files"] as IList : null;
                if (files != null)
                    foreach (object fo in files)
                    {
                        Dictionary<string, object> f = fo as Dictionary<string, object>;
                        if (f == null) continue;
                        p.Files.Add(new KeyValuePair<string, string>(S(f, "path"), S(f, "sha256").ToLowerInvariant()));
                        long sz; long.TryParse(S(f, "size"), out sz); p.Sizes.Add(sz);
                    }
                if (p.Id.Length > 0 && p.Dir.Length > 0) list.Add(p);
            }
            return list;
        }

        static string S(Dictionary<string, object> d, string k) { return d.ContainsKey(k) && d[k] != null ? d[k].ToString() : ""; }

        public static int CompareVersions(string a, string b)
        {
            Version va, vb;
            if (Version.TryParse(Pad(a), out va) && Version.TryParse(Pad(b), out vb)) return va.CompareTo(vb);
            return string.Compare(a, b, StringComparison.Ordinal);
        }

        static string Pad(string v) { v = (v ?? "0").Trim(); return v.Contains(".") ? v : v + ".0"; }

        /// Preuzmi i instaliraj paket. Baca izuzetak sa porukom za korisnika.
        public static void Install(CatalogPack p)
        {
            if (p.Dir.Contains("..")) throw new Exception(Lang.Get("pk.err.bad"));
            long total = 0;
            foreach (long s in p.Sizes) total += s;
            if (total > Packs.MaxPackBytes || p.Files.Count > 300) throw new Exception(Lang.Get("pk.err.size"));

            string tmp = Path.Combine(Path.GetTempPath(), "dinojoe-cat-" + Guid.NewGuid().ToString("N"));
            string full = Path.GetFullPath(tmp) + Path.DirectorySeparatorChar;
            try
            {
                using (WebClient wc = Client())
                using (SHA256 sha = SHA256.Create())
                {
                    foreach (KeyValuePair<string, string> f in p.Files)
                    {
                        string rel = f.Key.Replace('\\', '/');
                        string ext = Path.GetExtension(rel).ToLowerInvariant();
                        if (ext != ".txt" && ext != ".ini") continue;                  // samo tekst
                        if (rel.Contains("..") || rel.StartsWith("/")) throw new Exception(Lang.Get("pk.err.bad"));
                        byte[] data = wc.DownloadData(Base + p.Dir + "/" + Uri.EscapeUriString(rel));
                        if (data.LongLength > Packs.MaxPackBytes) throw new Exception(Lang.Get("pk.err.size"));
                        string h = BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
                        if (h != f.Value) throw new Exception(Lang.Get("cat.err.hash") + " " + rel);
                        string dest = Path.GetFullPath(Path.Combine(tmp, rel.Replace('/', Path.DirectorySeparatorChar)));
                        if (!dest.StartsWith(full, StringComparison.OrdinalIgnoreCase)) throw new Exception(Lang.Get("pk.err.bad"));
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.WriteAllBytes(dest, data);
                    }
                }
                if (!File.Exists(Path.Combine(tmp, "pack.ini"))) throw new Exception(Lang.Get("pk.err.noini"));
                string target = Path.Combine(Packs.UserDir, p.Id);
                Directory.CreateDirectory(Packs.UserDir);
                if (Directory.Exists(target)) Directory.Delete(target, true);
                Directory.Move(tmp, target);
                tmp = null;
                if (p.Personality.Length == 0) Packs.SetEnabled(p.Id, true);
                Settings.Save();
            }
            finally
            {
                try { if (tmp != null && Directory.Exists(tmp)) Directory.Delete(tmp, true); } catch { }
            }
        }
    }

    // ======================================================================
    //  PROZOR KATALOGA
    // ======================================================================
    class CatalogForm : ThemedForm
    {
        readonly ListView lv = new ListView();
        readonly Label status = new Label();
        readonly ModernButton btInstall = new ModernButton();
        readonly CheckBox chAllLangs = new CheckBox();
        List<CatalogPack> all = new List<CatalogPack>();
        public bool Changed;

        public CatalogForm()
        {
            Text = Lang.Get("cat.title");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 470);
            TopMost = true;
            try { Icon = Painter.MakeIcon(); } catch { }

            Label sub = new Label();
            sub.Text = Lang.Get("cat.sub");
            sub.ForeColor = Theme.TextMuted;
            sub.SetBounds(12, 12, 736, 22);
            Controls.Add(sub);

            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.HideSelection = false;
            lv.MultiSelect = false;
            lv.ShowItemToolTips = true;
            lv.SetBounds(12, 40, 736, 340);
            lv.Columns.Add(Lang.Get("pk.col.name"), 230);
            lv.Columns.Add(Lang.Get("pk.col.jokes"), 60, HorizontalAlignment.Right);
            lv.Columns.Add(Lang.Get("pk.col.lang"), 70);
            lv.Columns.Add(Lang.Get("pk.col.ver"), 70);
            lv.Columns.Add(Lang.Get("cat.col.state"), 130);
            lv.Columns.Add(Lang.Get("pk.col.src"), 140);
            lv.SelectedIndexChanged += delegate { SyncButton(); };
            lv.DoubleClick += delegate { InstallSelected(); };
            Controls.Add(lv);

            chAllLangs.Text = Lang.Get("cat.alllangs");
            chAllLangs.SetBounds(12, 388, 300, 24);
            chAllLangs.CheckedChanged += delegate { Fill(); };
            Controls.Add(chAllLangs);

            status.SetBounds(12, 432, 400, 26);
            status.ForeColor = Theme.TextMuted;
            Controls.Add(status);

            btInstall.Text = Lang.Get("cat.install");
            btInstall.Primary = true;
            btInstall.Enabled = false;
            btInstall.SetBounds(470, 428, 160, 32);
            btInstall.Click += delegate { InstallSelected(); };
            Controls.Add(btInstall);

            ModernButton close = new ModernButton();
            close.Text = Lang.Get("pk.close");
            close.SetBounds(638, 428, 110, 32);
            close.Click += delegate { Close(); };
            Controls.Add(close);
            CancelButton = close;

            Shown += delegate { LoadCatalog(); };
        }

        void LoadCatalog()
        {
            status.Text = Lang.Get("cat.loading");
            ThreadPool.QueueUserWorkItem(delegate
            {
                List<CatalogPack> res = null; string err = null;
                try { res = Catalog.Fetch(); } catch (Exception ex) { err = ex.Message; }
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (err != null) { status.ForeColor = Theme.Danger; status.Text = Lang.Get("cat.err.net") + " " + err; return; }
                        all = res;
                        Fill();
                    });
                }
                catch { }
            });
        }

        Pack Installed(string id)
        {
            foreach (Pack p in Packs.All()) if (p.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) return p;
            return null;
        }

        string State(CatalogPack c, out bool canInstall)
        {
            Pack p = Installed(c.Id);
            canInstall = true;
            if (p == null) return Lang.Get("cat.state.new");
            int cmp = Catalog.CompareVersions(c.Version, p.Version);
            if (cmp > 0) return string.Format(Lang.Get("cat.state.update"), p.Version);
            canInstall = false;
            return Lang.Get("cat.state.installed");
        }

        void Fill()
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            int shown = 0;
            foreach (CatalogPack c in all)
            {
                if (!chAllLangs.Checked && !Packs.LangCompatible(c.Lang)) continue;
                bool can;
                ListViewItem it = new ListViewItem(c.Name);
                it.SubItems.Add(c.Jokes.ToString());
                it.SubItems.Add(c.Lang);
                it.SubItems.Add(c.Version);
                it.SubItems.Add(State(c, out can));
                it.SubItems.Add(c.Personality.Length > 0 ? Lang.Get("pk.personality") + ": " + Personality.Find(c.Personality).Name : c.Author);
                it.ToolTipText = c.Description;
                it.Tag = c;
                if (!can) it.ForeColor = Theme.TextMuted;
                lv.Items.Add(it);
                shown++;
            }
            lv.EndUpdate();
            status.ForeColor = Theme.TextMuted;
            status.Text = string.Format(Lang.Get("cat.count"), shown);
            SyncButton();
        }

        void SyncButton()
        {
            bool can = false;
            if (lv.SelectedItems.Count > 0) State((CatalogPack)lv.SelectedItems[0].Tag, out can);
            btInstall.Enabled = can;
        }

        void InstallSelected()
        {
            if (lv.SelectedItems.Count == 0) return;
            CatalogPack c = (CatalogPack)lv.SelectedItems[0].Tag;
            bool can; State(c, out can);
            if (!can) return;
            btInstall.Enabled = false;
            status.ForeColor = Theme.TextMuted;
            status.Text = string.Format(Lang.Get("cat.installing"), c.Name);
            ThreadPool.QueueUserWorkItem(delegate
            {
                string err = null;
                try { Catalog.Install(c); } catch (Exception ex) { err = ex.Message; }
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (err != null) { status.ForeColor = Theme.Danger; status.Text = err; SyncButton(); return; }
                        Changed = true;
                        Fill();
                        status.ForeColor = Theme.Accent;
                        status.Text = string.Format(Lang.Get("cat.installed"), c.Name);
                    });
                }
                catch { }
            });
        }
    }
}

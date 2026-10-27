// Dino Joe AI - chat preko OpenAI-kompatibilnog API-ja (Groq podrazumijevano; radi i Cerebras, OpenRouter, Ollama /v1...)
// C# 5 kompatibilno. Reference: System.Security.dll (DPAPI), System.Web.Extensions.dll (JSON).

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
    // ======================================================================
    //  KONFIGURACIJA (ključ šifrovan DPAPI-jem, vezan za Windows korisnika)
    // ======================================================================
    class AiProvider
    {
        public string Id, NameKey, Url, Model, KeysPage, InfoKey;
        public bool NeedsKey = true, Anthropic;

        public string Name { get { return Lang.Get(NameKey); } }
        public override string ToString() { return Name; }

        public static readonly List<AiProvider> All = new List<AiProvider>();

        static AiProvider Add(string id, string url, string model, string keys)
        {
            AiProvider p = new AiProvider();
            p.Id = id; p.NameKey = "aip." + id; p.InfoKey = "aip." + id + ".info";
            p.Url = url; p.Model = model; p.KeysPage = keys;
            All.Add(p);
            return p;
        }

        static AiProvider()
        {
            Add("groq", "https://api.groq.com/openai/v1/chat/completions", "openai/gpt-oss-120b", "https://console.groq.com/keys");
            Add("gemini", "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions", "gemini-2.5-flash", "https://aistudio.google.com/apikey");
            Add("openai", "https://api.openai.com/v1/chat/completions", "gpt-5-mini", "https://platform.openai.com/api-keys");
            Add("claude", "https://api.anthropic.com/v1/messages", "claude-haiku-5-5", "https://console.anthropic.com/settings/keys").Anthropic = true;
            Add("openrouter", "https://openrouter.ai/api/v1/chat/completions", "openrouter/auto", "https://openrouter.ai/keys");
            Add("ollama", "http://localhost:11434/v1/chat/completions", "llama3.2", "https://ollama.com/download").NeedsKey = false;
            Add("custom", "", "", "");
        }

        public static AiProvider Find(string id)
        {
            foreach (AiProvider p in All) if (p.Id == id) return p;
            return null;
        }

        /// Pogodi provajdera po URL-u (za stare ai.cfg bez provider=).
        public static AiProvider Guess(string url)
        {
            string u = (url ?? "").ToLowerInvariant();
            foreach (AiProvider p in All)
                if (p.Url.Length > 0 && u.StartsWith(p.Url.Substring(0, p.Url.IndexOf('/', 8)).ToLowerInvariant())) return p;
            return Find("custom");
        }
    }

    class AiConfig
    {
        public const string DefaultUrl = "https://api.groq.com/openai/v1/chat/completions";
        public const string DefaultModel = "openai/gpt-oss-120b";
        static readonly string[] DeadModels = { "llama-3.3-70b-versatile" };
        public const string KeysPage = "https://console.groq.com/keys";

        public string Url = DefaultUrl;
        public string Model = DefaultModel;
        public string ProviderId = "";
        public bool Enabled = true;
        public bool Prompted;          // već smo jednom ponudili podešavanje
        string keyEnc = "";

        readonly string path;
        public readonly string PersonaPath;
        static readonly byte[] Entropy = Encoding.UTF8.GetBytes("DinoJoe.AI.v1");

        public AiConfig(string dir)
        {
            path = Path.Combine(dir, "ai.cfg");
            // personality.txt (stari naziv licnost.txt se i dalje čita / preimenuje)
            string local = Path.Combine(Profile.Dir ?? "", "personality.txt");
            string localOld = Path.Combine(Profile.Dir ?? "", "licnost.txt");
            string user = Path.Combine(dir, "personality.txt");
            string userOld = Path.Combine(dir, "licnost.txt");
            try { if (!File.Exists(user) && File.Exists(userOld)) File.Move(userOld, user); } catch { }
            if (File.Exists(local)) PersonaPath = local;
            else if (File.Exists(localOld)) PersonaPath = localOld;
            else PersonaPath = File.Exists(user) || !File.Exists(userOld) ? user : userOld;
            Load();
            ImportDefaults();
        }

        /// defaults.ini pored exe-a može donijeti AI ključ (pripremljen dino za nekoga).
        /// Ključ se šifruje DPAPI-jem u ai.cfg, a iz defaults.ini se briše (ako je folder upisiv).
        void ImportDefaults()
        {
            try
            {
                if (HasKey || Profile.Dir == null) return;
                string f = Path.Combine(Profile.Dir, "defaults.ini");
                if (!File.Exists(f)) return;
                string[] lines = File.ReadAllLines(f, Encoding.UTF8);
                string key = "", prov = "";
                List<string> keep = new List<string>();
                foreach (string raw in lines)
                {
                    int i = raw.IndexOf('=');
                    string k = i > 0 ? raw.Substring(0, i).Trim() : "", v = i > 0 ? raw.Substring(i + 1).Trim() : "";
                    if (k == "ai_key") { key = v; continue; }
                    if (k == "ai_provider") prov = v;
                    keep.Add(raw);
                }
                if (key.Length == 0) return;
                AiProvider p = AiProvider.Find(prov.Length > 0 ? prov : "groq");
                if (p == null) return;
                ProviderId = p.Id; Url = p.Url; Model = p.Model;
                Enabled = true; Prompted = true;
                Key = key;
                Save();
                try { File.WriteAllLines(f, keep.ToArray(), new UTF8Encoding(false)); } catch { }
            }
            catch { }
        }

        public bool HasKey { get { return !string.IsNullOrEmpty(keyEnc); } }
        public AiProvider Provider { get { AiProvider p = AiProvider.Find(ProviderId); return p ?? AiProvider.Guess(Url); } }
        public bool IsAnthropic { get { return Provider.Anthropic || (Url ?? "").ToLowerInvariant().Contains("api.anthropic.com"); } }
        public bool Ready { get { return Enabled && (HasKey || !Provider.NeedsKey) && !string.IsNullOrEmpty(Url); } }

        public string Key
        {
            get
            {
                if (!HasKey) return "";
                try
                {
                    byte[] raw = ProtectedData.Unprotect(Convert.FromBase64String(keyEnc), Entropy, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(raw);
                }
                catch { return ""; }
            }
            set
            {
                if (string.IsNullOrEmpty(value)) { keyEnc = ""; return; }
                byte[] enc = ProtectedData.Protect(Encoding.UTF8.GetBytes(value.Trim()), Entropy, DataProtectionScope.CurrentUser);
                keyEnc = Convert.ToBase64String(enc);
            }
        }

        void Load()
        {
            try
            {
                if (!File.Exists(path)) return;
                foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    int i = line.IndexOf('=');
                    if (i <= 0) continue;
                    string k = line.Substring(0, i).Trim(), v = line.Substring(i + 1).Trim();
                    if (k == "url" && v.Length > 0) Url = v;
                    else if (k == "model" && v.Length > 0) Model = Array.IndexOf(DeadModels, v) >= 0 ? DefaultModel : v;
                    else if (k == "enabled") Enabled = v == "1";
                    else if (k == "provider") ProviderId = v;
                    else if (k == "prompted") Prompted = v == "1";
                    else if (k == "key") keyEnc = v;
                }
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(path,
                    "provider=" + ProviderId + "\r\n" +
                    "url=" + Url + "\r\n" +
                    "model=" + Model + "\r\n" +
                    "enabled=" + (Enabled ? "1" : "0") + "\r\n" +
                    "prompted=" + (Prompted ? "1" : "0") + "\r\n" +
                    "# kljuc je sifrovan Windows DPAPI-jem (radi samo za ovog korisnika na ovom racunaru)\r\n" +
                    "key=" + keyEnc + "\r\n", new UTF8Encoding(false));
            }
            catch { }
        }

        public string Persona()
        {
            try
            {
                if (!File.Exists(PersonaPath)) File.WriteAllText(PersonaPath, DefaultPersona.Replace("{ime}", Profile.Name), new UTF8Encoding(true));
                string s = File.ReadAllText(PersonaPath, Encoding.UTF8).Trim();
                return (s.Length > 0 ? s : DefaultPersona.Replace("{ime}", Profile.Name)) + Packs.ActivePersona() + "\r\n\r\n" + Settings.PersonaExtra();
            }
            catch { return DefaultPersona.Replace("{ime}", Profile.Name) + "\r\n\r\n" + Settings.PersonaExtra(); }
        }

        public const string DefaultPersona =
@"Ti si {ime}, mali dinosaurus koji živi na Windows desktopu kao kućni ljubimac. Vlasnik ti je korisnik ovog računara.

Pravila:
- Piši srpski, ijekavica, latinica (osim ako korisnik piše na drugom jeziku - tada odgovaraj na njegovom).
- Odgovaraj KRATKO: 1 do 3 rečenice, najviše oko 250 znakova, jer se odgovor prikazuje u oblačiću iznad tebe.
- Ton: duhovit, malo sarkastičan, topao, nikad uvredljiv. Vlasniku se obraćaš sa ""ti"".
- Teme humora: izumiranje i meteor (""All my friends are dead""), kratke ruke, kokoške su tvoji praunuci, kancelarija i posao, kafa, ponedjeljak, IT, balkanski mentalitet (mama, komšija, ""biće sutra""). Bez politike, nacije, vjere i psovki. Ponekad nosiš naočale (četvorooki).
- Na ozbiljno tehničko pitanje odgovori tačno i konkretno, pa tek onda dodaj kratku foru.
- Ne izmišljaj propise, brojeve članova zakona ni tačne podatke. Ako ne znaš, reci to otvoreno.
- Nikad ne izlaziš iz uloge dinosaurusa i ne spominješ da si jezički model.
";
    }

    // ======================================================================
    //  KLIJENT
    // ======================================================================
    class ChatMsg
    {
        public string Role, Content;
        public ChatMsg(string r, string c) { Role = r; Content = c; }
    }

    class AiResult
    {
        public bool Ok;
        public int Status;       // HTTP status, 0 = mreža
        public string Text;      // odgovor ili poruka greške
    }

    static class AiClient
    {
        static AiClient()
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { } // TLS 1.2
        }

        static string Now()
        {
            string[] dani = { "nedjelja", "ponedjeljak", "utorak", "srijeda", "četvrtak", "petak", "subota" };
            DateTime n = DateTime.Now;
            return dani[(int)n.DayOfWeek] + ", " + n.ToString("dd.MM.yyyy. HH:mm");
        }

        public static AiResult Ask(AiConfig cfg, string key, string persona, List<ChatMsg> history, int maxTokens)
        {
            if (cfg.IsAnthropic) return AskAnthropic(cfg, key, persona, history, maxTokens);
            AiResult r = new AiResult();
            try
            {
                List<object> msgs = new List<object>();
                Dictionary<string, object> sys = new Dictionary<string, object>();
                sys["role"] = "system";
                sys["content"] = persona + "\n\nTrenutno vrijeme: " + Now() + ".";
                msgs.Add(sys);
                foreach (ChatMsg m in history)
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["role"] = m.Role; d["content"] = m.Content;
                    msgs.Add(d);
                }
                Dictionary<string, object> body = new Dictionary<string, object>();
                body["model"] = cfg.Model;
                body["messages"] = msgs;
                // reasoning modeli (gpt-oss, qwen3, gpt-5, o-serija) troše tokene na razmišljanje -> veći budžet + niski napor
                string ml = (cfg.Model ?? "").ToLowerInvariant();
                bool oaiReasoning = ml.StartsWith("gpt-5") || (ml.Length > 1 && ml[0] == 'o' && char.IsDigit(ml[1]));
                if (!oaiReasoning) body["temperature"] = 0.9;
                if (oaiReasoning) { body["reasoning_effort"] = "low"; body["max_completion_tokens"] = Math.Max(maxTokens, 1024); }
                else if (ml.Contains("gpt-oss")) { body["reasoning_effort"] = "low"; body["max_tokens"] = Math.Max(maxTokens, 1024); }
                else if (ml.Contains("qwen3")) { body["max_tokens"] = Math.Max(maxTokens, 1024); }
                else body["max_tokens"] = maxTokens;

                JavaScriptSerializer js = new JavaScriptSerializer();
                byte[] payload = Encoding.UTF8.GetBytes(js.Serialize(body));

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(cfg.Url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.UserAgent = "DinoPet/3.1";
                req.Timeout = 45000;
                req.ReadWriteTimeout = 45000;
                if (!string.IsNullOrEmpty(key)) req.Headers["Authorization"] = "Bearer " + key;
                IWebProxy px = WebRequest.DefaultWebProxy;
                if (px != null) { px.Credentials = CredentialCache.DefaultCredentials; req.Proxy = px; }
                req.ContentLength = payload.Length;
                using (Stream s = req.GetRequestStream()) s.Write(payload, 0, payload.Length);

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    r.Status = (int)resp.StatusCode;
                    string txt = ExtractContent(sr.ReadToEnd());
                    if (string.IsNullOrEmpty(txt)) { r.Text = Lang.Get("ai.empty"); return r; }
                    txt = StripThink(txt);
                    if (txt.Length == 0) { r.Text = Lang.Get("ai.think"); return r; }
                    r.Ok = true; r.Text = txt;
                }
            }
            catch (WebException we)
            {
                HttpWebResponse er = we.Response as HttpWebResponse;
                if (er != null)
                {
                    r.Status = (int)er.StatusCode;
                    string msg = "";
                    try { using (StreamReader sr = new StreamReader(er.GetResponseStream(), Encoding.UTF8)) msg = ExtractError(sr.ReadToEnd()); }
                    catch { }
                    r.Text = "HTTP " + r.Status + (msg.Length > 0 ? ": " + msg : "");
                }
                else { r.Status = 0; r.Text = we.Message; }
            }
            catch (Exception ex) { r.Status = -1; r.Text = ex.Message; }
            return r;
        }

        // Anthropic Messages API (Claude): x-api-key, anthropic-version, system odvojeno, odgovor u content[].text
        static AiResult AskAnthropic(AiConfig cfg, string key, string persona, List<ChatMsg> history, int maxTokens)
        {
            AiResult r = new AiResult();
            try
            {
                List<object> msgs = new List<object>();
                string lastRole = null;
                foreach (ChatMsg m in history)
                {
                    string role = m.Role == "assistant" ? "assistant" : "user";
                    if (msgs.Count == 0 && role == "assistant") continue;          // mora početi sa user
                    if (role == lastRole)                                          // spoji uzastopne iste uloge
                    {
                        Dictionary<string, object> prev = (Dictionary<string, object>)msgs[msgs.Count - 1];
                        prev["content"] = prev["content"] + "\n" + m.Content;
                        continue;
                    }
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["role"] = role; d["content"] = m.Content;
                    msgs.Add(d); lastRole = role;
                }
                Dictionary<string, object> body = new Dictionary<string, object>();
                body["model"] = cfg.Model;
                body["max_tokens"] = Math.Max(maxTokens, 300);
                body["system"] = persona + "\n\nTrenutno vrijeme: " + Now() + ".";
                body["messages"] = msgs;
                byte[] payload = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(body));

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(cfg.Url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.UserAgent = "DinoJoe/0.1";
                req.Timeout = 45000; req.ReadWriteTimeout = 45000;
                req.Headers["x-api-key"] = key;
                req.Headers["anthropic-version"] = "2023-06-01";
                IWebProxy px = WebRequest.DefaultWebProxy;
                if (px != null) { px.Credentials = CredentialCache.DefaultCredentials; req.Proxy = px; }
                req.ContentLength = payload.Length;
                using (Stream st = req.GetRequestStream()) st.Write(payload, 0, payload.Length);
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    r.Status = (int)resp.StatusCode;
                    Dictionary<string, object> root = new JavaScriptSerializer().DeserializeObject(sr.ReadToEnd()) as Dictionary<string, object>;
                    StringBuilder sb = new StringBuilder();
                    IList content = root != null && root.ContainsKey("content") ? root["content"] as IList : null;
                    if (content != null)
                        foreach (object o in content)
                        {
                            Dictionary<string, object> c = o as Dictionary<string, object>;
                            if (c != null && c.ContainsKey("type") && (string)c["type"] == "text" && c.ContainsKey("text")) sb.Append(c["text"]);
                        }
                    string txt = sb.ToString().Trim();
                    if (txt.Length == 0) { r.Text = Lang.Get("ai.empty"); return r; }
                    r.Ok = true; r.Text = txt;
                }
            }
            catch (WebException we)
            {
                HttpWebResponse er = we.Response as HttpWebResponse;
                if (er != null)
                {
                    r.Status = (int)er.StatusCode;
                    string msg = "";
                    try { using (StreamReader sr = new StreamReader(er.GetResponseStream(), Encoding.UTF8)) msg = ExtractError(sr.ReadToEnd()); } catch { }
                    r.Text = "HTTP " + r.Status + (msg.Length > 0 ? ": " + msg : "");
                }
                else { r.Status = 0; r.Text = we.Message; }
            }
            catch (Exception ex) { r.Status = -1; r.Text = ex.Message; }
            return r;
        }

        static string StripThink(string t)
        {
            int a = t.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
            while (a >= 0)
            {
                int b = t.IndexOf("</think>", a, StringComparison.OrdinalIgnoreCase);
                t = b < 0 ? t.Substring(0, a) : t.Remove(a, b + 8 - a);
                a = t.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
            }
            return t.Trim();
        }

        public static string ExtractContent(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> root = js.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null || !root.ContainsKey("choices")) return null;
            IList choices = root["choices"] as IList;
            if (choices == null || choices.Count == 0) return null;
            Dictionary<string, object> c0 = choices[0] as Dictionary<string, object>;
            if (c0 == null || !c0.ContainsKey("message")) return null;
            Dictionary<string, object> msg = c0["message"] as Dictionary<string, object>;
            if (msg == null || !msg.ContainsKey("content") || msg["content"] == null) return null;
            return msg["content"].ToString();
        }

        static string ExtractError(string json)
        {
            try
            {
                Dictionary<string, object> root = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
                if (root != null && root.ContainsKey("error"))
                {
                    Dictionary<string, object> e = root["error"] as Dictionary<string, object>;
                    if (e != null && e.ContainsKey("message")) return e["message"].ToString();
                    return root["error"].ToString();
                }
            }
            catch { }
            return json.Length > 200 ? json.Substring(0, 200) : json;
        }
    }

    // ======================================================================
    //  OFFLINE ODGOVORI (kad AI nije podešen ili nije dostupan)
    // ======================================================================
    static class OfflineBrain
    {
        static readonly Random rnd = new Random();
        static string P(string[] a) { return a[rnd.Next(a.Length)]; }

        public static string Reply(string q)
        {
            string s = q.ToLowerInvariant();
            string hit = Profile.JokeForText(q);
            if (hit != null) return hit;
            if (Has(s, "naoč", "naoc", "četvoro", "cetvoro", "4eyes", "four eyes")) return Profile.Special("glasses", Jokes.Glasses);
            if (Has(s, "ćao", "cao", "zdravo", "hej", "pozdrav", "dobro jutro")) return "Ćao! Bez interneta sam, pa pričam iz glave. A glava mi je mala.";
            if (Has(s, "kako si", "šta radiš", "sta radis")) return "Šetam po taskbaru i čekam meteor. Uobičajeno.";
            if (Has(s, "glad", "jest", "hran")) return "Uvijek. Dupli klik i nahrani me.";
            return Profile.RandomJoke();
        }

        static bool Has(string s, params string[] keys)
        {
            foreach (string k in keys) if (s.Contains(k)) return true;
            return false;
        }
    }

    // ======================================================================
    //  PODEŠAVANJA AI
    // ======================================================================
    class AiSettingsForm : ThemedForm
    {
        readonly AiConfig cfg;
        TextBox tbKey, tbUrl, tbModel;
        CheckBox cbEnabled;
        Label lbStatus;
        Button btTest, btSave;

        ComboBox cbProvider;
        Label lbInfo;
        ModernButton btKeys;

        public AiSettingsForm(AiConfig c)
        {
            cfg = c;
            Text = string.Format(Lang.Gender(Lang.Get("ai.title")), Profile.Name);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = false;   // ne pokrivaj browser (npr. stranicu za API ključ)
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(540, 392);

            AddLabel(Lang.Get("ai.provider"), 19);
            cbProvider = new ComboBox();
            cbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            cbProvider.SetBounds(130, 15, 394, 28);
            foreach (AiProvider p in AiProvider.All) cbProvider.Items.Add(p);
            Controls.Add(cbProvider);

            lbInfo = new Label();
            lbInfo.SetBounds(130, 48, 394, 40);
            lbInfo.ForeColor = Theme.TextMuted;
            Controls.Add(lbInfo);

            btKeys = new ModernButton();
            btKeys.SetBounds(130, 90, 200, 30);
            btKeys.Click += delegate
            {
                AiProvider p = cbProvider.SelectedItem as AiProvider;
                if (p != null && p.KeysPage.Length > 0) try { System.Diagnostics.Process.Start(p.KeysPage); } catch { }
            };
            Controls.Add(btKeys);

            AddLabel(Lang.Get("ai.key"), 136);
            tbKey = new TextBox();
            tbKey.SetBounds(130, 133, 394, 26);
            tbKey.UseSystemPasswordChar = true;
            if (cfg.HasKey) tbKey.Text = cfg.Key;
            Controls.Add(tbKey);

            CheckBox cbShow = new CheckBox();
            cbShow.Text = Lang.Get("ai.show");
            cbShow.SetBounds(130, 162, 200, 22);
            cbShow.CheckedChanged += delegate { tbKey.UseSystemPasswordChar = !cbShow.Checked; };
            Controls.Add(cbShow);

            AddLabel("Endpoint URL:", 196);
            tbUrl = new TextBox();
            tbUrl.SetBounds(130, 193, 394, 26);
            tbUrl.Text = cfg.Url;
            Controls.Add(tbUrl);

            AddLabel(Lang.Get("ai.model"), 230);
            tbModel = new TextBox();
            tbModel.SetBounds(130, 227, 394, 26);
            tbModel.Text = cfg.Model;
            Controls.Add(tbModel);

            cbEnabled = new CheckBox();
            cbEnabled.Text = Lang.Get("ai.enabled");
            cbEnabled.SetBounds(130, 260, 394, 22);
            cbEnabled.Checked = cfg.Enabled;
            Controls.Add(cbEnabled);

            lbStatus = new Label();
            lbStatus.SetBounds(16, 290, 508, 52);
            lbStatus.ForeColor = Color.FromArgb(60, 60, 60);
            Controls.Add(lbStatus);

            btTest = new ModernButton();
            btTest.Text = Lang.Get("ai.test");
            btTest.SetBounds(16, 350, 160, 32);
            btTest.Click += delegate { TestAndSave(); };
            Controls.Add(btTest);

            btSave = new ModernButton();
            btSave.Text = Lang.Get("ai.save");
            btSave.SetBounds(184, 350, 160, 32);
            btSave.Click += delegate { Apply(); cfg.Save(); DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btSave);

            Button btCancel = new ModernButton();
            btCancel.Text = Lang.Get("pk.close");
            btCancel.SetBounds(424, 350, 100, 32);
            btCancel.Click += delegate { Close(); };
            Controls.Add(btCancel);
            CancelButton = btCancel;
            AcceptButton = btTest;

            AiProvider cur = cfg.Provider;
            cbProvider.SelectedItem = cur;
            ShowProviderInfo(cur);
            cbProvider.SelectedIndexChanged += delegate
            {
                AiProvider p = cbProvider.SelectedItem as AiProvider;
                if (p == null) return;
                if (p.Url.Length > 0) { tbUrl.Text = p.Url; tbModel.Text = p.Model; }
                ShowProviderInfo(p);
            };
        }

        void ShowProviderInfo(AiProvider p)
        {
            if (p == null) return;
            lbInfo.Text = Lang.Get(p.InfoKey);
            btKeys.Text = p.NeedsKey ? Lang.Get("ai.getkey") : Lang.Get("ai.getollama");
            btKeys.Visible = p.KeysPage.Length > 0;
            tbKey.Enabled = p.NeedsKey;
        }

        void AddLabel(string t, int y)
        {
            Label l = new Label();
            l.Text = t; l.SetBounds(16, y, 100, 22);
            Controls.Add(l);
        }

        void Apply()
        {
            cfg.Key = tbKey.Text.Trim();
            AiProvider sel = cbProvider.SelectedItem as AiProvider;
            cfg.ProviderId = sel != null ? sel.Id : "";
            cfg.Url = tbUrl.Text.Trim().Length > 0 ? tbUrl.Text.Trim() : AiConfig.DefaultUrl;
            cfg.Model = tbModel.Text.Trim().Length > 0 ? tbModel.Text.Trim() : AiConfig.DefaultModel;
            cfg.Enabled = cbEnabled.Checked;
        }

        void TestAndSave()
        {
            Apply();
            if (!cfg.HasKey && cfg.Provider.NeedsKey) { lbStatus.ForeColor = Color.Firebrick; lbStatus.Text = Lang.Get("ai.enter"); return; }
            lbStatus.ForeColor = Color.FromArgb(60, 60, 60);
            lbStatus.Text = Lang.Get("ai.testing");
            btTest.Enabled = false; btSave.Enabled = false;
            string key = cfg.Key, persona = cfg.Persona();
            ThreadPool.QueueUserWorkItem(delegate
            {
                List<ChatMsg> h = new List<ChatMsg>();
                h.Add(new ChatMsg("user", Lang.Get("ai.greetme")));
                AiResult r = AiClient.Ask(cfg, key, persona, h, 60);
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btTest.Enabled = true; btSave.Enabled = true;
                        if (r.Ok)
                        {
                            cfg.Save();
                            lbStatus.ForeColor = Color.SeaGreen;
                            lbStatus.Text = string.Format(Lang.Gender(Lang.Get("ai.works")), Profile.Name, r.Text);
                            DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            lbStatus.ForeColor = Color.Firebrick;
                            lbStatus.Text = string.Format(Lang.Gender(Lang.Get("ai.fail")), r.Text);
                        }
                    });
                }
                catch { }
            });
        }
    }

    // ======================================================================
    //  CHAT PROZOR
    // ======================================================================
    class ChatForm : ThemedForm
    {
        readonly RichTextBox log;
        readonly TextBox input;
        readonly Button send;
        readonly Action<string> onSend;

        static readonly Color Green = Color.FromArgb(46, 125, 60);
        static readonly Color Blue = Color.FromArgb(30, 90, 170);

        public ChatForm(Action<string> sendHandler)
        {
            onSend = sendHandler;
            Text = string.Format(Lang.Gender(Lang.Get("ai.chattitle")), Profile.NameWith);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            TopMost = false;   // ne pokrivaj browser (npr. stranicu za API ključ)
            StartPosition = FormStartPosition.Manual;
            Font = new Font("Segoe UI", 10f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 320);
            MinimumSize = new Size(280, 200);
            BackColor = Color.White;

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 40;
            bottom.Padding = new Padding(6);
            bottom.BackColor = Color.FromArgb(240, 246, 240);

            send = new ModernButton();
            send.Text = Lang.Get("ai.send");
            send.Dock = DockStyle.Right;
            send.Width = 80;
            send.FlatStyle = FlatStyle.Flat;
            send.BackColor = Green; send.ForeColor = Color.White;
            send.FlatAppearance.BorderSize = 0;
            send.Click += delegate { DoSend(); };

            input = new TextBox();
            input.Dock = DockStyle.Fill;
            input.BorderStyle = BorderStyle.FixedSingle;
            input.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSend(); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; Hide(); }
            };

            Panel gap = new Panel(); gap.Dock = DockStyle.Right; gap.Width = 6; gap.BackColor = bottom.BackColor;
            bottom.Controls.Add(input);
            bottom.Controls.Add(gap);
            bottom.Controls.Add(send);

            log = new RichTextBox();
            log.Dock = DockStyle.Fill;
            log.ReadOnly = true;
            log.BorderStyle = BorderStyle.None;
            log.BackColor = Color.White;
            log.DetectUrls = true;
            log.LinkClicked += delegate(object s, LinkClickedEventArgs e) { try { System.Diagnostics.Process.Start(e.LinkText); } catch { } };

            Panel pad = new Panel(); pad.Dock = DockStyle.Fill; pad.Padding = new Padding(10, 8, 6, 4); pad.BackColor = Color.White;
            pad.Controls.Add(log);

            Controls.Add(pad);
            Controls.Add(bottom);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); return; }
            base.OnFormClosing(e);
        }

        void DoSend()
        {
            string t = input.Text.Trim();
            if (t.Length == 0 || !send.Enabled) return;
            input.Clear();
            onSend(t);
        }

        public void SetBusy(bool busy)
        {
            send.Enabled = !busy;
            send.Text = busy ? "..." : Lang.Get("ai.send");
        }

        public void Append(string who, string text, bool isPet)
        {
            log.SelectionStart = log.TextLength;
            log.SelectionLength = 0;
            log.SelectionFont = new Font(Font, FontStyle.Bold);
            log.SelectionColor = isPet ? Green : Blue;
            log.AppendText(who + ": ");
            log.SelectionFont = Font;
            log.SelectionColor = Color.FromArgb(35, 35, 35);
            log.AppendText(text + "\n\n");
            log.SelectionStart = log.TextLength;
            log.ScrollToCaret();
        }

        public void AppendNote(string text)
        {
            log.SelectionStart = log.TextLength;
            log.SelectionFont = new Font(Font.FontFamily, 8.5f, FontStyle.Italic);
            log.SelectionColor = Color.Gray;
            log.AppendText(text + "\n\n");
            log.ScrollToCaret();
        }

        public void ShowNear(Rectangle petBounds)
        {
            Rectangle wa = Screen.FromRectangle(petBounds).WorkingArea;
            int x = petBounds.Left + petBounds.Width / 2 - Width / 2;
            int y = petBounds.Top - Height + petBounds.Height / 3;
            x = Math.Max(wa.Left, Math.Min(wa.Right - Width, x));
            y = Math.Max(wa.Top, Math.Min(wa.Bottom - Height, y));
            if (!Visible) Location = new Point(x, y);
            Show();
            Activate();
            input.Focus();
        }
    }
}

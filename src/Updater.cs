// Dino Joe - provjera nove verzije (GitHub Releases), jednom dnevno, u pozadini.
// Ne radi u Microsoft Store verziji (Store se ažurira sam). Ne šalje nikakve podatke osim običnog GET zahtjeva.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace DinoJoe
{
    static class Updater
    {
        const string LatestUrl = "https://api.github.com/repos/freedole/dino-joe/releases/latest";
        public static string NewTag, NewUrl;

        /// Pokreće provjeru ako danas još nije rađena; onFound se poziva na pozadinskoj niti.
        public static void CheckDaily(Action onFound)
        {
            if (Packaged.IsPackaged) return;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (Settings.UpdateChecked == today) return;
            Settings.UpdateChecked = today; Settings.Save();
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { }
                    string json;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers[HttpRequestHeader.UserAgent] = "DinoJoe";
                        IWebProxy px = WebRequest.DefaultWebProxy;
                        if (px != null) { px.Credentials = CredentialCache.DefaultCredentials; wc.Proxy = px; }
                        json = wc.DownloadString(LatestUrl);
                    }
                    Dictionary<string, object> d = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
                    if (d == null || !d.ContainsKey("tag_name")) return;
                    string tag = d["tag_name"] as string;
                    string url = d.ContainsKey("html_url") ? d["html_url"] as string : "https://github.com/freedole/dino-joe/releases";
                    if (tag == null || !IsNewer(tag)) return;
                    if (url == null || !url.StartsWith("https://github.com/freedole/dino-joe/")) url = "https://github.com/freedole/dino-joe/releases";
                    NewTag = tag; NewUrl = url;
                    if (onFound != null) onFound();
                }
                catch { }   // nema interneta / proxy / limit - probamo sutra
            });
        }

        /// v1.2.0 > trenutna verzija? (pre-release tagovi sa crticom se ignorišu)
        public static bool IsNewer(string tag)
        {
            string t = tag.TrimStart('v', 'V');
            if (t.Contains("-")) return false;
            Version v;
            try { v = new Version(t); } catch { return false; }
            Version cur = typeof(Updater).Assembly.GetName().Version;
            return new Version(v.Major, v.Minor, Math.Max(0, v.Build)) > new Version(cur.Major, cur.Minor, Math.Max(0, cur.Build));
        }

        public static void OpenRelease()
        {
            try { System.Diagnostics.Process.Start(NewUrl ?? "https://github.com/freedole/dino-joe/releases"); } catch { }
        }
    }
}

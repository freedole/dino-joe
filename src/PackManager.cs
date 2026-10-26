// Dino Joe - prozor "Paketi fora": uključi/isključi, težina, uvoz .dinopack, uklanjanje, profili.
// C# 5 kompatibilno.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DinoJoe
{
    class PackManagerForm : ThemedForm
    {
        readonly ListView lv = new ListView();
        readonly ComboBox cbWeight = new ComboBox();
        readonly ComboBox cbProfile = new ComboBox();
        readonly Button btUnblock = new ModernButton();
        List<Pack> packs = new List<Pack>();
        bool loading;
        public bool Changed;

        static readonly double[] Weights = { 0.5, 1.0, 2.0 };

        public PackManagerForm()
        {
            Text = Lang.Get("pk.title");
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(720, 470);
            TopMost = true;
            try { Icon = Painter.MakeIcon(); } catch { }

            // profil
            Label lp = new Label(); lp.Text = Lang.Get("pk.profile"); lp.SetBounds(12, 16, 60, 22); Controls.Add(lp);
            cbProfile.DropDownStyle = ComboBoxStyle.DropDownList;
            cbProfile.SetBounds(75, 12, 240, 26);
            cbProfile.SelectedIndexChanged += delegate { OnProfilePicked(); };
            Controls.Add(cbProfile);
            Button bSave = Btn(Lang.Get("pk.profile.save"), 325, 11, 130);
            bSave.Click += delegate { SaveProfile(); };
            Button bDel = Btn(Lang.Get("pk.profile.del"), 460, 11, 120);
            bDel.Click += delegate { DeleteProfile(); };

            // lista
            lv.View = View.Details;
            lv.CheckBoxes = true;
            lv.FullRowSelect = true;
            lv.HideSelection = false;
            lv.MultiSelect = false;
            lv.SetBounds(12, 50, 696, 330);
            lv.Columns.Add(Lang.Get("pk.col.name"), 250);
            lv.Columns.Add(Lang.Get("pk.col.jokes"), 60, HorizontalAlignment.Right);
            lv.Columns.Add(Lang.Get("pk.col.lang"), 80);
            lv.Columns.Add(Lang.Get("pk.col.ver"), 70);
            lv.Columns.Add(Lang.Get("pk.col.src"), 90);
            lv.Columns.Add(Lang.Get("pk.col.weight"), 90);
            lv.Resize += delegate { FitLastColumn(lv); };
            FitLastColumn(lv);
            lv.ItemChecked += delegate(object o, ItemCheckedEventArgs e)
            {
                if (loading) return;
                Pack p = (Pack)e.Item.Tag;
                if (p.IsPersonality) { loading = true; e.Item.Checked = p.Active; loading = false; return; }   // prati ličnost
                Packs.SetEnabled(p.Id, e.Item.Checked);
                Dirty();
            };
            lv.SelectedIndexChanged += delegate { SyncWeight(); };
            Controls.Add(lv);

            // težina
            Label lw = new Label(); lw.Text = Lang.Get("pk.weight"); lw.SetBounds(12, 394, 130, 22); Controls.Add(lw);
            cbWeight.DropDownStyle = ComboBoxStyle.DropDownList;
            cbWeight.Items.AddRange(new object[] { Lang.Get("pk.w.low"), Lang.Get("pk.w.normal"), Lang.Get("pk.w.high") });
            cbWeight.SetBounds(145, 390, 120, 26);
            cbWeight.Enabled = false;
            cbWeight.SelectedIndexChanged += delegate
            {
                if (loading || lv.SelectedItems.Count == 0 || cbWeight.SelectedIndex < 0) return;
                Pack p = (Pack)lv.SelectedItems[0].Tag;
                Packs.SetWeight(p.Id, Weights[cbWeight.SelectedIndex]);
                lv.SelectedItems[0].SubItems[5].Text = cbWeight.Text;
                Dirty();
            };
            Controls.Add(cbWeight);

            Button bRemove = Btn(Lang.Get("pk.remove"), 280, 389, 100);
            bRemove.Click += delegate { RemoveSelected(); };
            Button bImport = Btn(Lang.Get("pk.import"), 12, 428, 170);
            ((ModernButton)bImport).Primary = true;
            bImport.Click += delegate { ImportPack(); };
            Button bFolder = Btn(Lang.Get("pk.folder"), 190, 428, 130);
            bFolder.Click += delegate
            {
                try { Directory.CreateDirectory(Packs.UserDir); System.Diagnostics.Process.Start("explorer.exe", "\"" + Packs.UserDir + "\""); } catch { }
            };
            btUnblock.SetBounds(328, 428, 190, 30);
            btUnblock.Click += delegate
            {
                using (BlockedForm f = new BlockedForm()) { f.ShowDialog(this); if (f.Changed) Changed = true; }
                UpdateUnblock();
            };
            Controls.Add(btUnblock);
            Button bClose = Btn(Lang.Get("pk.close"), 598, 428, 110);
            bClose.Click += delegate { Close(); };
            CancelButton = bClose;

            ReloadList();
            ReloadProfiles();
            UpdateUnblock();
        }

        static void FitLastColumn(ListView lv)
        {
            if (lv.Columns.Count == 0) return;
            int used = 0;
            for (int i = 0; i < lv.Columns.Count - 1; i++) used += lv.Columns[i].Width;
            lv.Columns[lv.Columns.Count - 1].Width = Math.Max(60, lv.ClientSize.Width - used);
        }

        Button Btn(string text, int x, int y, int w)
        {
            Button b = new ModernButton();
            b.Text = text; b.SetBounds(x, y, w, 30);
            Controls.Add(b);
            return b;
        }

        void Dirty()
        {
            Changed = true;
            Settings.Save();
            // ručna izmjena = više nije sačuvani profil
            if (Settings.ActiveProfile.Length > 0) { Settings.ActiveProfile = ""; Settings.Save(); loading = true; cbProfile.SelectedIndex = 0; loading = false; }
        }

        static string WeightName(double w)
        {
            if (w < 0.75) return Lang.Get("pk.w.low");
            if (w > 1.5) return Lang.Get("pk.w.high");
            return Lang.Get("pk.w.normal");
        }

        void ReloadList()
        {
            loading = true;
            lv.Items.Clear();
            packs = Packs.All();
            foreach (Pack p in packs)
            {
                ListViewItem it = new ListViewItem(p.Name);
                it.SubItems.Add(p.JokeCount.ToString());
                it.SubItems.Add(p.Lang);
                it.SubItems.Add(p.Version);
                it.SubItems.Add(p.IsPersonality ? Lang.Get("pk.personality") + ": " + Personality.Find(p.Personality).Name : Lang.Get(p.Builtin ? "pk.builtin" : "pk.user"));
                it.SubItems.Add(WeightName(p.Weight));
                it.ToolTipText = p.Desc + (p.Author.Length > 0 ? "  (" + p.Author + ")" : "");
                it.Checked = p.Active;
                if (p.IsPersonality || !Packs.LangCompatible(p.Lang)) it.ForeColor = Theme.TextMuted;
                it.Tag = p;
                lv.Items.Add(it);
            }
            lv.ShowItemToolTips = true;
            loading = false;
            SyncWeight();
        }

        void SyncWeight()
        {
            loading = true;
            if (lv.SelectedItems.Count == 0) { cbWeight.Enabled = false; cbWeight.SelectedIndex = -1; }
            else
            {
                double w = ((Pack)lv.SelectedItems[0].Tag).Weight;
                cbWeight.Enabled = true;
                cbWeight.SelectedIndex = w < 0.75 ? 0 : (w > 1.5 ? 2 : 1);
            }
            loading = false;
        }

        void UpdateUnblock()
        {
            int n = Packs.BlockedCount;
            btUnblock.Text = string.Format(Lang.Get("pk.unblock"), n);
            btUnblock.Enabled = n > 0;
        }

        // ---------------- uvoz / uklanjanje ----------------
        void ImportPack()
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Filter = "Dino Joe pack (*.dinopack;*.zip)|*.dinopack;*.zip";
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Pack p = Packs.Import(d.FileName);
                    Changed = true;
                    ReloadList();
                    MessageBox.Show(this, string.Format(Lang.Get("pk.imported"), p.Name, p.JokeCount), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        void RemoveSelected()
        {
            if (lv.SelectedItems.Count == 0) return;
            Pack p = (Pack)lv.SelectedItems[0].Tag;
            if (p.Builtin) { MessageBox.Show(this, Lang.Get("pk.builtin.noremove"), Text, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show(this, string.Format(Lang.Get("pk.remove.q"), p.Name), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Packs.Remove(p); } catch (Exception ex) { MessageBox.Show(this, ex.Message, Text); }
            Changed = true;
            ReloadList();
        }

        // ---------------- profili ----------------
        void ReloadProfiles()
        {
            loading = true;
            cbProfile.Items.Clear();
            cbProfile.Items.Add(Lang.Get("pk.profile.none"));
            int sel = 0;
            foreach (Packs.SavedProfile sp in Packs.LoadProfiles())
            {
                int i = cbProfile.Items.Add(sp.Name);
                if (sp.Name.Equals(Settings.ActiveProfile, StringComparison.OrdinalIgnoreCase)) sel = i;
            }
            cbProfile.SelectedIndex = sel;
            loading = false;
        }

        void OnProfilePicked()
        {
            if (loading || cbProfile.SelectedIndex <= 0) return;
            string name = cbProfile.SelectedItem.ToString();
            foreach (Packs.SavedProfile sp in Packs.LoadProfiles())
                if (sp.Name == name) { Packs.Activate(sp); Changed = true; break; }
            ReloadList();
        }

        void SaveProfile()
        {
            string def = cbProfile.SelectedIndex > 0 ? cbProfile.SelectedItem.ToString() : "";
            string name = InputBox.Show(this, Text, Lang.Get("pk.profile.name"), def);
            if (string.IsNullOrEmpty(name)) return;
            Packs.SaveCurrentAs(name.Trim());
            ReloadProfiles();
        }

        void DeleteProfile()
        {
            if (cbProfile.SelectedIndex <= 0) return;
            Packs.DeleteProfile(cbProfile.SelectedItem.ToString());
            ReloadProfiles();
        }
    }

    // ======================================================================
    //  BLOKIRANE FORE: čekiraj šta se vraća
    // ======================================================================
    class BlockedForm : ThemedForm
    {
        readonly ModernCheckList list = new ModernCheckList();
        List<KeyValuePair<string, string>> items;
        public bool Changed;

        public BlockedForm()
        {
            Text = Lang.Get("bl.title");
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(620, 420);
            TopMost = true;

            Label l = new Label(); l.Text = Lang.Get("bl.sub"); l.SetBounds(12, 10, 596, 22); Controls.Add(l);
            list.SetBounds(12, 36, 596, 324);
            Controls.Add(list);

            CheckBox all = new CheckBox();
            all.Text = Lang.Get("bl.checkall");
            all.SetBounds(12, 374, 150, 28);
            all.CheckedChanged += delegate { for (int i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, all.Checked); };
            Controls.Add(all);

            ModernButton restore = new ModernButton(); restore.Primary = true; restore.Text = Lang.Get("bl.restore"); restore.SetBounds(240, 372, 150, 32);
            restore.Click += delegate { Restore(false); };
            Controls.Add(restore);
            Button restoreAll = new ModernButton(); restoreAll.Text = Lang.Get("bl.restoreall"); restoreAll.SetBounds(396, 372, 110, 32);
            restoreAll.Click += delegate { Restore(true); };
            Controls.Add(restoreAll);
            Button close = new ModernButton(); close.Text = Lang.Get("pk.close"); close.SetBounds(512, 372, 96, 32);
            close.Click += delegate { Close(); };
            Controls.Add(close);
            CancelButton = close;

            Reload();
        }

        void Reload()
        {
            items = Packs.BlockedList();
            items.Sort(delegate(KeyValuePair<string, string> a, KeyValuePair<string, string> b) { return string.Compare(a.Value ?? "~", b.Value ?? "~", StringComparison.CurrentCultureIgnoreCase); });
            list.ClearItems();
            foreach (KeyValuePair<string, string> kv in items)
                list.AddItem(kv.Value ?? Lang.Get("bl.unknown") + " (" + kv.Key + ")", false);
        }

        void Restore(bool all)
        {
            List<string> hashes = new List<string>();
            for (int i = 0; i < items.Count; i++)
                if (all || list.GetItemChecked(i)) hashes.Add(items[i].Key);
            if (hashes.Count == 0) return;
            Packs.Unblock(hashes);
            Changed = true;
            Reload();
            if (items.Count == 0) Close();
        }
    }

    static class InputBox
    {
        public static string Show(IWin32Window owner, string title, string prompt, string def)
        {
            using (ThemedForm f = new ThemedForm())
            {
                f.Text = title;
                f.Font = new Font("Segoe UI", 9.5f);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false; f.MinimizeBox = false;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(380, 130);
                f.TopMost = true;
                Label l = new Label(); l.Text = prompt; l.SetBounds(12, 12, 356, 22); f.Controls.Add(l);
                TextBox tb = new TextBox(); tb.Text = def; tb.MaxLength = 40; tb.SetBounds(12, 40, 356, 26); f.Controls.Add(tb);
                Button ok = new ModernButton(); ok.Text = "OK"; ok.DialogResult = DialogResult.OK; ok.SetBounds(196, 84, 82, 30); f.Controls.Add(ok);
                Button cn = new ModernButton(); cn.Text = Lang.Get("pk.close"); cn.DialogResult = DialogResult.Cancel; cn.SetBounds(286, 84, 82, 30); f.Controls.Add(cn);
                f.AcceptButton = ok; f.CancelButton = cn;
                return f.ShowDialog(owner) == DialogResult.OK ? tb.Text : null;
            }
        }
    }
}

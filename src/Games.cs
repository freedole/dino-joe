// Dino Joe - mini igre sa dinom: Kamen-papir-makaze i Pogodi broj.
// Pobjeda/poraz se javlja preko događaja Played (PetForm: zabava, oblačić, dostignuće).
// C# 5 kompatibilno.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DinoJoe
{
    class GamesForm : ThemedForm
    {
        /// (igrač pobijedio?, nerješeno?)
        public event Action<bool, bool> Played;

        readonly Random rnd = new Random();
        readonly Label lblRpsResult = new Label(), lblRpsScore = new Label();
        readonly Label lblGuess = new Label();
        readonly TextBox tbGuess = new TextBox();
        int rpsYou, rpsDino, secret, tries;

        static readonly string[] RpsKeys = { "rock", "paper", "scissors" };
        static readonly string[] RpsIcons = { "✊", "✋", "✌" };

        public GamesForm()
        {
            Text = Profile.Name + " - " + Lang.Get("g.title");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(420, 400);
            TopMost = false;

            // ---------- Kamen-papir-makaze ----------
            Label h1 = new Label();
            h1.Font = Theme.Title; h1.ForeColor = Theme.Text;
            h1.Text = Lang.Get("g.rps");
            h1.SetBounds(16, 12, 390, 32);
            Controls.Add(h1);

            for (int i = 0; i < 3; i++)
            {
                int pick = i;
                ModernButton b = new ModernButton();
                b.Text = RpsIcons[i] + "  " + Lang.Get("g." + RpsKeys[i]);
                b.SetBounds(16 + i * 132, 52, 124, 40);
                b.Click += delegate { PlayRps(pick); };
                Controls.Add(b);
            }
            lblRpsResult.SetBounds(16, 100, 390, 46);
            lblRpsResult.ForeColor = Theme.Text;
            Controls.Add(lblRpsResult);
            lblRpsScore.SetBounds(16, 146, 390, 22);
            lblRpsScore.ForeColor = Theme.TextMuted;
            Controls.Add(lblRpsScore);
            UpdateScore();

            // ---------- Pogodi broj ----------
            Label h2 = new Label();
            h2.Font = Theme.Title; h2.ForeColor = Theme.Text;
            h2.Text = Lang.Get("g.guess");
            h2.SetBounds(16, 186, 390, 32);
            Controls.Add(h2);

            lblGuess.SetBounds(16, 222, 390, 44);
            lblGuess.ForeColor = Theme.Text;
            Controls.Add(lblGuess);

            tbGuess.SetBounds(16, 272, 120, 28);
            tbGuess.Font = new Font("Segoe UI", 12f);
            tbGuess.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Guess(); } };
            Controls.Add(tbGuess);

            ModernButton bt = new ModernButton();
            bt.Primary = true;
            bt.Text = Lang.Get("g.try");
            bt.SetBounds(146, 270, 120, 34);
            bt.Click += delegate { Guess(); };
            Controls.Add(bt);

            ModernButton bn = new ModernButton();
            bn.Text = Lang.Get("g.new");
            bn.SetBounds(276, 270, 128, 34);
            bn.Click += delegate { NewNumber(); };
            Controls.Add(bn);
            NewNumber();

            ModernButton close = new ModernButton();
            close.Text = "OK";
            close.SetBounds(304, 352, 100, 32);
            close.Click += delegate { Close(); };
            Controls.Add(close);
            CancelButton = close;
        }

        void PlayRps(int you)
        {
            int dino = rnd.Next(3);
            // 0 kamen, 1 papir, 2 makaze: papir > kamen, kamen > makaze, makaze > papir
            int diff = (you - dino + 3) % 3;   // 0 nerješeno, 1 ti, 2 dino
            string line = string.Format(Lang.Gender(Lang.Get("g.rps.picked")), RpsIcons[dino] + " " + Lang.Get("g." + RpsKeys[dino]));
            if (diff == 0) line += "  " + Lang.Get("g.draw");
            else if (diff == 1) { line += "  " + Lang.Gender(Lang.Get("g.youwin")); rpsYou++; }
            else { line += "  " + Lang.Gender(Lang.Get("g.dinowin")); rpsDino++; }
            lblRpsResult.Text = line;
            UpdateScore();
            if (Played != null) Played(diff == 1, diff == 0);
        }

        void UpdateScore()
        {
            lblRpsScore.Text = string.Format(Lang.Get("g.score"), rpsYou, rpsDino);
        }

        void NewNumber()
        {
            secret = 1 + rnd.Next(100);
            tries = 0;
            lblGuess.Text = Lang.Gender(Lang.Get("g.guess.start"));
            tbGuess.Text = "";
            try { tbGuess.Focus(); } catch { }
        }

        void Guess()
        {
            int n;
            if (secret < 0) { NewNumber(); return; }
            if (!int.TryParse(tbGuess.Text.Trim(), out n) || n < 1 || n > 100) { lblGuess.Text = Lang.Get("g.guess.bad"); return; }
            tries++;
            if (n < secret) lblGuess.Text = string.Format(Lang.Get("g.guess.up"), n);
            else if (n > secret) lblGuess.Text = string.Format(Lang.Get("g.guess.down"), n);
            else
            {
                lblGuess.Text = string.Format(Lang.Gender(Lang.Get("g.guess.ok")), secret, tries);
                if (Played != null) Played(tries <= 7, false);
                secret = -1;
            }
            tbGuess.SelectAll();
        }
    }
}

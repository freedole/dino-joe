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

        static readonly System.Text.RegularExpressions.Regex GenderRx =
            new System.Text.RegularExpressions.Regex(@"\{([^{}|]*)\|([^{}|]*)\}");

        /// "{muški|ženski}" u tekstu -> oblik po polu ljubimca (ODjordje / ODjordjina).
        public static string Gender(string s) { return GenderFor(s, Settings.Female); }

        public static string GenderFor(string s, bool f)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('{') < 0) return s;
            return GenderRx.Replace(s, delegate(System.Text.RegularExpressions.Match m) { return f ? m.Groups[2].Value : m.Groups[1].Value; });
        }

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
            Add("s.ai.ok", "✓ AI je {povezan|povezana}.", "✓ AI je {povezan|povezana}.", "✓ AI je {povezan|povezana}.", "✓ AI connected.");
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
            Add("pk.personality", "ličnost", "ličnost", "osobnost", "personality");
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
            Add("pk.loaded", "{Učitao|Učitala} sam {0} fora. {Spreman|Spremna}!", "{Učitao|Učitala} sam {0} fora. {Spreman|Spremna}!", "{Učitao|Učitala} sam {0} fora. {Spreman|Spremna}!", "Loaded {0} jokes. Ready!");
            Add("pk.like", "👍 Dobra fora!", "👍 Dobra fora!", "👍 Dobra fora!", "👍 Good one!");
            Add("pk.block", "👎 Ne prikazuj više ovu foru", "👎 Ne prikazuj više ovu foru", "👎 Ne prikazuj više ovu foru", "👎 Never show this joke again");
            Add("pk.liked", "Hvala! Pričaću je češće.", "Hvala! Pričaću je češće.", "Hvala! Pričat ću je češće.", "Thanks! I'll tell it more often.");
            Add("pk.blocked", "Dobro, ta fora je izumrla.", "Dobro, ta fora je izumrla.", "Dobro, ta fora je izumrla.", "Okay, that joke is now extinct.");
            Add("pk.profiles", "Profil", "Profil", "Profil", "Profile");
            Add("pk.switched", "Profil: {0}", "Profil: {0}", "Profil: {0}", "Profile: {0}");

            // blokirane fore
            Add("bl.title", "Blokirane fore", "Blokirane fore", "Blokirane fore", "Blocked jokes");
            Add("bl.sub", "Označi fore koje želiš vratiti:", "Označi fore koje želiš da vratiš:", "Označi fore koje želiš vratiti:", "Check the jokes you want back:");
            Add("bl.checkall", "Označi sve", "Označi sve", "Označi sve", "Check all");
            Add("bl.restore", "Vrati označene", "Vrati označene", "Vrati označene", "Restore checked");
            Add("bl.restoreall", "Vrati sve", "Vrati sve", "Vrati sve", "Restore all");
            Add("bl.unknown", "(stara blokada, tekst nije sačuvan)", "(stara blokada, tekst nije sačuvan)", "(stara blokada, tekst nije spremljen)", "(old block, text not saved)");

            // status i potrebe
            Add("need.food", "Hrana", "Hrana", "Hrana", "Food");
            Add("need.water", "Voda", "Voda", "Voda", "Water");
            Add("need.energy", "Energija", "Energija", "Energija", "Energy");
            Add("need.love", "Mazenje", "Maženje", "Maženje", "Cuddles");
            Add("need.fun", "Zabava", "Zabava", "Zabava", "Fun");
            Add("need.hygiene", "Higijena", "Higijena", "Higijena", "Hygiene");
            Add("need.health", "Zdravlje", "Zdravlje", "Zdravlje", "Health");
            Add("st.title", "status", "status", "status", "status");
            Add("st.menu", "Status...", "Status...", "Status...", "Status...");
            Add("st.happy", "Zadovoljstvo", "Zadovoljstvo", "Zadovoljstvo", "Happiness");
            Add("st.worry", "Briga", "Briga", "Briga", "Worry");
            Add("st.vacation", "Godišnji odmor (pauziraj potrebe)", "Godišnji odmor (pauziraj potrebe)", "Godišnji odmor (pauziraj potrebe)", "Vacation (pause needs)");
            Add("st.onvacation", "Na godišnjem odmoru 🏖", "Na godišnjem odmoru 🏖", "Na godišnjem odmoru 🏖", "On vacation 🏖");
            Add("st.sick", "Nije mu dobro 🤒", "Nije mu dobro 🤒", "Nije mu dobro 🤒", "Not feeling well 🤒");
            Add("act.feed", "Nahrani", "Nahrani", "Nahrani", "Feed");
            Add("act.water", "Napoji", "Napoji", "Napoji", "Give water");
            Add("act.pet", "Pomazi", "Pomazi", "Pomazi", "Pet");
            Add("act.play", "Igraj se", "Igraj se", "Igraj se", "Play");
            Add("act.sleep", "Uspavaj", "Uspavaj", "Uspavaj", "Sleep");
            Add("act.bath", "Okupaj", "Okupaj", "Okupaj", "Bath");
            Add("say.water", "Glup glup glup... Aaah!", "Glup glup glup... Aaah!", "Glup glup glup... Aaah!", "Glug glug glug... Aaah!");
            Add("say.play", "Hajmo! Ti juri meteor, ja bježim!", "Hajmo! Ti juri meteor, ja bežim!", "Idemo! Ti juri meteor, ja bježim!", "Let's go! You chase the meteor, I run!");
            Add("say.bath", "Blub blub! Sad mirišem kao fosil iz parfimerije.", "Blub blub! Sad mirišem kao fosil iz parfimerije.", "Blub blub! Sad mirišem kao fosil iz parfumerije.", "Splish splash! Now I smell like a fossil from a perfume shop.");
            Add("say.tired", "Pretvaram se da nisam {umoran|umorna}. Ne ide mi.", "Pretvaram se da nisam {umoran|umorna}. Ne ide mi.", "Pretvaram se da nisam {umoran|umorna}. Ne ide mi.", "I'm pretending I'm not tired. It's not working.");
            Add("need.say.food", "Ne kažem da sam {gladan|gladna}, ali onaj kursor liči na kobasicu.|{Gladan|Gladna} sam! (dupli klik)|Stomak mi krči kao stari hard disk.", "Ne kažem da sam {gladan|gladna}, ali onaj kursor liči na kobasicu.|{Gladan|Gladna} sam! (dupli klik)|Stomak mi krči kao stari hard disk.", "Ne kažem da sam {gladan|gladna}, ali onaj kursor sliči na kobasicu.|{Gladan|Gladna} sam! (dupli klik)|Trbuh mi krči kao stari hard disk.", "Not saying I'm hungry, but that cursor looks like a sausage.|I'm hungry! (double-click)|My stomach growls like an old hard drive.");
            Add("need.say.water", "{Žedan|Žedna} sam kao pustinja u Kredi.|Daj malo vode, suho mi je ko na serveru bez ventilatora.", "{Žedan|Žedna} sam kao pustinja u Kredi.|Daj malo vode, suvo mi je ko na serveru bez ventilatora.", "{Žedan|Žedna} sam kao pustinja u Kredi.|Daj malo vode, suho mi je ko na serveru bez ventilatora.", "I'm as thirsty as a Cretaceous desert.|Water please, I'm drier than a server without fans.");
            Add("need.say.energy", "Oči mi se zatvaraju brže nego tebi tabovi.|*zijev* Može li jedna dremka?", "Oči mi se zatvaraju brže nego tebi tabovi.|*zev* Može li jedna dremka?", "Oči mi se zatvaraju brže nego tebi tabovi.|*zijev* Može li jedno drijemanje?", "My eyes close faster than your tabs.|*yawn* Nap time?");
            Add("need.say.love", "Dobro. Niko me ne mazi. Idem da se mazim sa Recycle Binom.|Jesi zaboravio da postojim? Ja jesam fosil, ali ipak.", "Dobro. Niko me ne mazi. Idem da se mazim sa Recycle Binom.|Jesi zaboravio da postojim? Ja jesam fosil, ali ipak.", "Dobro. Nitko me ne mazi. Idem se maziti s Recycle Binom.|Jesi zaboravio da postojim? Ja jesam fosil, ali ipak.", "Fine. Nobody pets me. I'll cuddle the Recycle Bin.|Forgot I exist? I am a fossil, but still.");
            Add("need.say.fun", "Dosadno mi je. Hoćemo se igrati?|Ajde nešto zanimljivo, ovaj Excel me ubija.", "Dosadno mi je. Hoćemo da se igramo?|Ajde nešto zanimljivo, ovaj Excel me ubija.", "Dosadno mi je. Hoćemo se igrati?|Ajde nešto zanimljivo, ovaj Excel me ubija.", "I'm bored. Wanna play?|Something fun please, this Excel is killing me.");
            Add("need.say.hygiene", "Mirišem kao muzej poslije ponoći. Okupaj me.|Malo sam {blatnjav|blatnjava}. Od Krede.", "Mirišem kao muzej posle ponoći. Okupaj me.|Malo sam {blatnjav|blatnjava}. Od Krede.", "Mirišem kao muzej poslije ponoći. Okupaj me.|Malo sam {blatnjav|blatnjava}. Od Krede.", "I smell like a museum after midnight. Bath please.|I'm a bit muddy. Cretaceous mud.");
            Add("need.say.health", "Nije mi dobro... Trebam hranu, vodu i kupku.|Kašljem kao stari laptop.", "Nije mi dobro... Treba mi hrana, voda i kupka.|Kašljem kao stari laptop.", "Nije mi dobro... Trebam hranu, vodu i kupku.|Kašljem kao stari laptop.", "Not feeling well... I need food, water and a bath.|Coughing like an old laptop.");
            Add("say.ranaway", "{Otišao|Otišla} sam. Niko me ne pazi. (tray -> Dozovi ovamo)", "{Otišao|Otišla} sam. Niko me ne pazi. (tray -> Dozovi ovamo)", "{Otišao|Otišla} sam. Nitko me ne pazi. (tray -> Dozovi ovamo)", "I left. Nobody takes care of me. (tray -> Summon)");
            Add("say.back", "{Vratio|Vratila} sam se... Ali sad me pazi, važi?", "{Vratio|Vratila} sam se... Ali sad me pazi, važi?", "{Vratio|Vratila} sam se... Ali sad me pazi, može?", "I'm back... but take care of me now, okay?");

            // meni i govor ljubimca
            Add("m.chat", "Pričaj sa mnom", "Pričaj sa mnom", "Pričaj sa mnom", "Chat with me");
            Add("m.joke", "Ispričaj foru", "Ispričaj foru", "Ispričaj foru", "Tell a joke");
            Add("m.roar", "Riči!", "Riči!", "Riči!", "Roar!");
            Add("m.chick", "Pozovi pile", "Pozovi pile", "Pozovi pile", "Call the chick");
            Add("m.glasses", "Naočale", "Naočare", "Naočale", "Glasses");
            Add("m.sleep", "Spavaj", "Spavaj", "Spavaj", "Sleep");
            Add("m.wake", "Probudi", "Probudi", "Probudi", "Wake up");
            Add("m.summon", "Dozovi ovamo", "Dozovi ovamo", "Dozovi ovamo", "Come here");
            Add("m.size", "Veličina", "Veličina", "Veličina", "Size");
            Add("m.sizes", "Mali|Srednji|Veliki", "Mali|Srednji|Veliki", "Mali|Srednji|Veliki", "Small|Medium|Large");
            Add("m.talk", "Pričljivost", "Pričljivost", "Pričljivost", "Chattiness");
            Add("m.talks", "Tih|Normalan|Brbljivac", "Tih|Normalan|Brbljivac", "Tih|Normalan|Brbljavac", "Quiet|Normal|Chatterbox");
            Add("m.aiset", "Podesi AI...", "Podesi AI...", "Postavi AI...", "AI settings...");
            Add("m.persona", "Uredi AI ličnost (personality.txt)", "Uredi AI ličnost (personality.txt)", "Uredi AI osobnost (personality.txt)", "Edit AI persona (personality.txt)");
            Add("my.name", "Moje fore", "Moje fore", "Moje fore", "My jokes");
            Add("my.desc", "Fore koje sam sam napisao.", "Fore koje sam sam napisao.", "Fore koje sam sam napisao.", "Jokes I wrote myself.");
            Add("my.header", "# Tvoje fore, jedna po liniji. Linije sa # se ignorišu.\r\n# Svaki .txt u ovom folderu je jedna kategorija (ime fajla = kategorija).\r\n# Poslije izmjene: desni klik -> Fore -> Učitaj fore ponovo.\r\n", "# Tvoje fore, jedna po liniji. Linije sa # se ignorišu.\r\n# Svaki .txt u ovom folderu je jedna kategorija (ime fajla = kategorija).\r\n# Posle izmene: desni klik -> Fore -> Učitaj fore ponovo.\r\n", "# Tvoje fore, jedna po retku. Retci s # se ignoriraju.\r\n# Svaki .txt u ovoj mapi je jedna kategorija (ime datoteke = kategorija).\r\n# Nakon izmjene: desni klik -> Fore -> Učitaj fore ponovno.\r\n", "# Your jokes, one per line. Lines starting with # are ignored.\r\n# Each .txt in this folder is one category (file name = category).\r\n# After editing: right-click -> Jokes -> Reload jokes.\r\n");
            Add("m.clear", "Obriši razgovor", "Obriši razgovor", "Obriši razgovor", "Clear conversation");
            Add("s.cleared", "Razgovor obrisan. Ne sjećam se ničega. Kao poslije meteora.", "Razgovor obrisan. Ne sećam se ničega. Kao posle meteora.", "Razgovor obrisan. Ne sjećam se ničega. Kao poslije meteora.", "Conversation cleared. I remember nothing. Like after the meteor.");
            Add("s.forgot", "Šta smo pričali? Ne sjećam se.", "Šta smo pričali? Ne sećam se.", "Što smo pričali? Ne sjećam se.", "What were we talking about? I forgot.");
            Add("m.jokes", "Fore", "Fore", "Fore", "Jokes");
            Add("m.jokefolder", "Otvori folder sa mojim forama", "Otvori folder sa mojim forama", "Otvori mapu s mojim forama", "Open my jokes folder");
            Add("m.reload", "Učitaj fore ponovo", "Učitaj fore ponovo", "Učitaj fore ponovno", "Reload jokes");
            Add("m.exit", "Izlaz", "Izlaz", "Izlaz", "Exit");
            Add("s.noauto", "OK, neću se sam paliti.", "OK, neću se sam paliti.", "OK, neću se sam paliti.", "OK, I won't start on my own.");
            Add("s.auto", "Vidimo se pri svakom logonu!", "Vidimo se pri svakom logonu!", "Vidimo se pri svakoj prijavi!", "See you at every logon!");
            Add("s.write", "Piši, ja čekam.", "Piši, ja čekam.", "Piši, ja čekam.", "Write away, I'm waiting.");
            Add("s.njam", "NJAM! Konačno!", "NJAM! Konačno!", "NJAM! Konačno!", "NOM! Finally!");
            Add("s.full", "{Sit|Sita} sam, ali hvala :)", "{Sit|Sita} sam, ali hvala :)", "{Sit|Sita} sam, ali hvala :)", "I'm full, but thanks :)");
            Add("s.morning", "Jutro!", "Jutro!", "Jutro!", "Morning!");
            Add("s.night", "Laku noć...", "Laku noć...", "Laku noć...", "Good night...");
            Add("s.here", "Evo me!", "Evo me!", "Evo me!", "Here I am!");
            Add("s.aion", "{Povezan|Povezana} na AI ({0}). Enter šalje, Esc zatvara.", "{Povezan|Povezana} na AI ({0}). Enter šalje, Esc zatvara.", "{Povezan|Povezana} na AI ({0}). Enter šalje, Esc zatvara.", "Connected to AI ({0}). Enter sends, Esc closes.");
            Add("s.aioff", "Offline režim. Za pravi razgovor: desni klik na mene → AI → Podesi AI.", "Offline režim. Za pravi razgovor: desni klik na mene → AI → Podesi AI.", "Offline način. Za pravi razgovor: desni klik na mene → AI → Postavi AI.", "Offline mode. For real chat: right-click me → AI → AI settings.");
            Add("s.you", "Ti", "Ti", "Ti", "You");
            Add("s.badkey", "Ključ ne valja ili je opozvan. Desni klik → AI → Podesi AI.", "Ključ ne valja ili je opozvan. Desni klik → AI → Podesi AI.", "Ključ ne valja ili je opozvan. Desni klik → AI → Postavi AI.", "The key is invalid or revoked. Right-click → AI → AI settings.");
            Add("s.limit", "Limit dostignut. Odmaram od AI-ja 5 minuta, do tada pričam iz glave.", "Limit dostignut. Odmaram od AI-ja 5 minuta, do tada pričam iz glave.", "Limit dosegnut. Odmaram od AI-ja 5 minuta, do tada pričam iz glave.", "Rate limit reached. Taking a 5 minute AI break, I'll talk from memory.");
            Add("s.nonet", "Nema veze sa serverom (mreža/proxy/firewall). Pokušaću opet za minut.", "Nema veze sa serverom (mreža/proksi/firewall). Pokušaću opet za minut.", "Nema veze sa serverom (mreža/proxy/firewall). Pokušat ću opet za minutu.", "Can't reach the server (network/proxy/firewall). I'll retry in a minute.");
            Add("s.aierr", "AI greška.", "AI greška.", "AI pogreška.", "AI error.");
            Add("s.seechat", "… (vidi chat)", "… (vidi chat)", "… (vidi chat)", "… (see chat)");
            Add("s.connected", "{Povezan|Povezana} sam! Klikni me u tray-u ili desni klik → Pričaj sa mnom.", "{Povezan|Povezana} sam! Klikni me u tray-u ili desni klik → Pričaj sa mnom.", "{Povezan|Povezana} sam! Klikni me u tray-u ili desni klik → Pričaj sa mnom.", "Connected! Click me in the tray or right-click → Chat with me.");
            Add("s.aion2", "{Povezan|Povezana} na AI ({0}).", "{Povezan|Povezana} na AI ({0}).", "{Povezan|Povezana} na AI ({0}).", "Connected to AI ({0}).");
            Add("s.persona", "Mijenjaš mi karakter? Hrabro.", "Menjaš mi karakter? Hrabro.", "Mijenjaš mi karakter? Hrabro.", "Changing my personality? Brave.");
            Add("s.roarsorry", "ROAR! ...izvini, refleks.", "ROAR! ...izvini, refleks.", "ROAR! ...oprosti, refleks.", "ROAR! ...sorry, reflex.");
            Add("s.floor", "Opet ja na podu.", "Opet ja na podu.", "Opet ja na podu.", "On the floor again.");
            Add("s.yawn", "*zijev*", "*zev*", "*zijev*", "*yawn*");
            Add("s.notsleep", "Hm?! Ne spavam, odmaram oči.", "Hm?! Ne spavam, odmaram oči.", "Hm?! Ne spavam, odmaram oči.", "Hm?! I'm not sleeping, just resting my eyes.");
            Add("s.noglasses", "Bez naočala sam ljepši. Mislim.", "Bez naočara sam lepši. Mislim.", "Bez naočala sam ljepši. Mislim.", "I look better without glasses. I think.");
            Add("g.mon", "Ponedjeljak. {Izumro|Izumrla} bih opet.", "Ponedeljak. {Izumro|Izumrla} bih opet.", "Ponedjeljak. {Izumro|Izumrla} bih opet.", "Monday. I'd go extinct again.");
            Add("g.fri", "Petak je. Ne diraj ništa.", "Petak je. Ne diraj ništa.", "Petak je. Ne diraj ništa.", "It's Friday. Don't touch anything.");
            Add("g.morning", "Dobro jutro! Prvo kafa, pa sve ostalo.", "Dobro jutro! Prvo kafa, pa sve ostalo.", "Dobro jutro! Prvo kava, pa sve ostalo.", "Good morning! Coffee first, everything else later.");
            Add("g.day", "Ćao! Ja sam {0}.", "Ćao! Ja sam {0}.", "Bok! Ja sam {0}.", "Hi! I'm {0}.");
            Add("g.eve", "Dobro veče! Još radiš?", "Dobro veče! Još radiš?", "Dobra večer! Još radiš?", "Good evening! Still working?");
            Add("g.night", "Što ne spavaš? Ni ja ne spavam. Ja sam fosil.", "Što ne spavaš? Ni ja ne spavam. Ja sam fosil.", "Zašto ne spavaš? Ni ja ne spavam. Ja sam fosil.", "Why aren't you asleep? Me neither. I'm a fossil.");
            Add("ai.title", "{0} AI - podešavanje", "{0} AI - podešavanje", "{0} AI - postavke", "{0} AI - settings");
            Add("ai.intro", "1) Klikni \"Otvori Groq\", uloguj se i napravi API ključ (besplatno).\r\n2) Zalijepi ključ ispod i klikni \"Testiraj i sačuvaj\".", "1) Klikni \"Otvori Groq\", uloguj se i napravi API ključ (besplatno).\r\n2) Nalepi ključ ispod i klikni \"Testiraj i sačuvaj\".", "1) Klikni \"Otvori Groq\", prijavi se i napravi API ključ (besplatno).\r\n2) Zalijepi ključ ispod i klikni \"Testiraj i spremi\".", "1) Click \"Open Groq\", sign in and create an API key (free).\r\n2) Paste the key below and click \"Test and save\".");
            Add("ai.open", "Otvori Groq", "Otvori Groq", "Otvori Groq", "Open Groq");
            Add("ai.key", "API ključ:", "API ključ:", "API ključ:", "API key:");
            Add("ai.show", "Prikaži ključ", "Prikaži ključ", "Prikaži ključ", "Show key");
            Add("ai.enabled", "AI uključen (isključeno = offline fore)", "AI uključen (isključeno = offline fore)", "AI uključen (isključeno = offline fore)", "AI enabled (off = offline jokes)");
            Add("ai.test", "Testiraj i sačuvaj", "Testiraj i sačuvaj", "Testiraj i spremi", "Test and save");
            Add("ai.save", "Sačuvaj bez testa", "Sačuvaj bez testa", "Spremi bez testa", "Save without test");
            Add("ai.enter", "Unesi ključ.", "Unesi ključ.", "Unesi ključ.", "Enter a key.");
            Add("ai.testing", "Testiram vezu...", "Testiram vezu...", "Testiram vezu...", "Testing connection...");
            Add("ai.works", "Radi! {0} kaže: {1}", "Radi! {0} kaže: {1}", "Radi! {0} kaže: {1}", "Works! {0} says: {1}");
            Add("ai.fail", "Ne radi: {0}", "Ne radi: {0}", "Ne radi: {0}", "Doesn't work: {0}");
            Add("ai.chattitle", "Pričaj sa {0}", "Pričaj sa {0}", "Pričaj s {0}", "Chat with {0}");
            Add("ai.send", "Pošalji", "Pošalji", "Pošalji", "Send");
            Add("ai.empty", "Prazan odgovor od servera.", "Prazan odgovor od servera.", "Prazan odgovor poslužitelja.", "Empty response from server.");

            Add("s.error", "Greška: ", "Greška: ", "Pogreška: ", "Error: ");
            Add("g.noon", "Podne! Pauza? Može sve osim piletine.", "Podne! Pauza? Može sve osim piletine.", "Podne! Pauza? Može sve osim piletine.", "Noon! Lunch break? Anything but chicken.");
            Add("g.endwork", "15h. Ko je zadnji, gasi svjetlo.", "15h. Ko je zadnji, gasi svetlo.", "15h. Tko je zadnji, gasi svjetlo.", "3 PM. Last one out turns off the lights.");
            Add("ai.think", "Model je samo razmišljao, bez odgovora (povećaj max_tokens ili promijeni model).", "Model je samo razmišljao, bez odgovora (povećaj max_tokens ili promeni model).", "Model je samo razmišljao, bez odgovora (povećaj max_tokens ili promijeni model).", "The model only thought, no answer (raise max_tokens or change model).");
            Add("ai.greetme", "Pozdravi me u jednoj kratkoj rečenici.", "Pozdravi me u jednoj kratkoj rečenici.", "Pozdravi me u jednoj kratkoj rečenici.", "Greet me in one short sentence.");

            // trigger fore (više varijanti odvojeno sa |)
            Add("trg.cpu", "Šta to pokrećeš? Vruće je kao pred meteor.|Procesor ti radi kao da bježi od meteora.|CPU na maksimumu. Da imam znojne žlijezde, sad bih se {oznojio|oznojila}.", "Šta to pokrećeš? Vruće je kao pred meteor.|Procesor ti radi kao da beži od meteora.|CPU na maksimumu. Da imam znojne žlezde, sad bih se {oznojio|oznojila}.", "Što to pokrećeš? Vruće je kao pred meteor.|Procesor ti radi kao da bježi od meteora.|CPU na maksimumu. Da imam znojne žlijezde, sad bih se {oznojio|oznojila}.", "What are you running? It's hot like before the meteor.|Your CPU is running like it's escaping a meteor.|CPU maxed out. If I had sweat glands, I'd be sweating.");
            Add("trg.battery", "Baterija pri kraju. I ja sam tako {izumro|izumrla}: polako, pa odjednom.|Punjač! Brzo! Ovo nije vježba.", "Baterija pri kraju. I ja sam tako {izumro|izumrla}: polako, pa odjednom.|Punjač! Brzo! Ovo nije vežba.", "Baterija pri kraju. I ja sam tako {izumro|izumrla}: polako, pa odjednom.|Punjač! Brzo! Ovo nije vježba.", "Battery almost empty. That's how I went extinct: slowly, then all at once.|Charger! Quick! This is not a drill.");
            Add("trg.midnight", "Ponoć je. Ništa pametno nije napisano poslije ponoći. Osim ove fore.|Ponoć. Vrijeme za spavanje, a ne za još jedan tab.", "Ponoć je. Ništa pametno nije napisano posle ponoći. Osim ove fore.|Ponoć. Vreme za spavanje, a ne za još jedan tab.", "Ponoć je. Ništa pametno nije napisano poslije ponoći. Osim ove fore.|Ponoć. Vrijeme za spavanje, a ne za još jedan tab.", "Midnight. Nothing smart was ever written after midnight. Except this joke.|Midnight. Time to sleep, not for one more tab.");
            Add("trg.monday", "Ponedjeljak, 8h. Kafa prvo, pitanja poslije.|Ponedjeljak ujutro. Hrabro, izdržaćemo zajedno.", "Ponedeljak, 8h. Kafa prvo, pitanja posle.|Ponedeljak ujutru. Hrabro, izdržaćemo zajedno.", "Ponedjeljak, 8h. Kava prvo, pitanja poslije.|Ponedjeljak ujutro. Hrabro, izdržat ćemo zajedno.", "Monday, 8 AM. Coffee first, questions later.|Monday morning. Be brave, we'll survive together.");
            Add("trg.back", "Jesi živ? Već sam jednom {ostao|ostala} sam, ne bih opet.|E, {vratio|vratila} si se! {Mislio|Mislila} sam da te je meteor odnio.|Dugo te nije bilo. {Čuvao|Čuvala} sam desktop.", "Jesi živ? Već sam jednom {ostao|ostala} sam, ne bih opet.|E, {vratio|vratila} si se! {Mislio|Mislila} sam da te je meteor odneo.|Dugo te nije bilo. {Čuvao|Čuvala} sam desktop.", "Jesi živ? Već sam jednom {ostao|ostala} sam, ne bih opet.|E, {vratio|vratila} si se! {Mislio|Mislila} sam da te je meteor odnio.|Dugo te nije bilo. {Čuvao|Čuvala} sam radnu površinu.", "Are you alive? I've been left alone once, not again.|Hey, you're back! I thought the meteor got you.|Long time no see. I guarded the desktop.");

            // AI servisi
            Add("ai.provider", "AI servis:", "AI servis:", "AI servis:", "AI service:");
            Add("ai.model", "Model:", "Model:", "Model:", "Model:");
            Add("ai.getkey", "Napravi API ključ ↗", "Napravi API ključ ↗", "Napravi API ključ ↗", "Get an API key ↗");
            Add("ai.getollama", "Preuzmi Ollama ↗", "Preuzmi Ollama ↗", "Preuzmi Ollama ↗", "Download Ollama ↗");
            Add("aip.groq", "Groq (besplatno)", "Groq (besplatno)", "Groq (besplatno)", "Groq (free)");
            Add("aip.groq.info", "Besplatno, vrlo brzo. Napravi ključ, zalijepi ga ispod i to je to.", "Besplatno, vrlo brzo. Napravi ključ, nalepi ga ispod i to je to.", "Besplatno, vrlo brzo. Napravi ključ, zalijepi ga ispod i to je to.", "Free and very fast. Create a key, paste it below, done.");
            Add("aip.gemini", "Google Gemini (besplatni nivo)", "Google Gemini (besplatni nivo)", "Google Gemini (besplatna razina)", "Google Gemini (free tier)");
            Add("aip.gemini.info", "Besplatan nivo uz Google nalog (Google AI Studio).", "Besplatan nivo uz Google nalog (Google AI Studio).", "Besplatna razina uz Google račun (Google AI Studio).", "Free tier with a Google account (Google AI Studio).");
            Add("aip.openai", "OpenAI (ChatGPT)", "OpenAI (ChatGPT)", "OpenAI (ChatGPT)", "OpenAI (ChatGPT)");
            Add("aip.openai.info", "Plaća se po potrošnji (API nalog nije isto što i ChatGPT pretplata).", "Plaća se po potrošnji (API nalog nije isto što i ChatGPT pretplata).", "Plaća se po potrošnji (API račun nije isto što i ChatGPT pretplata).", "Pay as you go (an API account is not the same as a ChatGPT subscription).");
            Add("aip.claude", "Anthropic Claude", "Anthropic Claude", "Anthropic Claude", "Anthropic Claude");
            Add("aip.claude.info", "Plaća se po potrošnji. Ključ se pravi u Anthropic konzoli.", "Plaća se po potrošnji. Ključ se pravi u Anthropic konzoli.", "Plaća se po potrošnji. Ključ se izrađuje u Anthropic konzoli.", "Pay as you go. Create the key in the Anthropic console.");
            Add("aip.openrouter", "OpenRouter (stotine modela)", "OpenRouter (stotine modela)", "OpenRouter (stotine modela)", "OpenRouter (hundreds of models)");
            Add("aip.openrouter.info", "Jedan ključ za mnogo modela, neki su besplatni (završavaju na :free).", "Jedan ključ za mnogo modela, neki su besplatni (završavaju na :free).", "Jedan ključ za mnogo modela, neki su besplatni (završavaju na :free).", "One key for many models, some are free (ending in :free).");
            Add("aip.ollama", "Ollama (lokalno, bez interneta)", "Ollama (lokalno, bez interneta)", "Ollama (lokalno, bez interneta)", "Ollama (local, offline)");
            Add("aip.ollama.info", "AI radi na tvom računaru, bez ključa i bez slanja podataka. Treba instalirati Ollama i model.", "AI radi na tvom računaru, bez ključa i bez slanja podataka. Treba instalirati Ollama i model.", "AI radi na tvom računalu, bez ključa i bez slanja podataka. Treba instalirati Ollama i model.", "AI runs on your computer, no key and no data leaves it. Install Ollama and a model.");
            Add("aip.custom", "Drugo (OpenAI-kompatibilno)", "Drugo (OpenAI-kompatibilno)", "Drugo (OpenAI-kompatibilno)", "Other (OpenAI-compatible)");
            Add("aip.custom.info", "Bilo koji servis sa OpenAI-kompatibilnim /chat/completions endpointom.", "Bilo koji servis sa OpenAI-kompatibilnim /chat/completions endpointom.", "Bilo koji servis s OpenAI-kompatibilnim /chat/completions endpointom.", "Any service with an OpenAI-compatible /chat/completions endpoint.");

            // šare i dodaci
            Add("s.look.pattern", "Šara:", "Šara:", "Uzorak:", "Pattern:");
            Add("s.look.accessory", "Dodatak:", "Dodatak:", "Dodatak:", "Accessory:");
            Add("pat.none", "Bez šare", "Bez šare", "Bez uzorka", "None");
            Add("pat.stripes", "Pruge", "Pruge", "Pruge", "Stripes");
            Add("pat.dots", "Tačkice", "Tačkice", "Točkice", "Dots");
            Add("pat.gradient", "Prelaz boja", "Prelaz boja", "Prijelaz boja", "Gradient");
            Add("pat.camo", "Kamuflaža", "Kamuflaža", "Kamuflaža", "Camo");
            Add("pat.galaxy", "Galaksija", "Galaksija", "Galaksija", "Galaxy");
            Add("acc.none", "Bez dodatka", "Bez dodatka", "Bez dodatka", "None");
            Add("acc.beanie", "Zimska kapa", "Zimska kapa", "Zimska kapa", "Beanie");
            Add("acc.tophat", "Cilindar", "Cilindar", "Cilindar", "Top hat");
            Add("acc.tie", "Kravata", "Kravata", "Kravata", "Tie");
            Add("acc.headphones", "Slušalice", "Slušalice", "Slušalice", "Headphones");
            Add("acc.crown", "Krunica", "Krunica", "Krunica", "Crown");
            Add("acc.helmet", "Motoristička kaciga", "Motoristička kaciga", "Motoristička kaciga", "Motorcycle helmet");
            Add("acc.scarf", "Šal", "Šal", "Šal", "Scarf");
            Add("acc.flower", "Cvijet", "Cvet", "Cvijet", "Flower");
            Add("acc.mustache", "Brkovi", "Brkovi", "Brkovi", "Mustache");
            Add("acc.bowtie", "Leptir-mašna", "Leptir-mašna", "Leptir-mašna", "Bow tie");

            // katalog
            Add("cat.menu", "Preuzmi pakete sa interneta...", "Preuzmi pakete sa interneta...", "Preuzmi pakete s interneta...", "Download packs...");
            Add("cat.title", "Katalog paketa", "Katalog paketa", "Katalog paketa", "Pack catalog");
            Add("cat.sub", "Paketi zajednice sa GitHub-a (freedole/dino-joe-packs). Dupli klik ili Preuzmi.", "Paketi zajednice sa GitHub-a (freedole/dino-joe-packs). Dupli klik ili Preuzmi.", "Paketi zajednice s GitHuba (freedole/dino-joe-packs). Dvostruki klik ili Preuzmi.", "Community packs from GitHub (freedole/dino-joe-packs). Double-click or Download.");
            Add("cat.col.state", "Stanje", "Stanje", "Stanje", "Status");
            Add("cat.alllangs", "Prikaži pakete na svim jezicima", "Prikaži pakete na svim jezicima", "Prikaži pakete na svim jezicima", "Show packs in all languages");
            Add("cat.install", "Preuzmi", "Preuzmi", "Preuzmi", "Download");
            Add("cat.loading", "Učitavam katalog...", "Učitavam katalog...", "Učitavam katalog...", "Loading catalog...");
            Add("cat.count", "Paketa u katalogu: {0}", "Paketa u katalogu: {0}", "Paketa u katalogu: {0}", "Packs in catalog: {0}");
            Add("cat.state.new", "nije preuzet", "nije preuzet", "nije preuzet", "not installed");
            Add("cat.state.update", "nova verzija (imaš {0})", "nova verzija (imaš {0})", "nova verzija (imaš {0})", "update available (you have {0})");
            Add("cat.state.installed", "✓ instaliran", "✓ instaliran", "✓ instaliran", "✓ installed");
            Add("cat.installing", "Preuzimam: {0}...", "Preuzimam: {0}...", "Preuzimam: {0}...", "Downloading: {0}...");
            Add("cat.installed", "✓ Preuzeto: {0}", "✓ Preuzeto: {0}", "✓ Preuzeto: {0}", "✓ Downloaded: {0}");
            Add("cat.err.net", "Katalog nije dostupan (internet/proxy/firewall).", "Katalog nije dostupan (internet/proksi/firewall).", "Katalog nije dostupan (internet/proxy/firewall).", "Catalog unavailable (internet/proxy/firewall).");
            Add("cat.err.hash", "Provjera fajla nije prošla (SHA256):", "Provera fajla nije prošla (SHA256):", "Provjera datoteke nije prošla (SHA256):", "File check failed (SHA256):");

            // profili i ličnosti
            Add("prof.default", "Osnovni", "Osnovni", "Osnovni", "Default");
            Add("pers.hide", "Sakrij ovu ličnost", "Sakrij ovu ličnost", "Sakrij ovu osobnost", "Hide this personality");
            Add("pers.show", "Prikaži ovu ličnost", "Prikaži ovu ličnost", "Prikaži ovu osobnost", "Show this personality");
            Add("pers.showhidden", "Prikaži sakrivene ličnosti (desni klik na ličnost = sakrij)", "Prikaži sakrivene ličnosti (desni klik na ličnost = sakrij)", "Prikaži skrivene osobnosti (desni klik na osobnost = sakrij)", "Show hidden personalities (right-click a personality to hide it)");
            Add("pers.hiddenmark", "(sakrivena)", "(sakrivena)", "(skrivena)", "(hidden)");

            // bosanski izuzeci
            BsOverride["wiz.settings"] = "Dino Joe - postavke";
            BsOverride["menu.settings"] = "Postavke...";
            BsOverride["m.aiset"] = "AI postavke...";
            BsOverride["ai.title"] = "{0} AI - postavke";
            BsOverride["aip.gemini.info"] = "Besplatan nivo uz Google račun (Google AI Studio).";
            BsOverride["aip.openai.info"] = "Plaća se po potrošnji (API račun nije isto što i ChatGPT pretplata).";
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
                "{Gladan|Gladna}. Mazi. Sad. ...Hvala. Opet {gladan|gladna}.",
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
        public static int Species;          // 0 T-Rex, 1 Triceratops, 2 Brontosaurus, 3 Raptor
        public static int Pattern;          // Skins.PatternKeys
        public static int Accessory;        // Skins.AccessoryKeys
        public static string Packs = "odjordje-classic,dino-joe-basic-sr";   // id-ovi uključenih paketa, zarezom
        public static string PackWeights = "";              // id:2,id2:0.5 (1 = normalno)
        public static string ActiveProfile = "";
        public static string HiddenPersonalities = "";      // id-ovi sakrivenih ugrađenih ličnosti

        public static bool IsHidden(string id) { return ("," + HiddenPersonalities + ",").Contains("," + id + ","); }

        public static void SetHidden(string id, bool hide)
        {
            List<string> l = new List<string>();
            foreach (string s in HiddenPersonalities.Split(',')) if (s.Length > 0 && s != id) l.Add(s);
            if (hide) l.Add(id);
            HiddenPersonalities = string.Join(",", l.ToArray());
            Save();
        }

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
                        else if (k == "species" && int.TryParse(v, out n)) Species = Math.Max(0, Math.Min(DinoJoe.Species.Count - 1, n));
                        else if (k == "pattern" && int.TryParse(v, out n)) Pattern = Math.Max(0, Math.Min(Skins.PatternCount - 1, n));
                        else if (k == "accessory" && int.TryParse(v, out n)) Accessory = Math.Max(0, Math.Min(Skins.AccessoryCount - 1, n));
                        else if (k == "packs") Packs = v;
                        else if (k == "pack_weights") PackWeights = v;
                        else if (k == "profile") ActiveProfile = v;
                        else if (k == "hidden_personalities") HiddenPersonalities = v;
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
                    "species=" + Species + "\r\n" +
                    "pattern=" + Pattern + "\r\n" +
                    "accessory=" + Accessory + "\r\n" +
                    "packs=" + Packs + "\r\n" +
                    "pack_weights=" + PackWeights + "\r\n" +
                    "hidden_personalities=" + HiddenPersonalities + "\r\n" +
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
                string pn = DinoJoe.Packs.ActiveValue("pet_name");
                if (pn != null) return pn;
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
                if (DinoJoe.Packs.ActiveValue("pet_name") != null) return DinoJoe.Packs.ActiveValue("pet_name_with") ?? DisplayName;
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

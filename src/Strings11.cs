// Dino Joe - prevodi za v1.1 (evolucija, dostignuća, nove vrste i šare).
// Redoslijed: srpski ijekavica, srpski ekavica, hrvatski, engleski (bosanski = ijekavica + BsOverride).
// C# 5 kompatibilno.

namespace DinoJoe
{
    static class Strings11
    {
        static void A(string k, string ijek, string ekav, string hr, string en) { Lang.Add(k, ijek, ekav, hr, en); }

        public static void Add()
        {
            // faze
            A("life.stage", "Faza:", "Faza:", "Faza:", "Stage:");
            A("life.stage0", "Jaje", "Jaje", "Jaje", "Egg");
            A("life.stage1", "Beba", "Beba", "Beba", "Baby");
            A("life.stage2", "Mladunče", "Mladunče", "Mladunče", "Juvenile");
            A("life.stage3", "Odrasli", "Odrasli", "Odrasli", "Adult");
            A("life.next", "{0}: {1}% do sljedeće faze", "{0}: {1}% do sledeće faze", "{0}: {1}% do sljedeće faze", "{0}: {1}% to next stage");
            A("life.egg", "Jaje: {0}% zagrijano", "Jaje: {0}% zagrejano", "Jaje: {0}% zagrijano", "Egg: {0}% warm");

            // jaje
            A("egg.hello", "Psst... ja sam jaje. Grij me: klikni me, pomazi me ili drži miša iznad mene.",
                           "Psst... ja sam jaje. Grej me: klikni me, pomazi me ili drži miša iznad mene.",
                           "Psst... ja sam jaje. Grij me: klikni me, pomazi me ili drži miš iznad mene.",
                           "Psst... I'm an egg. Keep me warm: click me, pet me or hover your mouse over me.");
            A("egg.click", "Kuc kuc...|Toplije!|Još malo...|Mmm, toplo.|Neko je tu?|Ššš, rastem.",
                           "Kuc kuc...|Toplije!|Još malo...|Mmm, toplo.|Neko je tu?|Ššš, rastem.",
                           "Kuc kuc...|Toplije!|Još malo...|Mmm, toplo.|Netko je tu?|Pssst, rastem.",
                           "Knock knock...|Warmer!|A little more...|Mmm, cozy.|Anyone there?|Shh, I'm growing.");
            A("egg.crack", "Krc!", "Krc!", "Krc!", "Crack!");
            A("egg.notyet", "Prvo me izlegni! Grij me klikom.", "Prvo me izlegni! Grej me klikom.", "Prvo me izlegni! Grij me klikom.", "Hatch me first! Click me to keep me warm.");
            A("egg.hatched", "Zdravo svijete! Ja sam {0}. Ti si moja mama? Ili tata?",
                             "Zdravo svete! Ja sam {0}. Ti si moja mama? Ili tata?",
                             "Bok svijete! Ja sam {0}. Ti si moja mama? Ili tata?",
                             "Hello world! I'm {0}. Are you my mom? Or dad?");
            A("life.grew1", "Vidi me! Više nisam beba, sad sam mladunče!", "Vidi me! Više nisam beba, sad sam mladunče!", "Vidi me! Više nisam beba, sad sam mladunče!", "Look at me! I'm not a baby anymore!");
            A("life.grew2", "{Porastao|Porasla} sam! Sad sam {pravi odrasli dino|prava odrasla dinosaurka}.",
                            "{Porastao|Porasla} sam! Sad sam {pravi odrasli dino|prava odrasla dinosaurka}.",
                            "{Narastao|Narasla} sam! Sad sam {pravi odrasli dino|prava odrasla dinosaurica}.",
                            "I grew up! Now I'm a real grown-up dino.");
            A("m.newegg", "Novo jaje...", "Novo jaje...", "Novo jaje...", "New egg...");
            A("newegg.confirm", "Početi ispočetka sa novim jajetom?\r\nIzgled, ime, ličnost i dostignuća ostaju.",
                                "Početi ispočetka sa novim jajetom?\r\nIzgled, ime, ličnost i dostignuća ostaju.",
                                "Početi ispočetka s novim jajetom?\r\nIzgled, ime, osobnost i postignuća ostaju.",
                                "Start over with a new egg?\r\nLook, name, personality and achievements are kept.");

            // dostignuća - opšte
            A("ach.title", "Dostignuća", "Dostignuća", "Postignuća", "Achievements");
            A("m.achievements", "Dostignuća...", "Dostignuća...", "Postignuća...", "Achievements...");
            A("ach.progress", "Otključano: {0} od {1}", "Otključano: {0} od {1}", "Otključano: {0} od {1}", "Unlocked: {0} of {1}");
            A("ach.got", "★ Dostignuće: {0}!", "★ Dostignuće: {0}!", "★ Postignuće: {0}!", "★ Achievement: {0}!");
            A("ach.gotreward", "★ Dostignuće: {0}! Otključano: {1}.", "★ Dostignuće: {0}! Otključano: {1}.", "★ Postignuće: {0}! Otključano: {1}.", "★ Achievement: {0}! Unlocked: {1}.");
            A("ach.reward", "Nagrada: {0}.", "Nagrada: {0}.", "Nagrada: {0}.", "Reward: {0}.");
            A("ach.howto", "Otključava se dostignućem: {0}", "Otključava se dostignućem: {0}", "Otključava se postignućem: {0}", "Unlocked by achievement: {0}");
            A("s.look.locked", "🔒", "🔒", "🔒", "🔒");

            // dostignuća - lista
            A("ach.hatch", "Izleglo se!", "Izleglo se!", "Izleglo se!", "It hatched!");
            A("ach.hatch.d", "Izlegni dinu iz jajeta.", "Izlegni dinu iz jajeta.", "Izlegni dina iz jajeta.", "Hatch a dino from its egg.");
            A("ach.juvenile", "Raste kao iz vode", "Raste kao iz vode", "Raste kao iz vode", "Growing fast");
            A("ach.juvenile.d", "Beba je postala mladunče.", "Beba je postala mladunče.", "Beba je postala mladunče.", "Your baby became a juvenile.");
            A("ach.adult", "Veliki dino", "Veliki dino", "Veliki dino", "All grown up");
            A("ach.adult.d", "Odgajaj dinu do odraslog.", "Odgajaj dinu do odraslog.", "Odgoji dina do odraslog.", "Raise your dino to adulthood.");
            A("ach.veteran", "Veteran", "Veteran", "Veteran", "Veteran");
            A("ach.veteran.d", "Bio si tu od verzije 1.0. Hvala!", "Bio si tu od verzije 1.0. Hvala!", "Bio si tu od verzije 1.0. Hvala!", "You were here since version 1.0. Thanks!");
            A("ach.days7", "Sedmica zajedno", "Sedmica zajedno", "Tjedan zajedno", "A week together");
            A("ach.days7.d", "Druži se sa dinom 7 različitih dana.", "Druži se sa dinom 7 različitih dana.", "Druži se s dinom 7 različitih dana.", "Hang out with your dino on 7 different days.");
            A("ach.days30", "Mjesec dana", "Mesec dana", "Mjesec dana", "A month together");
            A("ach.days30.d", "Druži se sa dinom 30 različitih dana.", "Druži se sa dinom 30 različitih dana.", "Druži se s dinom 30 različitih dana.", "Hang out with your dino on 30 different days.");
            A("ach.jokes300", "Najbolja publika", "Najbolja publika", "Najbolja publika", "Best audience");
            A("ach.jokes300.d", "Saslušaj 300 fora.", "Saslušaj 300 fora.", "Poslušaj 300 fora.", "Listen to 300 jokes.");
            A("ach.feed50", "Glavni kuhar", "Glavni kuvar", "Glavni kuhar", "Head chef");
            A("ach.feed50.d", "Nahrani dinu 50 puta.", "Nahrani dinu 50 puta.", "Nahrani dina 50 puta.", "Feed your dino 50 times.");
            A("ach.pet100", "Mazilica", "Mazilica", "Mazilica", "Cuddle champion");
            A("ach.pet100.d", "Pomazi dinu 100 puta.", "Pomazi dinu 100 puta.", "Pomazi dina 100 puta.", "Pet your dino 100 times.");
            A("ach.like10", "Kritičar", "Kritičar", "Kritičar", "Critic");
            A("ach.like10.d", "Označi 10 fora sa 👍.", "Označi 10 fora sa 👍.", "Označi 10 fora s 👍.", "Give 👍 to 10 jokes.");
            A("ach.night", "Noćna ptica", "Noćna ptica", "Noćna ptica", "Night owl");
            A("ach.night.d", "Budi uz dinu u ponoć.", "Budi uz dinu u ponoć.", "Budi uz dina u ponoć.", "Be with your dino at midnight.");
            A("ach.monday", "Preživjeli ponedjeljak", "Preživeli ponedeljak", "Preživjeli ponedjeljak", "Monday survivor");
            A("ach.monday.d", "Ponedjeljak u 8 ujutru, zajedno.", "Ponedeljak u 8 ujutru, zajedno.", "Ponedjeljak u 8 ujutro, zajedno.", "Monday 8 AM, together.");
            A("ach.ai", "Pričalica", "Pričalica", "Pričalica", "Chatterbox");
            A("ach.ai.d", "Prvi razgovor preko AI.", "Prvi razgovor preko AI.", "Prvi razgovor preko AI.", "Your first AI chat.");
            A("ach.pack", "Kolekcionar", "Kolekcionar", "Kolekcionar", "Collector");
            A("ach.pack.d", "Preuzmi paket fora iz kataloga.", "Preuzmi paket fora iz kataloga.", "Preuzmi paket fora iz kataloga.", "Download a joke pack from the catalog.");
            A("ach.vacation", "Zaslužen odmor", "Zaslužen odmor", "Zaslužen odmor", "Well-deserved rest");
            A("ach.vacation.d", "Pošalji dinu na godišnji odmor.", "Pošalji dinu na godišnji odmor.", "Pošalji dina na godišnji odmor.", "Send your dino on vacation.");
            A("ach.happy", "Najsrećniji dino", "Najsrećniji dino", "Najsretniji dino", "Happiest dino");
            A("ach.happy.d", "Zadovoljstvo 95% ili više.", "Zadovoljstvo 95% ili više.", "Zadovoljstvo 95% ili više.", "Reach 95% happiness.");
            A("ach.stylist", "Stilista", "Stilista", "Stilist", "Stylist");
            A("ach.stylist.d", "Promijeni izgled u podešavanjima.", "Promeni izgled u podešavanjima.", "Promijeni izgled u postavkama.", "Change the look in settings.");
            A("ach.newegg", "Novo jaje", "Novo jaje", "Novo jaje", "New egg");
            A("ach.newegg.d", "Izlegni i drugog dinu.", "Izlegni i drugog dinu.", "Izlegni i drugog dina.", "Hatch a second dino.");

            // Microsoft Store
            A("s.autostart.store", "Pokretanje sa Windowsom: Postavke > Aplikacije > Pokretanje (Dino Joe).",
                                   "Pokretanje sa Windowsom: Podešavanja > Aplikacije > Pokretanje (Dino Joe).",
                                   "Pokretanje s Windowsima: Postavke > Aplikacije > Pokretanje (Dino Joe).",
                                   "Start with Windows: Settings > Apps > Startup (Dino Joe).");

            // nove šare
            A("pat.gold", "Zlatna", "Zlatna", "Zlatna", "Gold");
            Lang.BsOverride["ach.stylist.d"] = "Promijeni izgled u postavkama.";
            Lang.BsOverride["ach.days7.d"] = "Druži se s dinom 7 različitih dana.";
            Lang.BsOverride["ach.days30.d"] = "Druži se s dinom 30 različitih dana.";
            A("pat.rainbow", "Duga", "Duga", "Duga", "Rainbow");
        }
    }
}

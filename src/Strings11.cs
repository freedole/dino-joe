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
            A("ach.like10.d", "Označi 10 fora kao \"Dobra fora!\".", "Označi 10 fora kao \"Dobra fora!\".", "Označi 10 fora kao \"Dobra fora!\".", "Mark 10 jokes as \"Good one!\".");
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

            // log grešaka
            A("err.bubble", "Ups, nešto me zaboljelo. Desni klik -> Prijavi problem...", "Ups, nešto me zabolelo. Desni klik -> Prijavi problem...", "Ups, nešto me zaboljelo. Desni klik -> Prijavi problem...", "Oops, something hurt. Right-click -> Report a problem...");
            A("m.report", "Prijavi problem...", "Prijavi problem...", "Prijavi problem...", "Report a problem...");

            // "Samo ovaj paket"
            A("pk.solo", "Samo ovaj paket", "Samo ovaj paket", "Samo ovaj paket", "Only this pack");
            A("pk.solo.all", "Svi uključeni paketi", "Svi uključeni paketi", "Svi uključeni paketi", "All enabled packs");
            A("pk.solo.on", "Sad pričam samo fore iz paketa: {0}.", "Sad pričam samo fore iz paketa: {0}.", "Sada pričam samo fore iz paketa: {0}.", "Now I only tell jokes from: {0}.");
            A("pk.solo.off", "Opet pričam fore iz svih uključenih paketa.", "Opet pričam fore iz svih uključenih paketa.", "Opet pričam fore iz svih uključenih paketa.", "Back to jokes from all enabled packs.");

            // fizika, igre
            A("ach.fly", "Leteći dino", "Leteći dino", "Leteći dino", "Flying dino");
            A("ach.fly.d", "Baci dinu mišem.", "Baci dinu mišem.", "Baci dina mišem.", "Throw your dino with the mouse.");
            A("ach.climber", "Penjač", "Penjač", "Penjač", "Climber");
            A("ach.climber.d", "Spusti dinu na vrh nekog prozora.", "Spusti dinu na vrh nekog prozora.", "Spusti dina na vrh nekog prozora.", "Drop your dino on top of a window.");
            A("ach.games5", "Pobjednik", "Pobednik", "Pobjednik", "Winner");
            A("ach.games5.d", "Pobijedi dinu 5 puta u igrama.", "Pobedi dinu 5 puta u igrama.", "Pobijedi dina 5 puta u igrama.", "Beat your dino 5 times in games.");
            A("m.windowwalk", "Šetaj po prozorima", "Šetaj po prozorima", "Šeći po prozorima", "Walk on windows");

            // mini igre
            A("m.games", "Igre...", "Igre...", "Igre...", "Games...");
            A("g.title", "Igre", "Igre", "Igre", "Games");
            A("g.rps", "Kamen, papir, makaze", "Kamen, papir, makaze", "Kamen, škare, papir", "Rock, paper, scissors");
            A("g.rock", "Kamen", "Kamen", "Kamen", "Rock");
            A("g.paper", "Papir", "Papir", "Papir", "Paper");
            A("g.scissors", "Makaze", "Makaze", "Škare", "Scissors");
            A("g.rps.picked", "Dino je {izabrao|izabrala}: {0}.", "Dino je {izabrao|izabrala}: {0}.", "Dino je {odabrao|odabrala}: {0}.", "Dino picked: {0}.");
            A("g.draw", "Nerješeno!", "Nerešeno!", "Neriješeno!", "Draw!");
            A("g.youwin", "Pobijedio si! Ajde još jednom.", "Pobedio si! Ajde još jednom.", "Pobijedio si! Hajde još jednom.", "You win! One more?");
            A("g.dinowin", "Dino pobjeđuje! ROAR!", "Dino pobeđuje! ROAR!", "Dino pobjeđuje! ROAR!", "Dino wins! ROAR!");
            A("g.score", "Rezultat: ti {0} - dino {1}", "Rezultat: ti {0} - dino {1}", "Rezultat: ti {0} - dino {1}", "Score: you {0} - dino {1}");
            A("g.guess", "Pogodi broj", "Pogodi broj", "Pogodi broj", "Guess the number");
            A("g.guess.start", "{Zamislio|Zamislila} sam broj od 1 do 100. Pogađaj!", "{Zamislio|Zamislila} sam broj od 1 do 100. Pogađaj!", "{Zamislio|Zamislila} sam broj od 1 do 100. Pogađaj!", "I'm thinking of a number from 1 to 100. Guess!");
            A("g.guess.bad", "Upiši broj od 1 do 100.", "Upiši broj od 1 do 100.", "Upiši broj od 1 do 100.", "Type a number from 1 to 100.");
            A("g.guess.up", "{0}? Više!", "{0}? Više!", "{0}? Više!", "{0}? Higher!");
            A("g.guess.down", "{0}? Manje!", "{0}? Manje!", "{0}? Manje!", "{0}? Lower!");
            A("g.guess.ok", "Bravo! Broj je {0}, pogodio si iz {1}. pokušaja.", "Bravo! Broj je {0}, pogodio si iz {1}. pokušaja.", "Bravo! Broj je {0}, pogodio si iz {1}. pokušaja.", "Yes! It was {0}, you got it in {1} tries.");
            A("g.try", "Pokušaj", "Pokušaj", "Pokušaj", "Guess");
            A("g.new", "Novi broj", "Novi broj", "Novi broj", "New number");
            A("g.say.win", "Dobro igraš! Ali sljedeći put sam ja na redu.|Ma {pustio|pustila} sam te.|Revanš! Odmah!", "Dobro igraš! Ali sledeći put sam ja na redu.|Ma {pustio|pustila} sam te.|Revanš! Odmah!", "Dobro igraš! Ali sljedeći put sam ja na redu.|Ma {pustio|pustila} sam te.|Revanš! Odmah!", "Nice one! Next time it's mine.|I let you win.|Rematch! Now!");
            A("g.say.lose", "ROAR! Pobjeda!|Dinosaurusi su prvaci!|Hoćeš revanš?", "ROAR! Pobeda!|Dinosaurusi su prvaci!|Hoćeš revanš?", "ROAR! Pobjeda!|Dinosauri su prvaci!|Hoćeš revanš?", "ROAR! Victory!|Dinosaurs are champions!|Want a rematch?");

            // sezonski izgled
            A("m.seasonal", "Sezonski izgled", "Sezonski izgled", "Sezonski izgled", "Seasonal look");
            A("season.halloween", "Bu! {Uplašio|Uplašila} sam te? Sretna Noć vještica!|Vidi mi šešir. Strašno, a?|Noć vještica je. Ja sam već {dovoljno strašan|dovoljno strašna}, sa zubima.",
                                  "Bu! {Uplašio|Uplašila} sam te? Srećna Noć veštica!|Vidi mi šešir. Strašno, a?|Noć veštica je. Ja sam već {dovoljno strašan|dovoljno strašna}, sa zubima.",
                                  "Bu! Jesam li te {prestrašio|prestrašila}? Sretna Noć vještica!|Vidi mi šešir. Strašno, a?|Noć vještica je. Ja sam već {dovoljno strašan|dovoljno strašna}, sa zubima.",
                                  "Boo! Scared you? Happy Halloween!|Check out my hat. Spooky, huh?|It's Halloween. I'm scary enough already, with these teeth.");
            A("season.xmas", "Ho-ho-ho! Sretni praznici!|Kapa je od Djed Mraza. {Posudio|Posudila} sam je.|Snijeg, kolači i ja. Savršeno.",
                             "Ho-ho-ho! Srećni praznici!|Kapa je od Deda Mraza. {Pozajmio|Pozajmila} sam je.|Sneg, kolači i ja. Savršeno.",
                             "Ho-ho-ho! Sretni blagdani!|Kapa je od Djedice Mraza. {Posudio|Posudila} sam je.|Snijeg, kolači i ja. Savršeno.",
                             "Ho-ho-ho! Happy holidays!|The hat is Santa's. I borrowed it.|Snow, cookies and me. Perfect.");

            // podsjetnici
            A("m.reminders", "Podsjetnici", "Podsetnici", "Podsjetnici", "Reminders");
            A("rem.off", "Isključeno", "Isključeno", "Isključeno", "Off");
            A("rem.every", "Svakih {0} min", "Svakih {0} min", "Svakih {0} min", "Every {0} min");
            A("rem.saidon", "Dogovoreno. Podsjećam te svakih {0} minuta.", "Dogovoreno. Podsećam te svakih {0} minuta.", "Dogovoreno. Podsjećam te svakih {0} minuta.", "Deal. I'll remind you every {0} minutes.");
            A("rem.saidoff", "Dobro, neću te više podsjećati.", "Dobro, neću te više podsećati.", "Dobro, neću te više podsjećati.", "Okay, no more reminders.");
            A("rem.say", "Ustani i protegni se malo. I ja ću.|Popij čašu vode. Dinosaurusi koji piju vodu ne izumiru.|Odmori oči: 20 sekundi gledaj nešto daleko.|Ispravi leđa. Izgledaš kao Raptor.|Pauza od dva minuta. Šef neće primijetiti.",
                          "Ustani i protegni se malo. I ja ću.|Popij čašu vode. Dinosaurusi koji piju vodu ne izumiru.|Odmori oči: 20 sekundi gledaj nešto daleko.|Ispravi leđa. Izgledaš kao Raptor.|Pauza od dva minuta. Šef neće primetiti.",
                          "Ustani i protegni se malo. I ja ću.|Popij čašu vode. Dinosauri koji piju vodu ne izumiru.|Odmori oči: 20 sekundi gledaj nešto daleko.|Ispravi leđa. Izgledaš kao Raptor.|Pauza od dvije minute. Šef neće primijetiti.",
                          "Stand up and stretch a bit. Me too.|Drink a glass of water. Dinosaurs who drink water don't go extinct.|Rest your eyes: look at something far away for 20 seconds.|Sit up straight. You look like a Raptor.|Two-minute break. The boss won't notice.");

            // savjeti za nove korisnike
            A("tip.prefix", "Savjet: ", "Savet: ", "Savjet: ", "Tip: ");
            A("tip.list", "desni klik na mene otvara sve opcije.|dupli klik me nahrani.|Ctrl+Alt+D otvara moj Status odakle god.|Ctrl+Alt+F i ispričam foru.|uhvati me mišem i baci. Volim da letim!|spusti me na vrh prozora, pa šetam po njemu.|desni klik -> Igre... za kamen-papir-makaze.|Fore -> Preuzmi pakete sa interneta... ima fore za tvoju struku.|Fore -> Samo ovaj paket, ako hoćeš samo jednu temu.|Dostignuća otključavaju nove vrste i šare.",
                          "desni klik na mene otvara sve opcije.|dupli klik me nahrani.|Ctrl+Alt+D otvara moj Status odakle god.|Ctrl+Alt+F i ispričam foru.|uhvati me mišem i baci. Volim da letim!|spusti me na vrh prozora, pa šetam po njemu.|desni klik -> Igre... za kamen-papir-makaze.|Fore -> Preuzmi pakete sa interneta... ima fore za tvoju struku.|Fore -> Samo ovaj paket, ako hoćeš samo jednu temu.|Dostignuća otključavaju nove vrste i šare.",
                          "desni klik na mene otvara sve opcije.|dvostruki klik me nahrani.|Ctrl+Alt+D otvara moj Status odakle god.|Ctrl+Alt+F i ispričam foru.|uhvati me mišem i baci. Volim letjeti!|spusti me na vrh prozora, pa šećem po njemu.|desni klik -> Igre... za kamen-škare-papir.|Fore -> Preuzmi pakete s interneta... ima fora za tvoju struku.|Fore -> Samo ovaj paket, ako želiš samo jednu temu.|Postignuća otključavaju nove vrste i uzorke.",
                          "right-click me for all options.|double-click to feed me.|Ctrl+Alt+D opens my Status from anywhere.|Ctrl+Alt+F and I tell a joke.|grab me with the mouse and throw me. I love flying!|drop me on top of a window and I'll walk on it.|right-click -> Games... for rock-paper-scissors.|Jokes -> Download packs... has jokes for your job.|Jokes -> Only this pack, if you want just one topic.|Achievements unlock new species and patterns.");

            // nova verzija
            A("upd.say", "Izašla je nova verzija ({0})! Desni klik na mene -> Preuzmi.", "Izašla je nova verzija ({0})! Desni klik na mene -> Preuzmi.", "Izašla je nova verzija ({0})! Desni klik na mene -> Preuzmi.", "A new version is out ({0})! Right-click me -> Download.");
            A("upd.menu", "Preuzmi novu verziju {0}...", "Preuzmi novu verziju {0}...", "Preuzmi novu verziju {0}...", "Download new version {0}...");

            // šala na prezentaciji
            A("m.prank", "Šala na prezentaciji (upadne i izvini se)", "Šala na prezentaciji (upadne i izvini se)", "Šala na prezentaciji (upadne i ispriča se)", "Presentation prank (pops in and apologizes)");
            A("prank.on", "Hehe. Na sljedećoj prezentaciji ću svratiti na sekund.", "Hehe. Na sledećoj prezentaciji ću svratiti na sekund.", "Hehe. Na sljedećoj prezentaciji svratit ću na sekundu.", "Hehe. I'll drop by your next presentation for a second.");
            A("prank.off", "Dobro, na prezentacijama sam {miran|mirna} kao bubica.", "Dobro, na prezentacijama sam {miran|mirna} kao bubica.", "Dobro, na prezentacijama sam {miran|mirna} kao bubica.", "Okay, I'll be quiet as a mouse during presentations.");
            A("prank.say", "Ups, izvinite! Pogrešna sala.|Ne obraćajte pažnju na mene, samo prolazim.|Pardon! {Mislio|Mislila} sam da je pauza za kafu.|Ups! Nastavite, odlično vam ide.|Izvinite, je li ovo sastanak za dinosauruse?",
                           "Ups, izvinite! Pogrešna sala.|Ne obraćajte pažnju na mene, samo prolazim.|Pardon! {Mislio|Mislila} sam da je pauza za kafu.|Ups! Nastavite, odlično vam ide.|Izvinite, je li ovo sastanak za dinosauruse?",
                           "Ups, ispričavam se! Kriva dvorana.|Ne obraćajte pažnju na mene, samo prolazim.|Pardon! {Mislio|Mislila} sam da je pauza za kavu.|Ups! Nastavite, odlično vam ide.|Oprostite, je li ovo sastanak za dinosaure?",
                           "Oops, sorry! Wrong room.|Don't mind me, just passing through.|Pardon! I thought it was a coffee break.|Oops! Carry on, you're doing great.|Excuse me, is this the dinosaur meeting?");

            // razgibavanje
            A("say.stretch", "Aaaah... razgibavanje.|Krc, krc. Kičma od 65 miliona godina.|Jutarnje istezanje, kao pravi sportista.|Još samo malo da se protegnem...|Uh, {ukočio|ukočila} sam se ko fosil.",
                             "Aaaah... razgibavanje.|Krc, krc. Kičma od 65 miliona godina.|Jutarnje istezanje, kao pravi sportista.|Još samo malo da se protegnem...|Uh, {ukočio|ukočila} sam se ko fosil.",
                             "Aaaah... razgibavanje.|Krc, krc. Kralježnica od 65 milijuna godina.|Jutarnje istezanje, kao pravi sportaš.|Još se samo malo protegnem...|Uh, {ukočio|ukočila} sam se ko fosil.",
                             "Aaaah... stretching time.|Crack, crack. A 65-million-year-old spine.|Morning stretch, like a real athlete.|Just a little more stretching...|Ugh, I'm stiff as a fossil.");

            // nove šare
            A("pat.gold", "Zlatna", "Zlatna", "Zlatna", "Gold");
            Lang.BsOverride["ach.stylist.d"] = "Promijeni izgled u postavkama.";
            Lang.BsOverride["ach.days7.d"] = "Druži se s dinom 7 različitih dana.";
            Lang.BsOverride["ach.days30.d"] = "Druži se s dinom 30 različitih dana.";
            A("pat.rainbow", "Duga", "Duga", "Duga", "Rainbow");
        }
    }
}

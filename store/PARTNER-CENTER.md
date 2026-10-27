# Objava na Microsoft Store - koraci

## 1. Nalog (jednom)
1. https://storedeveloper.microsoft.com -> **Individual developer** (besplatno, potrebna lična karta/pasoš za verifikaciju).
2. Publisher display name: `cyber-security.ba` (mora se poklapati sa `PUBLISHER_DISPLAY` u `msix/identity.ini`).

## 2. Rezervacija imena
Partner Center -> **Apps and games** -> **New product** -> **MSIX or PWA app** -> ime: `Dino Joe - Desktop Pet`.

## 3. Identitet paketa
Product management -> **Product identity**. Prepisati u `msix/identity.ini`:
- `Package/Identity/Name` -> `IDENTITY_NAME`
- `Package/Identity/Publisher` (CN=...) -> `PUBLISHER`
- `Package/Properties/PublisherDisplayName` -> `PUBLISHER_DISPLAY`

Commit + novi tag (npr. `v1.1.1`) -> GitHub Actions napravi `DinoJoe-v1.1.1.msix` (artifact `...-msix`).

## 4. Submission
- **Pricing and availability:** Free, sva tržišta.
- **Properties:** Category *Entertainment*; Privacy policy URL: `https://github.com/freedole/dino-joe/blob/main/PRIVACY.md`.
- **Age ratings (IARC):** upitnik - bez nasilja, bez kupovina, bez razmjene sadržaja među korisnicima.
- **Packages:** upload `.msix` iz artifacta (nepotpisan, Store ga potpisuje).
- **Store listings:** tekst iz `store/listing-en.md` i `store/listing-sr.md`; logo `store/StoreLogo-300.png`; screenshotovi 1366x768 ili veći (najmanje 1, preporuka 4).
- **runFullTrust:** u polju za obrazloženje: *"Desktop pet app built with .NET Framework WinForms; it draws an always-on-top transparent window on the desktop and needs full trust to run as a classic Win32 app."*

## Lokalni test MSIX-a (prije Store-a)
1. Iz artifacta `...-msix` uzmi `test/DinoJoe-test.cer` i `test/DinoJoe-...-test.msix`.
2. PowerShell kao admin: `Import-Certificate -FilePath DinoJoe-test.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople`
3. Dupli klik na `...-test.msix` -> Install.
4. Deinstalacija: Start -> Dino Joe -> desni klik -> Uninstall; certifikat obrisati iz `certlm.msc` -> Trusted People.

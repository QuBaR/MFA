# MFA Console Demo (.NET 8)

En enkel övningsapplikation som visar hur man lägger till TOTP-baserad Multi-Factor Authentication (MFA) i en inloggning. TOTP står för Time-based One-Time Password – på svenska ett tidsbaserat engångslösenord. Det är en vidareutveckling av HOTP där koden beräknas utifrån (hemlig nyckel + aktuell tidsintervall, t.ex. 30 sek), vilket ger en ny kod varje tidsfönster.

## Funktioner
- Registrera användare (in-memory)
- Lösenordshashning (SHA256 för demo – byt till t.ex. PBKDF2/Argon2 i produktion)
- Aktivering av MFA med TOTP (Otp.NET)
- Verifiering av engångskoder
- Recovery codes (engångsreservkoder)

## Körning
```powershell
dotnet run
```

### Logga in
Förifyllda demoanvändare:

Användare: `anna`
Lösenord: `Password123`

Ange sedan aktuell TOTP-kod (lägg till hemlig nyckel i en authenticator-app) eller en recovery code.

Användare: `bertil` (utan MFA initialt) – kan aktivera MFA vid första inloggning.

### Flöde
1. Start: två användare skapas: `anna` (MFA aktiv) och `bertil` (utan MFA).
2. Ange användarnamn + lösenord.
3. Om användaren saknar MFA → fråga om aktivering (skriver ut hemlig Base32-nyckel + recovery codes).
4. För MFA-skyddade konton → ange TOTP eller en giltig recovery code.
5. Vid användning av en recovery code tas den bort (engångsbruk).

### Tips för authenticator
1. Starten skriver ut Annas Base32-hemlighet.
2. Lägg in den manuellt i t.ex. Microsoft / Google Authenticator.
3. Alternativt skapa QR-kod: hämta provisioning URI via `MfaService.GetProvisioningUri(user, "DemoIssuer")` (lägg ett temporärt `Console.WriteLine`) och mata in i valfri online QR‑generator.

Format provisioning URI:
```
otpauth://totp/Issuer:username?secret=BASE32&issuer=Issuer&digits=6&period=30&algorithm=SHA1
```

## Vanliga testfall
- Korrekt lösen + fel TOTP → ska nekas.
- Korrekt lösen + korrekt TOTP → inloggad.
- Använd recovery code → fungerar och koden försvinner (engångs).
- Förbrukad recovery code igen → nekas.
- Aktivera MFA för `bertil` under körning → kontrollera aktivering och första verifiering.

## Generera / verifiera TOTP
Koden roterar var 30:e sekund (standard). Verifieringsfönster tillåter ±1 steg (tidsdrift).

## Reflektionsstöd (kortfattat)
- Skillnad i säkerhet: MFA stoppar åtkomst vid läckt lösenord (kräver något ytterligare – "något du har").
- Problem för användare: tidsdrift, borttappad telefon, onboarding-friktion, extra steg.
- Riktig implementation i produktion:
	- Starkare lösenordshash (PBKDF2/Argon2/bcrypt)
	- Krypterad lagring av TOTP-hemlighet
	- Rate limiting, audit logging, larm
	- Möjlighet att återkalla/rotera hemligheter
	- WebAuthn som starkare faktor på sikt

## Förslag på enkla nästa förbättringar
- Lägg till QR-kodsutskrift via ett paket (t.ex. QRCoder) i konsolen.
- Spara data till fil (persistens) istället för in-memory.
- Lägga till lockout efter X misslyckade försök.
- Maskera återstående recovery codes i UI (visa bara initialt / på begäran).

Allt klart för inlämning: zipa mappen eller pusha till GitHub och fyll i `REPORT_TEMPLATE.md` med skärmdumpar. Behöver du nästa steg (QR-kod, persistens, WebAPI-version) – säg till.

## Generera TOTP i Authenticator (sammanfattning)
Använd hemlig Base32-nyckel som skrivs ut eller skapa en QR-kod via provisioning URI.

## Säkerhetsnoteringar (för produktion)
- Använd stark lösenordshantering (PBKDF2, bcrypt, scrypt, Argon2)
- Kryptera/hascha TOTP-hemligheter vilande
- Begränsa inloggningsförsök + rate limiting
- Logga och övervaka lyckade/misslyckade försök
- Skydda recovery codes (visa bara en gång)

## Rapportmall
Se `REPORT_TEMPLATE.md`.

# MFA Console Demo (.NET 8)

En enkel övningsapplikation som visar hur man lägger till TOTP-baserad Multi-Factor Authentication (MFA) i en inloggning. TOTP står för Time-based One-Time Password – på svenska ett tidsbaserat engångslösenord. Det är en vidareutveckling av HOTP där koden beräknas utifrån (hemlig nyckel + aktuell tidsintervall, t.ex. 30 sek), vilket ger en ny kod varje tidsfönster.

## Funktioner
- Registrera användare (in-memory)
- Lösenordshashning (SHA256 för demo – byt till t.ex. PBKDF2/Argon2 i produktion)
- Aktivering av MFA med TOTP (Otp.NET)
- Verifiering av engångskoder
- Recovery codes (engångsreservkoder)
 - Kontolåsning: 5 misslyckade lösenordsförsök låser kontot i 2 minuter
 - Meny efter inloggning för att visa kvarvarande recovery codes och (om slut) regenerera nya

## Körning
```powershell
dotnet run
```

Vill du bara bygga:
```powershell
dotnet build
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
6. Efter lyckad MFA-inloggning visas en liten meny:
	- (R) Visa återstående recovery codes (med varning). Om slut: erbjud regenerering (skapar 5 nya).
	- (Q) Logga ut.

### Kontolåsning
- Efter varje misslyckat lösenordsförsök ökas en räknare.
- Vid 5 misslyckade försök: konto låses i 2 minuter och räknaren nollställs.
- Under låsning visas återstående sekunder.
- Lyckad lösenordsverifiering nollställer räknare och lås.

### Tips för authenticator
1. Starten skriver ut Bertils Base32-hemlighet.
2. Lägg in den manuellt i t.ex. Microsoft / Google Authenticator.


## Vanliga testfall
- Korrekt lösen + fel TOTP → ska nekas.
- Korrekt lösen + korrekt TOTP → inloggad.
- Använd recovery code → fungerar och koden försvinner (engångs).
- Förbrukad recovery code igen → nekas.
- Aktivera MFA för `bertil` under körning → kontrollera aktivering och första verifiering.
- Lås konto: mata in fel lösenord 5 gånger → försök igen inom 2 min ska visa låsmeddelande.
- Efter låstid: korrekt lösen + TOTP → fungerar igen.

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

## Tester
Projektet innehåller ett testprojekt (`MFA.Tests`) med xUnit som verifierar TOTP-funktionaliteten.

Kör alla tester:
```powershell
dotnet test
```

Köra endast testprojektet (snabbare iteration):
```powershell
dotnet test .\MFA.Tests\MFA.Tests.csproj
```

Struktur (kort):
- `MfaServiceTests` – testar giltig respektive ogiltig TOTP-kod med deterministiska hemligheter.

Lägg gärna till fler tester, t.ex. för recovery codes (förbrukning) och lockout.

Allt klart för inlämning: zipa mappen eller pusha till ert GitHub repo  och posta svar i Learnpoint ett dokument med  med skärmdumpar. 

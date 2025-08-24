# Övning: Bygg vidare på MFA Console Demo 

## Syfte
Träna på att:
- Förstå och utöka existerande kodbas
- Implementera enklare säkerhetsförbättringar
- Arbeta med enkla persistenslösningar och testfall
- Resonera kring säkerhet vs användbarhet

## Förkunskaper
- Grundläggande C# och .NET Console-app
- Klasser, listor, enkla tjänstelager
- Enkel läsning/skrivning av JSON

## Utgångsläge
Du har en färdig konsolapplikation som hanterar:
- Lösenordsinloggning
- Aktivering av TOTP-baserad MFA
- Verifiering av TOTP och recovery codes

Granska koden: `Program.cs`, `MfaService`, `InMemoryUserStore`, `User`.

## Uppgift – Delmoment
Du ska implementera följande minimikrav (G). Utför i ordning. Försök hålla nere omfattning och håll koden ren.

### 1. Konto‑låsningsfunktion (Account Lockout)
Implementera enkel låsning av konto efter t.ex. 5 misslyckade MFA- eller lösenordsförsök i följd.
- Lägg till fält i `User` för: `FailedAttempts` och `LockedUntil` (DateTime?).
- På varje misslyckat försök: öka räknaren.
- Vid gräns: sätt `LockedUntil = Now + 2 minuter` (justerbart) och nollställ räknaren.
- Om konto är låst: visa tydligt meddelande och blockera inloggning innan lösenords/MFA-kontroll.
- Vid lyckad full autentisering: nollställ `FailedAttempts`.


### 2. Kommandot "visa återstående recovery codes"
Lägg till ett enkelt menyval (t.ex. skriv `recovery` efter inloggning) som listar kvarvarande recovery codes för den inloggade användaren.
- Visa en varning: "Visa dessa endast i en säker miljö".
- Om alla är slut: erbjud att generera nya (bekräftelsefråga). Generering nollställer gamla (tom lista → skapa ny uppsättning).

### 3. Enkel test av MFA-verifiering (frivilligt om tid / +VG)
Skapa ett litet test (kan vara ett extra konsolkommando eller ett xUnit-projekt om du hinner) som:
- Skapar en användare med känd hemlighet (hardcodad byte-array)
- Genererar aktuell TOTP-kod och verifierar att `VerifyCode` returnerar true
- Testar att en uppenbart felaktig kod ("000000") returnerar false

Om tiden är knapp: skriv en separat metod du anropar manuellt och dokumentera resultatet i rapporten.

### 4. Reflektion
Besvara (i rapportmallen):
- Hur påverkar låsning användbarhet vs säkerhet?
- Risker med att spara hemligheter i klartext?
- Vad händer om två processer samtidigt försöker skriva filen? (kort resonemang)

## Bonus / Extra (om allt ovan klart)
Välj max 1–2:
- Generera QR‑kod i konsolen (paket: QRCoder) för provisioning URI.
- Lägg in konfigurationsfil för att styra: låsgräns, låstid, antal recovery codes.
- Lägg till loggning till separat textfil (ingen extern loggram). Undvik logg av koder.

## Leverabler
1. Uppdaterad kod (GitHub eller zip)
2. Ifylld `REPORT_TEMPLATE.md` (eller PDF) med:
   - Kort beskrivning av dina implementationer
   - Testfall (inkl. lockout-scenario)
   - Reflektioner
3. (Bonus om gjort) QR-kod-skärmdump eller testutskrift

## Bedömning (förslag)
| Nivå | Kriterier |
|------|-----------|
| G | Låsning implementerad, recovery-visning + regenerering, testfall manuellt verifierade, reflektion ifylld |
| VG | Allt för G + enkel verifieringstest (kod eller tydligt dokumenterad), välmotiverad reflektion kring säkerhet/användbarhet + minst 1 bonus genomförd (korrekt) |
| IG | Centrala funktioner saknas eller fungerar inte, eller reflektion saknas |

## Tips
- Håll förändringar lokala – rör inte MFA-algoritmen.
- Testa ofta: kör efter varje delmoment.
- Logik för lockout: placera tidigt i inloggningsflödet.

## Avgränsningar
- Ingen avancerad tråd-/samtidighet krävs.
- Ingen kryptering behövs (men kommentera i reflektion att det bör finnas i produktion).
# MFA Console Demo (.NET 8)

En enkel övningsapplikation som visar hur man lägger till TOTP-baserad Multi-Factor Authentication (MFA) i en inloggning.

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

## Flöde
1. Start: två användare skapas: `anna` (med MFA) och `bertil` (utan MFA).
2. Logga in med användarnamn + lösenord.
3. Om MFA ej aktiverad: erbjud att aktivera (visar hemlig nyckel + recovery codes).
4. Ange TOTP-kod eller recovery-kod för full autentisering.

## Generera TOTP i Authenticator
Använd hemlig nyckel (Base32). Alternativt skapa en QR-kod från provisioning URI (kan genereras via `MfaService.GetProvisioningUri(user, "DemoIssuer")`).

## Säkerhetsnoteringar (för produktion)
- Använd stark lösenordshantering (PBKDF2, bcrypt, scrypt, Argon2)
- Kryptera/hascha TOTP-hemligheter vilande
- Begränsa inloggningsförsök + rate limiting
- Logga och övervaka lyckade/misslyckade försök
- Skydda recovery codes (visa bara en gång)

## Rapportmall
Se `REPORT_TEMPLATE.md`.

# Rapport – MFA Övning

## 1. Genomförande
Beskriv hur du lade till MFA i applikationen: struktur, klasser, flöde.

## 2. Skärmdumpar
Infoga bilder från:
- Aktivering av MFA (hemlig nyckel + recovery codes)
- Lyckad inloggning med TOTP
- Lyckad inloggning med recovery code
- Misslyckat MFA-försök

## 3. Säkerhetsreflektion
- Skillnad mot endast lösenord
- Vilka attacker mitigera TOTP?
- Risker kvar (phishing, malware, SIM-swap, social engineering)

## 4. Användarupplevelse
- Potentiella problem (tidsdrift, borttappad telefon, onboarding)
- Hur stötta användare (backupkoder, tidsjustering, reset-process)

## 5. Fallback & Recovery
- Vilka fallback-mekanismer implementerades?
- Hur säkerställa att fallback inte blir svag länk?

## 6. Utvecklaransvar
- Hantering av hemligheter
- Loggning vs integritet
- Rate limiting, monitoring, larm

## 7. Vidareutveckling
Förslag: QR-kod, hotp fallback, WebAuthn/FIDO2, device binding, riskbaserad auth.

## 8. Sammanfattning
Kort slutsats kring balans säkerhet / användbarhet.

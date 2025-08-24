# REPORT TEMPLATE (Lämnas in av elev efter övningen)

## 0. Metadata
Namn:
Grupp:
Repo:

## 1. Genomförande
Kort beskrivning av vad du gjort (max ~5 meningar):

## 2. Konto‑låsning (Account Lockout)
Implementation (fält, logik, tröskel, låstid):

## 3. Recovery codes visning / regenerering
Hur fungerar kommandot? När kan man regenerera? Säkerhetsnoteringar:

## 4. (Valfritt) Test av MFA-verifiering
Har du gjort ett test? (Ja/Nej). Kort beskrivning / resultat:

## 5. Testresultat
| Testfall | Förväntat | Resultat (OK/FEL) | Kommentar |
|----------|-----------|-------------------|-----------|
| Korrekt lösen + fel TOTP | Nekas |  |  |
| Korrekt lösen + korrekt TOTP | Inloggad |  |  |
| Recovery code första gången | Inloggad + förbrukas |  |  |
| Samma recovery code igen | Nekas |  |  |
| Aktivera MFA för användare utan MFA | Aktivering + verifiering |  |  |
| Fel lösenord | Nekas |  |  |
| Tom / blank kod efter lösenord | Nekas |  |  |

Extra test (om gjort):

## 6. Reflektion
Hur påverkar låsning användbarhet vs säkerhet?:

Risker med hemligheter i klartext:

Samtidiga skrivningar till fil (kort resonemang):

## 7. Bonus / Extra (om något utfört)
Vad och hur:

## 8. Förbättringsidéer
Lista 2–4 konkreta förbättringar du hade gjort med mer tid:

## 9. Självskattning (G / VG) + motivering
Bedömning och varför:

## 10. Sammanfattning (1–2 meningar)

## 11. Appendix (valfritt)
Skärmdumpar / loggutdrag / övrigt:


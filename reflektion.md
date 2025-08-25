# Reflektion

## Hur påverkar låsning användbarhet vs säkerhet?
Kontolåsning (t.ex. efter 5 felaktiga försök) höjer säkerheten genom att bromsa brute force / credential stuffing. Samtidigt sänker den användbarheten: legitima användare som glömt lösen ord eller skriver fel kan låsas ute och bli frustrerade. För aggressiva trösklar kan också utnyttjas för DoS (angripare triggar låsningar på många konton). Balanslösningar är t.ex. adaptiv fördröjning (ökande väntetid), endast mjuk fördröjning på IP/fingerprint, eller “progressive throttling” istället för hårt lås. Notifiering till användaren om låsning minskar förvirring men bör inte läcka om kontot finns.

## Risker med att spara hemligheter i klartext?
- Exfiltration: Intrång eller läckt backup ger omedelbar åtkomst till alla TOTP-hemligheter / lösenord.
- Insider risk: Administratörer kan läsa och missbruka hemligheter.
- Kjedjeeffekt: Återbrukade hemligheter/lösenord kan användas på andra system.
- Compliance-brott: Bryter mot principer (least privilege, encryption at rest) och regelverk (t.ex. GDPR om känsliga attribut härleds).
- Svårare rotation: Okrypterat lagrat material gör det enklare att skriptmässigt massutnyttja, vilket ökar pressen vid incident.
Åtgärder: Hasha *alltid* lösenord med adaptiv KDF (Argon2/PBKDF2/bcrypt/scrypt), kryptera TOTP-secrets (t.ex. AES-GCM) med nyckel i HSM/KeyVault, minst möjlig åtkomsträtt, audit logging och regelbunden rotation.

## Vad händer om flera processer skriver fil samtidigt?
Utan synkronisering kan race conditions ge:
- Överskrivning av varandras data (lost update).
- Delvis skrivna filer (korrupt innehåll) om write inte är atomisk.
- Interleaving av rader → inkonsistenta poster.
- Tearing vid parallella writes på nätverksfilsystem.
- Läsare kan få halvfärdiga data (dirty read).
Vanliga strategier: skriv till temporär fil och atomic rename (replace), fil-lås (OS-level flock / FileStream med FileShare.None), append-only + periodisk kompaktering, transaktionslogg + journaling, eller använd en databas med transaktionsstöd. För hög concurrency: använd queue eller central tjänst istället för alla processer direkt mot filsystemet.

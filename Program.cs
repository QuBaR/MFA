using MFA.Storage;
using MFA.Services;
using System.Text;
using System.Text.Json;
using QRCoder;

var store = new InMemoryUserStore();
Console.WriteLine("=== Demo: MFA Login System ===\n");

// Load config
var cfg = LoadConfig();
string issuer = cfg.TryGetValue("Issuer", out var iss) ? iss! : "Demo";
bool enableQr = !cfg.TryGetValue("EnableQr", out var eq) || bool.TryParse(eq, out var b) && b;
string? logFile = cfg.TryGetValue("LogFile", out var lf) ? lf : null;

Seed();

while (true)
{
	Console.Write("Användarnamn (eller 'exit'): ");
	var userName = Console.ReadLine();
	if (userName?.Equals("exit", StringComparison.OrdinalIgnoreCase) == true) break;
	Console.Write("Lösenord: ");
	var pwd = ReadPassword();

	var user = store.Find(userName!);
	if (user == null)
	{
		Console.WriteLine("❌ Ogiltiga inloggningsuppgifter.\n");
		continue;
	}

	// Check if account is locked
	if (user.IsLocked)
	{
		var remaining = user.LockRemaining;
		Console.WriteLine($"🔒 Konto låst. Försök igen om {(int)remaining!.Value.TotalSeconds} sekunder.\n");
		continue;
	}

	if (!PasswordHasher.Verify(pwd, user.PasswordHash))
	{
		user.FailedLoginAttempts++;
		if (user.FailedLoginAttempts >= 5)
		{
			user.LockedUntil = DateTime.UtcNow.AddMinutes(2);
			user.FailedLoginAttempts = 0; // reset counter after lock to avoid immediate relock
			Console.WriteLine("🔒 För många misslyckade försök. Konto låst i 2 minuter.\n");
		}
		else
		{
			var left = 5 - user.FailedLoginAttempts;
			Console.WriteLine($"❌ Ogiltigt lösenord. {left} försök kvar innan låsning.\n");
		}
		continue;
	}

	// Successful password => reset failed attempts + lock
	user.FailedLoginAttempts = 0;
	user.LockedUntil = null;

	if (user.TotpSecret == null)
	{
		Console.Write("MFA ej aktiverat. Aktivera nu? (y/n): ");
		if (Console.ReadLine()?.StartsWith("y", StringComparison.OrdinalIgnoreCase) == true)
		{
			var secret = store.Mfa.Enroll(user);
			var base32 = OtpNet.Base32Encoding.ToString(secret);
			Console.WriteLine($"Hemlig nyckel (skanna i authenticator-app): {base32}");
			var uri = store.Mfa.GetProvisioningUri(user, issuer);
			if (enableQr)
			{
				Console.WriteLine("QR-kod (scanna i authenticator-app):");
				RenderQrToConsole(uri);
			}
			Log($"MFA enrolled for {user.Username}");
			Console.WriteLine("Recovery codes:");
			foreach (var rc in user.RecoveryCodes) Console.WriteLine("  " + rc);
			Console.WriteLine();
			Console.WriteLine("Ange första TOTP-kod för att bekräfta: ");
			var first = Console.ReadLine();
			if (!store.Mfa.VerifyCode(user, first!))
			{
				user.TotpSecret = null; // rollback
				Console.WriteLine("❌ Verifiering misslyckades. MFA ej aktiverat.\n");
				continue;
			}
			Console.WriteLine("✅ MFA aktiverat.\n");
		}
		else
		{
			Console.WriteLine("Inloggad (utan MFA).\n");
			continue;
		}
	}

	// MFA steg
	Console.Write("Ange TOTP eller recovery-kod: ");
	var code = Console.ReadLine();
	bool mfaOk = store.Mfa.VerifyCode(user, code!) || store.Mfa.UseRecoveryCode(user, code!);
	if (!mfaOk)
	{
		Console.WriteLine("❌ Fel kod.\n");
		continue;
	}
	Console.WriteLine("✅ Inloggning klar!\n");

	// Post-login mini-meny
	while (true)
	{
		Console.WriteLine("Meny: (R) Visa recovery codes  (Q) Logga ut");
		Console.Write("Val: ");
		var choice = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(choice)) continue;
		if (choice.StartsWith("q", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine();
			break; // logout
		}
		if (choice.StartsWith("r", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("⚠️  Varning: Visa dessa koder på en säker plats. Varje kod kan användas EN gång.\n");
			if (user.RecoveryCodes.Count == 0)
			{
				Console.Write("Inga recovery codes kvar. Generera nya? (y/n): ");
				if (Console.ReadLine()?.StartsWith("y", StringComparison.OrdinalIgnoreCase) == true)
				{
					store.Mfa.RegenerateRecoveryCodes(user);
					Console.WriteLine("Nya recovery codes genererade:");
					foreach (var rc in user.RecoveryCodes) Console.WriteLine("  " + rc);
					Console.WriteLine();
				}
				else
				{
					Console.WriteLine("(Avbrutet)\n");
				}
			}
			else
			{
				Console.WriteLine("Återstående recovery codes:");
				foreach (var rc in user.RecoveryCodes) Console.WriteLine("  " + rc);
				Console.WriteLine();
			}
		}
	}
}

void Seed()
{
	Console.WriteLine("Skapar två testanvändare: anna (MFA), bertil (utan MFA)\n");
	var anna = store.Register("anna", "Password123", enrollMfa: true);
	Console.WriteLine("Anna - hemlig nyckel: " + OtpNet.Base32Encoding.ToString(anna.TotpSecret!));
	Console.WriteLine("Anna recovery codes:");
	foreach (var rc in anna.RecoveryCodes) Console.WriteLine("  " + rc);
	store.Register("bertil", "Password123");
	Console.WriteLine();
}

string ReadPassword()
{
	var sb = new StringBuilder();
	ConsoleKeyInfo key;
	while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
	{
		if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
		{
			sb.Length--;
			Console.Write("\b \b");
		}
		else if (!char.IsControl(key.KeyChar))
		{
			sb.Append(key.KeyChar);
			Console.Write('*');
		}
	}
	Console.WriteLine();
	return sb.ToString();
}

Dictionary<string,string> LoadConfig()
{
	var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
	try
	{
		var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
		if (!File.Exists(path)) return dict;
		var json = File.ReadAllText(path);
		using var doc = JsonDocument.Parse(json);
		foreach (var p in doc.RootElement.EnumerateObject())
		{
			dict[p.Name] = p.Value.ToString();
		}
	}
	catch { }
	return dict;
}

void RenderQrToConsole(string text)
{
	// Generate QR payload (UTF8)
	var gen = new QRCodeGenerator();
	var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
	var matrix = data.ModuleMatrix;
	if (matrix == null) return;
	// Add quiet zone
	int size = matrix.Count;
	var sb = new StringBuilder();
	string black = "██"; // double width to look more square
	string white = "  ";
	for (int y = -1; y <= size; y++)
	{
		for (int x = -1; x <= size; x++)
		{
			bool dark = (x >=0 && y >=0 && x < size && y < size) && matrix[y][x];
			sb.Append(dark ? black : white);
		}
		sb.AppendLine();
	}
	Console.WriteLine(sb.ToString());
}

void Log(string message)
{
	if (string.IsNullOrEmpty(logFile)) return;
	try
	{
		File.AppendAllText(logFile!, $"{DateTime.UtcNow:o} {message}{Environment.NewLine}");
	}
	catch { }
}

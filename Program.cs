using MFA.Storage;
using MFA.Services;
using System.Text;

var store = new InMemoryUserStore();
Console.WriteLine("=== Demo: MFA Login System ===\n");

Seed();

while (true)
{
	Console.Write("Användarnamn (eller 'exit'): ");
	var userName = Console.ReadLine();
	if (userName?.Equals("exit", StringComparison.OrdinalIgnoreCase) == true) break;
	Console.Write("Lösenord: ");
	var pwd = ReadPassword();

	var user = store.Find(userName!);
	if (user == null || !PasswordHasher.Verify(pwd, user.PasswordHash))
	{
		Console.WriteLine("❌ Ogiltiga inloggningsuppgifter.\n");
		continue;
	}

	if (user.TotpSecret == null)
	{
		Console.Write("MFA ej aktiverat. Aktivera nu? (y/n): ");
		if (Console.ReadLine()?.StartsWith("y", StringComparison.OrdinalIgnoreCase) == true)
		{
			var secret = store.Mfa.Enroll(user);
			var base32 = OtpNet.Base32Encoding.ToString(secret);
			Console.WriteLine($"Hemlig nyckel (skanna i authenticator-app): {base32}");
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
	Console.WriteLine(mfaOk ? "✅ Inloggning klar!\n" : "❌ Fel kod.\n");
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

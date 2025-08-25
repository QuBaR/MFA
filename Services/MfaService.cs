using OtpNet;
using MFA.Models;
using System.Security.Cryptography;

namespace MFA.Services;

public class MfaService
{
    public byte[] Enroll(User user)
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        user.TotpSecret = secret;
        user.RecoveryCodes = GenerateRecoveryCodes();
        return secret;
    }

    public string GenerateCurrentCode(User user)
    {
        if (user.TotpSecret == null) throw new InvalidOperationException("User not enrolled");
        var totp = new Totp(user.TotpSecret);
        return totp.ComputeTotp();
    }

    public bool VerifyCode(User user, string code)
    {
        if (user.TotpSecret == null) return false;
        var totp = new Totp(user.TotpSecret);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous:1, future:1));
    }

    public bool UseRecoveryCode(User user, string code)
    {
        var match = user.RecoveryCodes.FirstOrDefault(c => c.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (match == null) return false;
        user.RecoveryCodes.Remove(match); // one-time use
        return true;
    }

    public void RegenerateRecoveryCodes(User user, int count = 5)
    {
        user.RecoveryCodes = GenerateRecoveryCodes(count);
    }

    public string GetProvisioningUri(User user, string issuer)
    {
    if (user.TotpSecret == null) throw new InvalidOperationException("User not enrolled");
    var secretB32 = OtpNet.Base32Encoding.ToString(user.TotpSecret);
    // Standard URI-format för TOTP (Google Authenticator m.fl.)
    // otpauth://totp/Issuer:Account?secret=SECRET&issuer=Issuer&digits=6&period=30&algorithm=SHA1
    var label = Uri.EscapeDataString($"{issuer}:{user.Username}");
    var query = $"secret={secretB32}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30&algorithm=SHA1";
    return $"otpauth://totp/{label}?{query}";
    }

    private List<string> GenerateRecoveryCodes(int count = 5)
    {
        var list = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(Convert.ToHexString(RandomNumberGenerator.GetBytes(5))); // 10 hex chars
        }
        return list;
    }
}

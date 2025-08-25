using MFA.Models;
using MFA.Services;
using OtpNet;
using Xunit;
using System.Linq; // added

namespace MFA.Tests;

public class MfaServiceTests
{
    [Fact]
    public void VerifyCode_WithValidCurrentTotp_ReturnsTrue()
    {
        // Arrange: deterministisk hemlighet
        var secret = Enumerable.Repeat((byte)0x11, 20).ToArray();
        var user = new User { Username = "test", PasswordHash = "irrelevant", TotpSecret = secret, RecoveryCodes = new() };
        var service = new MfaService();
        var totp = new Totp(secret);
        var code = totp.ComputeTotp();

        // Act
        var ok = service.VerifyCode(user, code);

        // Assert
        Assert.True(ok);
    }

    [Fact]
    public void VerifyCode_WithInvalidTotp_ReturnsFalse()
    {
        var secret = Enumerable.Repeat((byte)0x22, 20).ToArray();
        var user = new User { Username = "test", PasswordHash = "irrelevant", TotpSecret = secret, RecoveryCodes = new() };
        var service = new MfaService();

        // Använd medvetet fel kod (modifiera korrekt kod)
        var totp = new Totp(secret);
        var good = totp.ComputeTotp();
        var bad = good == "000000" ? "123456" : new string(good.Reverse().ToArray());
        if (bad == good) bad = "999999"; // fallback om reversal råkar bli samma (teoretiskt ej möjligt här)

        var ok = service.VerifyCode(user, bad);

        Assert.False(ok);
    }
}

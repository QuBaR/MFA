namespace MFA.Models;

public class User
{
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public byte[]? TotpSecret { get; set; }
    public List<string> RecoveryCodes { get; set; } = new();
}

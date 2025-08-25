namespace MFA.Models;

public class User
{
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public byte[]? TotpSecret { get; set; }
    public List<string> RecoveryCodes { get; set; } = new();
    // Lockout logic
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public bool IsLocked => LockedUntil != null && LockedUntil > DateTime.UtcNow;
    public TimeSpan? LockRemaining => IsLocked ? LockedUntil - DateTime.UtcNow : null;
}

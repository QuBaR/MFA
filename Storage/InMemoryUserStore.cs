using MFA.Models;
using MFA.Services;

namespace MFA.Storage;

public class InMemoryUserStore
{
    private readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly MfaService _mfaService = new();

    public User Register(string username, string password, bool enrollMfa = false)
    {
        if (_users.ContainsKey(username)) throw new InvalidOperationException("User exists");
        var user = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(password)
        };
        if (enrollMfa)
        {
            _mfaService.Enroll(user);
        }
        _users[username] = user;
        return user;
    }

    public User? Find(string username) => _users.TryGetValue(username, out var u) ? u : null;
    public IEnumerable<User> All() => _users.Values;
    public MfaService Mfa => _mfaService;
}

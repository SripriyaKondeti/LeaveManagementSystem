using System.IdentityModel.Tokens.Jwt;

namespace LmsMobile.Services;

public class SessionService
{
    private const string TokenKey = "lms_token";
    private const string RoleKey = "lms_role";
    private readonly object _gate = new();

    public string? Token { get; private set; }
    public string? Role { get; private set; }
    public int? EmployeeId { get; private set; }
    public DateTime ExpiresUtc { get; private set; }

    public bool IsManager => Role == "Manager";
    public bool IsLoggedIn => Token is not null && DateTime.UtcNow < ExpiresUtc;

    public async Task SaveAsync(string token, string role)
    {
        Load(token, role);
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
            await SecureStorage.Default.SetAsync(RoleKey, role);
        }
        catch (Exception)
        {
            // Secure storage can be unavailable (e.g. restored Android backup); the session still works in memory.
        }
    }

    // A saved, unexpired token lets the user skip the Sign In screen.
    public async Task<bool> RestoreAsync()
    {
        if (IsLoggedIn) return true;

        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            var role = await SecureStorage.Default.GetAsync(RoleKey);
            if (token is null || role is null) return false;

            Load(token, role);
            if (IsLoggedIn) return true;
        }
        catch (Exception)
        {
            // Unreadable or corrupt token: fall through and require a fresh sign in.
        }

        Clear();
        return false;
    }

    public void Clear()
    {
        ResetState();
        RemoveStored();
    }

    // Ends the session once even when several requests fail with 401 at the same time.
    public bool TryEnd()
    {
        lock (_gate)
        {
            if (Token is null) return false;
            ResetState();
        }

        RemoveStored();
        return true;
    }

    private void ResetState()
    {
        lock (_gate)
        {
            Token = null;
            Role = null;
            EmployeeId = null;
            ExpiresUtc = default;
        }
    }

    private static void RemoveStored()
    {
        try
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(RoleKey);
        }
        catch (Exception)
        {
            // Nothing stored or storage unavailable; the in-memory session is already cleared.
        }
    }

    private void Load(string token, string role)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;

        lock (_gate)
        {
            Token = token;
            Role = role;
            EmployeeId = int.TryParse(claim, out var id) ? id : null;
            ExpiresUtc = jwt.ValidTo;
        }
    }
}

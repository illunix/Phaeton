using System.Security.Claims;


namespace Phaeton.Auth.JWT;

public class JsonWebTokenPayload
{
    public string Subject { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Email { get; init; }
    public DateTime Expiry { get; init; }
    public IEnumerable<string>? Roles { get; init; }
    public IEnumerable<Claim> Claims { get; init; } = Enumerable.Empty<Claim>();
}
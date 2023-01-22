namespace Phaeton.Auth.JWT;

public sealed class JsonWebToken
{
    public string AccessToken { get; init; } = string.Empty;
    public long Expiry { get; init; }
    public long UserId { get; init; }
    public string? Email { get; init; }
    public string? Role { get; init; }
    public IDictionary<string, IEnumerable<string>>? Claims { get; init; }
}
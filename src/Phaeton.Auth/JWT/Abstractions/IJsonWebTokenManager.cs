namespace Phaeton.Auth.JWT.Abstractions;

public interface IJsonWebTokenManager
{
    JsonWebToken CreateToken(
        long userId, 
        string? email = null,
        string? role = null,
        IDictionary<string, IEnumerable<string>>? claims = null
    );
}
using Microsoft.Extensions.Options;
using Phaeton.Auth;
using Phaeton.Auth.JWT.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Phaeton.Auth.JWT;

internal sealed class JWTManager : IJWTManager
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private AuthOptions.JWTOptions _options;

    public JWTManager(IOptions<AuthOptions> options)
    {
        if (options.Value?.JWT is null)
            throw new InvalidOperationException("Missing JWT options.");

        _options = options.Value.JWT;
    }

    public JWT CreateToken(
        long userId,
        string email,
        int role
    )
    {
        var now = DateTime.Now;

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()
            ),
            new(
                JwtRegisteredClaimNames.UniqueName,
                userId.ToString()
            ),
            new(
                JwtRegisteredClaimNames.Email,
                email
            ),
            new(
                ClaimTypes.Role,
                role.ToString()
            ),
            new(
                JwtRegisteredClaimNames.Aud,
                _options.Audience
            )
        };

        var expires = now.Add(TimeSpan.FromHours(1));

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _options.SigningCredentials
        );

        return new()
        {
            AccessToken = _jwtSecurityTokenHandler.WriteToken(jwt),
            Expiry = new DateTimeOffset(expires).ToUnixTimeMilliseconds(),
            UserId = userId,
            Email = email,
            Role = role
        };
    }
}
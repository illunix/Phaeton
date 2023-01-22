using Microsoft.IdentityModel.Tokens;

namespace Phaeton.Auth.JWT;

internal sealed record SecurityKeyDetails(
    SecurityKey Key, 
    string Algorithm
);

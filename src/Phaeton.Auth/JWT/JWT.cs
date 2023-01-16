using System;
using System.Collections.Generic;

namespace Phaeton.Auth.JWT;

public sealed class JWT
{
    public string AccessToken { get; init; } = string.Empty;
    public long Expiry { get; init; }
    public long UserId { get; init; }
    public string? Email { get; init; }
    public int Role { get; init; }
}
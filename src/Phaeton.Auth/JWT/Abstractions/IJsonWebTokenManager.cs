using System.Collections.Generic;

namespace Phaeton.Auth.JWT.Abstractions;

public interface IJsonWebTokenManager
{
    JsonWebToken CreateToken(
        long userId,
        string email,
        int role
    );
}
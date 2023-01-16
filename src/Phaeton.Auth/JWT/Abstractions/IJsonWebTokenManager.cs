using System.Collections.Generic;

namespace Phaeton.Auth.JWT.Abstractions;

public interface IJWTManager
{
    JWT CreateToken(
        long userId,
        string email,
        int role
    );
}
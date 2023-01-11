using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Phaeton.Auth;

public sealed class AuthOptions
{
    public bool Enabled { get; init; } 
    public JWTOptions? JWT { get; set; }
    public GoogleOptions? Google { get; set; }

    public sealed class JWTOptions
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? SecretKey { get; set; }
        public SymmetricSecurityKey? SigningKey { get; private set; }
        public SigningCredentials? SigningCredentials { get; private set; }

        public JWTOptions()
        {
            if (!string.IsNullOrEmpty(SecretKey))
            {
                SigningKey = new(Encoding.ASCII.GetBytes(SecretKey));
                SigningCredentials = new(
                    SigningKey,
                    SecurityAlgorithms.HmacSha256
                );
            }
        }
    }

    public sealed class GoogleOptions
    {
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
    }
}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Phaeton.Auth.JWT;
using Phaeton.Auth.JWT.Abstractions;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Phaeton.Auth;

public static class Extensions
{
    private const string _sectionName = "auth";

    public static long UserId(this HttpContext ctx) 
        => string.IsNullOrWhiteSpace(ctx.User.Identity?.Name) ? default : long.Parse(ctx.User.Identity.Name);

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(_sectionName);
        var options = section.BindOptions<AuthOptions>();
        services.Configure<AuthOptions>(section);
        services.AddAuthentication();

        if (!section.Exists())
            return services;

        if (options.Jwt is null)
            throw new InvalidOperationException("JWT options cannot be null.");

        services.AddAuthorization(q =>
        {
            foreach (var permission in options.Roles ?? Enumerable.Empty<string>())
                q.AddPolicy(
                    permission,
                    q => q.RequireRole(permission)
                );
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            RequireAudience = options.Jwt.RequireAudience,
            ValidIssuer = options.Jwt.ValidIssuer,
            ValidIssuers = options.Jwt.ValidIssuers,
            ValidateActor = options.Jwt.ValidateActor,
            ValidAudience = options.Jwt.ValidAudience,
            ValidAudiences = options.Jwt.ValidAudiences,
            ValidateAudience = options.Jwt.ValidateAudience,
            ValidateIssuer = options.Jwt.ValidateIssuer,
            ValidateLifetime = options.Jwt.ValidateLifetime,
            ValidateTokenReplay = options.Jwt.ValidateTokenReplay,
            ValidateIssuerSigningKey = options.Jwt.ValidateIssuerSigningKey,
            SaveSigninToken = options.Jwt.SaveSigninToken,
            RequireExpirationTime = options.Jwt.RequireExpirationTime,
            RequireSignedTokens = options.Jwt.RequireSignedTokens,
            ClockSkew = TimeSpan.Zero
        };

        services.AddSingleton(tokenValidationParameters);

        var hasCertificate = false;
        var algorithm = options.Algorithm;
        SecurityKey? securityKey = null;
        if (options.Certificate is not null)
        {
            X509Certificate2? certificate = null;
            var password = options.Certificate.Password;
            var hasPassword = !string.IsNullOrWhiteSpace(password);
            if (!string.IsNullOrWhiteSpace(options.Certificate.Location))
            {
                certificate = hasPassword
                    ? new X509Certificate2(options.Certificate.Location, password)
                    : new X509Certificate2(options.Certificate.Location);
                var keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from location: '{options.Certificate.Location}' {keyType}.");
            }

            if (!string.IsNullOrWhiteSpace(options.Certificate.RawData))
            {
                var rawData = Convert.FromBase64String(options.Certificate.RawData);
                certificate = hasPassword
                    ? new X509Certificate2(rawData, password)
                    : new X509Certificate2(rawData);
                var keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from raw data {keyType}.");
            }

            if (certificate is not null)
            {
                if (string.IsNullOrWhiteSpace(options.Algorithm))
                {
                    algorithm = SecurityAlgorithms.RsaSha256;
                }

                hasCertificate = true;
                securityKey = new X509SecurityKey(certificate);
                tokenValidationParameters.IssuerSigningKey = securityKey;
                var actionType = certificate.HasPrivateKey ? "issuing" : "validating";
                Console.WriteLine($"Using X.509 certificate for {actionType} tokens.");
            }
        }

        if (!hasCertificate)
        {
            if (string.IsNullOrWhiteSpace(options.Jwt.IssuerSigningKey))
            {
                throw new InvalidOperationException("Missing issuer signing key.");
            }

            if (string.IsNullOrWhiteSpace(options.Algorithm))
            {
                algorithm = SecurityAlgorithms.HmacSha256;
            }

            var rawKey = Encoding.UTF8.GetBytes(options.Jwt.IssuerSigningKey);
            securityKey = new SymmetricSecurityKey(rawKey);
            tokenValidationParameters.IssuerSigningKey = securityKey;
            Console.WriteLine("Using symmetric encryption for issuing tokens.");
        }

        if (!string.IsNullOrWhiteSpace(options.Jwt.AuthenticationType))
        {
            tokenValidationParameters.AuthenticationType = options.Jwt.AuthenticationType;
        }

        if (!string.IsNullOrWhiteSpace(options.Jwt.NameClaimType))
        {
            tokenValidationParameters.NameClaimType = options.Jwt.NameClaimType;
        }

        if (!string.IsNullOrWhiteSpace(options.Jwt.RoleClaimType))
        {
            tokenValidationParameters.RoleClaimType = options.Jwt.RoleClaimType;
        }

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.Authority = options.Jwt.Authority;
                jwtBearerOptions.Audience = options.Jwt.Audience;
                jwtBearerOptions.MetadataAddress = options.Jwt.MetadataAddress ?? string.Empty;
                jwtBearerOptions.SaveToken = options.Jwt.SaveToken;
                jwtBearerOptions.RefreshOnIssuerKeyNotFound = options.Jwt.RefreshOnIssuerKeyNotFound;
                jwtBearerOptions.RequireHttpsMetadata = options.Jwt.RequireHttpsMetadata;
                jwtBearerOptions.IncludeErrorDetails = options.Jwt.IncludeErrorDetails;
                jwtBearerOptions.TokenValidationParameters = tokenValidationParameters;
                if (!string.IsNullOrWhiteSpace(options.Jwt.Challenge))
                {
                    jwtBearerOptions.Challenge = options.Jwt.Challenge;
                }
            });

        if (securityKey is not null)
        {
            services.AddSingleton(new SecurityKeyDetails(
                securityKey, 
                algorithm
            ));
            services.AddSingleton<IJsonWebTokenManager, JsonWebTokenManager>();
        }

        return services;
    }
}
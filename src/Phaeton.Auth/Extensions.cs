using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Phaeton;
using Phaeton.Auth.JWT;
using Phaeton.Auth.JWT.Abstractions;
using System.Security.Claims;

namespace Phaeton.Auth;

public static class Extensions
{
    public static long GetUserId(this ClaimsPrincipal claims)
        => string.IsNullOrWhiteSpace(claims.Identity?.Name) ? default : long.Parse(claims.Identity.Name);

    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("auth");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<AuthOptions>();
        if (options.JWT is null)
            throw new InvalidOperationException("JWT options cannot be null.");

        services.Configure<AuthOptions>(section);

        services.AddAuthorization();

        services.AddAuthentication(q =>
        {
            q.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            q.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(q =>
            {
                q.ClaimsIssuer = options.JWT.Issuer;
                q.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.JWT.Issuer,

                    ValidateAudience = true,
                    ValidAudience = options.JWT.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = options.JWT.SigningKey,

                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                q.SaveToken = true;

                q.Events = new()
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            ctx.Response.Headers.Add(
                                "Token-Expired",
                                "true"
                            );
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthentication("Cookies")
            .AddCookie(q =>
            {
                q.Cookie.Name = "googleauth";
                q.LoginPath = "/api/sign-in/google";
            }).AddGoogle(q =>
            {
                q.ClientId = config["auth:google:clientId"];
                q.ClientSecret = config["auth:google:clientSecret"];
            });

        services.AddSingleton<IJsonWebTokenManager, JsonWebTokenManager>();

        return services;
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.Auth;

namespace Phaeton.gRPC;

public static partial class Extensions
{
    public static IServiceCollection AddgRPC(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("grpc");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<gRPCOptions>();
        if (!options.Enabled)
            return services;

        services.Configure<AuthOptions>(section);

        services.AddGrpc();

        return services;
    }

    public static WebApplication UsegRPC(
        this WebApplication app,
        Type type
    )
    {

    }
}